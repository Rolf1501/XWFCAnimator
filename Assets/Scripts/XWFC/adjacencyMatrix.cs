using System;
using System.Linq;
using UnityEngine;

using System.Collections.Generic;

namespace XWFC
{
    public class HashSetAdjacency : HashSet<Adjacency> { } // Class for shorthand notation of set of adjacency constraints.

    public class AdjacencyMatrix
    {
        private int[] TileIds { get; }
        public HashSetAdjacency TileAdjacencyConstraints { get; } // Set of tile adjacency constraints.
        private TileSet TileSet { get; }
        private int OffsetsDimensions { get; } // Set the number of dimensions to operate in.
        public HashSetAdjacency AtomAdjacencies { get; private set; } // Set of atom adjacency constraints.
        private Vector3[] Offsets { get; }
        private Dictionary<Vector3, bool[,]> TileAdjacencyMatrix { get; } // 2D matrix for tile adjacency constraints per offset. 
        private Dictionary<Vector3, float[,]> TileAdjacencyMatrixWeights { get; } // 2D matrix for tile adjacency constraint weights per offset.
        private Dictionary<int, int> TileToIndexMapping { get; } // Mapping of parts to their index.
        public Bidict<(int, Vector3, int), int> AtomMapping { get; } // Mapping of atom indices to relative atom coordinate, corresponding terminal id and orientation.
        public Dictionary<Vector3, bool[,]> AtomAdjacencyMatrix { get; }
        private Dictionary<Vector3, float[,]> AtomAdjacencyMatrixW { get; }
        public Dictionary<int, (int, int)> TileAtomRangeMapping { get; } // Reserves an index range for the atoms of the tile.
        public Dictionary<int, HashSetAdjacency> TileAdjacencyMapping { get; private set; }

        public AdjacencyMatrix(int[] tileIds, HashSetAdjacency tileAdjacencyConstraints, TileSet tileSet, int offsetsDimensions = 3)
        {
            TileIds = tileIds;
            TileAdjacencyConstraints = tileAdjacencyConstraints;
            TileSet = tileSet;
            OffsetsDimensions = offsetsDimensions;
            AtomAdjacencies = new HashSetAdjacency();
            AtomMapping = new Bidict<(int, Vector3, int), int>();
            AtomAdjacencyMatrix = new Dictionary<Vector3, bool[,]>();
            AtomAdjacencyMatrixW = new Dictionary<Vector3, float[,]>();
            TileAtomRangeMapping = new Dictionary<int, (int, int)>();
            TileAdjacencyMapping = new Dictionary<int, HashSetAdjacency>();

            int nTiles = TileIds.Length;
            TileToIndexMapping = new Dictionary<int, int>();
            for (int i = 0; i < nTiles; i++)
            {
                TileToIndexMapping.Add(TileIds[i], i);
            }

            Offsets = OffsetFactory.GetOffsets(OffsetsDimensions);
            InferAtomAdjacencies();

            TileAdjacencyMatrix = new Dictionary<Vector3, bool[,]>();
            TileAdjacencyMatrixWeights = new Dictionary<Vector3, float[,]>();
            foreach (Vector3 offset in Offsets)
            {
                TileAdjacencyMatrix[offset] = new bool[nTiles, nTiles];
                TileAdjacencyMatrixWeights[offset] = new float[nTiles, nTiles];
            }
            InferTileAdjacencyConstraints(TileAdjacencyConstraints);
        }

        // TODO: implement this for showing adjs in GUI.
        // public Dictionary<int, HashSetAdjacency> GetAllPartAdjacenciesAsId()
        // {
        //     return null;
        // }

        public int GetNAtoms()
        {
            return AtomMapping.GetNEntries();
        }

        private static int CalcAtomRange(Terminal terminal)
        {
            return terminal.NAtoms;
        }
        private void InferAtomAdjacencies()
        {
            int nAtoms = 0;

            // Map each terminal to a range in the mapping list.
            foreach (int p in TileIds)
            {
                Terminal t = TileSet[p];
                int tAtoms = CalcAtomRange(t);
                TileAtomRangeMapping[p] = (nAtoms, nAtoms + tAtoms);
                nAtoms += tAtoms;
            }

            // Initialize the initial nd arrays.
            foreach (Vector3 offset in Offsets)
            {
                AtomAdjacencyMatrix[offset] = new bool[nAtoms, nAtoms];
                AtomAdjacencyMatrixW[offset] = new float[nAtoms, nAtoms];
            }

            InnerAtomAdjacency(); // With atoms from within a molecule.
            OuterAtomAdjacency(); // With atoms from another molecule.
        }

        private void InnerAtomAdjacency()
        {
            /*
             * Formalizes the atom atom adjacencies within a molecule.
             */
            foreach (int p in TileIds)
            {
                Terminal t = TileSet[p];
                CreatePartToIndexEntry(p, t);

                foreach (int d in t.DistinctOrientations)
                {
                    for (int i = 0; i < t.OrientedIndices[d].Length; i++)
                    {
                        Vector3 atomIndex = t.OrientedIndices[d][i];
                        int thisIndex = AtomMapping.Get((p, atomIndex, d));
                        foreach (Vector3 offset in Offsets)
                        {
                            if (!t.ContainsAtom(d, atomIndex + offset)) continue;
                            int otherIndex = AtomMapping.Get((p, atomIndex + offset, d));

                            // Update the entries, operation is symmetric.
                            AtomAdjacencyMatrix[offset][thisIndex, otherIndex] = true;
                            AtomAdjacencyMatrix[Vector3Util.Negate(offset)][otherIndex, thisIndex] = true;
                            AtomAdjacencyMatrixW[offset][thisIndex, otherIndex] = 1.0f;
                            AtomAdjacencyMatrixW[Vector3Util.Negate(offset)][otherIndex, thisIndex] = 1.0f;
                        }
                    }
                }
            }
        }

        private void CreatePartToIndexEntry(int partId, Terminal t)
        {
            /*
             * Append entries mapping an index to the corresponding terminal info.
             */

            foreach (int d in t.DistinctOrientations)
            {
                for (int i = 0; i < t.OrientedIndices[d].Length; i++)
                {
                    Vector3 index = t.OrientedIndices[d][i];
                    int newKeyEntry = AtomMapping.GetNEntries();
                    AtomMapping.AddPair((partId, index, d), newKeyEntry);
                }
            }
        }

        private void OuterAtomAdjacency()
        {
            /*
             * Formalizes the atom atom adjacency constraints of atoms belonging to different tiles.
             */
            foreach (Adjacency a in TileAdjacencyConstraints)
            {
                foreach (Relation r in a.Relations)
                {
                    InferAtomAdjacencyWithVoidMask(a.Source, r.Other, a.Offset, r.Weight, r.Rotations);
                }
            }
        }

        private void InferAtomAdjacencyWithVoidMask(int thisId, int thatId, Vector3 offset, float weight, int[] thatRotations)
        {
            /*
             * Slides one of the two tiles along the other tile at the given offset.
             * Uses void masks to infer the offset required in case of concave tiles. 
             */
            var (ox, oy, oz) = Vector3Util.CastInt(offset);
            int offsetDirectionIndex = Array.FindIndex(new int[] { ox, oy, oz }, e => e != 0);
            Terminal thisT = TileSet[thisId];
            Terminal thatT = TileSet[thatId];
            var thisVmAdj = new VoidMaskAdjacencyData(
                thisT,
                thisId,
                offset,
                thisT.DistinctOrientations,
                offsetDirectionIndex
            );
            var thatVmAdj = new VoidMaskAdjacencyData(
                thatT,
                thatId,
                Vector3Util.Negate(offset),
                thatRotations,
                offsetDirectionIndex
            );

            // By selecting the smaller void mask as the base, the number of checks for touching surfaces is reduced.
            VoidMaskAdjacencyData baseVmAdj = thisVmAdj;
            VoidMaskAdjacencyData sliderVmAdj = thatVmAdj;
            if (thisVmAdj.GetVolume() > thatVmAdj.GetVolume())
            {
                baseVmAdj = thatVmAdj;
                sliderVmAdj = thisVmAdj;
                // Switch the offset, since the base and slider are swapped compared to the input.
                offset = Vector3Util.Negate(offset);
            }

            foreach (int br in baseVmAdj.Rotations)
            {
                var baseAux = new VmData(baseVmAdj, br);

                foreach (int sr in sliderVmAdj.Rotations)
                {
                    var sliderAux = new VmData(sliderVmAdj, sr);

                    Vector2 shifts = CalcShifts(baseAux.VmShapeVector, sliderAux.VmShapeVector);

                    PerformShifts(baseAux, sliderAux, shifts, offset, offsetDirectionIndex, weight);
                }

            }
        }

        private Vector3 GetSliderMinPosition(int[] sliderShape, int[] baseShape, Vector3 configOffset, Vector3 offset, int sign)
        {
            /*
             * To find slider coord relative to base, take the minimal position as a starting point, then add the required offsets.
             */
            
            // Relative to base, slider min point is always equal to shape of slider.
            var sliderMinPositionFromBase = new Vector3(-sliderShape[0], -sliderShape[1], -sliderShape[2]); 
            sliderMinPositionFromBase += configOffset; // Need to offset to ensure a configuration where overlap could occur.
            
            // Need additional offset of base if in positive direction.
            if (sign > 0)
                sliderMinPositionFromBase += Vector3Util.Mult(offset,new Vector3(
                    sliderShape[0] + baseShape[0], 
                    sliderShape[1] + baseShape[1], 
                    sliderShape[2] + baseShape[2]));

            return sliderMinPositionFromBase;
        }

        private static T[,] SelectRegion2D<T>(T[,] matrix, Range range)
        {
            var output = new T[range.GetYLength(), range.GetXLength()];
            for (int i = 0; i < range.GetYLength(); i++)
            {
                for (int j = 0; j < range.GetXLength(); j++)
                {
                    output[i, j] = matrix[range.YStart + i, range.XStart + j];
                }
            }

            return output;
        }

        private int[] FlattenedSliceMask(Range range, int[,] mask)
        // private int[] FlattenedSliceMask(Range range, Span2D<int> mask)
        {
            // var sliced = mask.Slice(range.YStart, range.XStart, range.GetYLength(), range.GetXLength());
            int[,] sliced = SelectRegion2D(mask, range);
            // var maskArr = new int[mask.Length];
            // sliced.CopyTo(maskArr);
            int[] maskArr = Flatten(sliced);
            return maskArr;
        }

        private T[] Flatten<T>(T[,] target)
        {
            return target.Cast<T>().ToArray();
        }

        private int CalcMinSum(int[] baseSlice, int baseMaxDepth, int[] sliderSlice, int sliderMaxDepth)
        {
            // There cannot be an adjacency when all layers in either the base or slider are empty.
            if (baseSlice.Min() > baseMaxDepth - 1 || sliderSlice.Min() > sliderMaxDepth - 1) return -1;
            
            // Find the minimum sum of the overlaid masks.
            return baseSlice.Zip(sliderSlice, (a, b) => a + b).Min();
        }

        private void PerformShifts(
            VmData baseData,
            VmData sliderData,
            Vector2 shifts,
            Vector3 offset,
            int offsetDirectionIndex,
            float weight)
        {
            /*
             * Performs the shifting of a slider along a base and records the encountered atom adjacency pairs as constraints.
             */
            
            // Offset required from the base position to ensure a possibility of a valid configuration.
            var configOffset = new Vector3(1, 1, 1)
            {
                [offsetDirectionIndex] = 0
            };

            var (up, looking) = Terminal.CalcUpAndLookingDirections(offsetDirectionIndex, false);
            
            int sign = offset[offsetDirectionIndex] < 0 ? -1 : 1;
            
            // Map the base coordinate to be the relative coordinate to the origin of the slider.
            // This is needed for inferring which specific atoms are touching.
            var sliderMinPositionFromBase = GetSliderMinPosition(sliderData.ShapeXyz, baseData.ShapeXyz, configOffset, offset, sign);
            
                             
            for (int y = 0; y < shifts.y; y++)
            {
                // Find the overlapping regions of the overlaid void void masks in up-direction (i.e. relative Y).
                var (startYSlider, endYSlider) = CalcSliderRange(sliderData.MaxY, baseData.MaxY, y);
                var (startYBase, endYBase) = CalcBaseRange(baseData.MaxY, startYSlider, endYSlider, y);

                for (int x = 0; x < shifts.x; x++)
                {
                    // Find the overlapping regions of the overlaid void void masks in looking-direction (i.e. relative X).
                    var (startXSlider, endXSlider) = CalcSliderRange(sliderData.MaxX, baseData.MaxX, x);
                    var (startXBase, endXBase) = CalcBaseRange(baseData.MaxX, startXSlider, endXSlider, x);
                    
                    var sliderRange = new Range(startXSlider, endXSlider, startYSlider, endYSlider);
                    var sliderSlice = FlattenedSliceMask(sliderRange, sliderData.VoidMask);
                    
                    var baseRange = new Range(startXBase, endXBase, startYBase, endYBase);
                    var baseSlice = FlattenedSliceMask(baseRange, baseData.VoidMask);
                    
                    int minSum = CalcMinSum(baseSlice, baseData.ShapeXyz[offsetDirectionIndex], 
                        sliderSlice, sliderData.ShapeXyz[offsetDirectionIndex]);

                    Vector3 relativeSliderPosition = RelativeSliderPosition(
                        sliderMinPositionFromBase, x, y, minSum, sign, offsetDirectionIndex, up, looking);
                    
                    CalcAdjacentAtoms(sliderData, baseData, relativeSliderPosition, weight);
                }
            }
        }

        private void CalcAdjacentAtoms(VmData sliderData, VmData baseData, Vector3 relativeSliderPosition, float weight)
        {
            // For each atom in the base, check if it is adjacent to one of the atoms in the slider.
            for (int iy = 0; iy < baseData.ShapeXyz[1]; iy++)
            {
                for (int ix = 0; ix < baseData.ShapeXyz[0]; ix++)
                {
                    for (int iz = 0; iz < baseData.ShapeXyz[2]; iz++)
                    {
                        var baseAtomCoord = new Vector3(ix, iy, iz);
                        
                        if (!baseData.Terminal.ContainsAtom(baseData.Orientation, baseAtomCoord)) continue;
                        
                        int baseAtomIndex = AtomMapping.Get((baseData.TerminalId, baseAtomCoord, baseData.Orientation));

                        // TODO potential optimization: A base atom can only form an adjacency constraint with a slider atom, if the base atom has no inner atom adjacency constraints.
                        
                        // For each offset, check if the base atom touches any slider atom.
                        var sliderAtomIndices = CalcSliderAtomIndices(sliderData, baseAtomCoord, relativeSliderPosition);
                        foreach (var (index, o) in sliderAtomIndices)
                        {
                            UpdateAtomAdjacency(baseAtomIndex, index, o, weight);
                        }
                    }
                }
            }
        }

        private IEnumerable<(int sliderAtomIndex, Vector3 io)> CalcSliderAtomIndices(VmData sliderData, Vector3 baseAtomCoord, Vector3 relativeSliderPosition)
        {
            foreach (Vector3 io in Offsets)
            {
                Vector3 relativeOffset = baseAtomCoord + io;
                            
                // Find the vector pointing to the relative slider atom coordinate.
                Vector3 sliderAtomCoord = relativeOffset - relativeSliderPosition;

                if (!sliderData.Terminal.WithinBounds(sliderAtomCoord)) continue;
                            
                // Bounds check implicitly done with contains atom. If the mapped position is not an atom, continue.
                if (!sliderData.Terminal.ContainsAtom(sliderData.Orientation, sliderAtomCoord)) continue;

                int sliderAtomIndex = AtomMapping.Get((sliderData.TerminalId, sliderAtomCoord, sliderData.Orientation));

                yield return (sliderAtomIndex, io);
            }
        }

        private Vector3 RelativeSliderPosition(Vector3 minSliderPosition, int shiftX, int shiftY, int minSum, int sign, int offsetDirectionIndex, int up, int looking)
        {
            /*
             * Find translation from base origin to slider origin. This is the slider's position relative to the base.
             * With that coordinate, we can find the relative coordinates of the slider atoms.
             */
            // Account for the shifts.
            var relativeSliderPosition = minSliderPosition; // Account for the shifts.
            relativeSliderPosition[up] += shiftY;
            relativeSliderPosition[looking] += shiftX;
                    
            // Account for the minsum. If sign is positive, we need to inset one. Otherwise, we need to offset one.
            relativeSliderPosition[offsetDirectionIndex] += minSum * -sign;

            return relativeSliderPosition;
        }
        private bool UpdateAtomAdjacency(int thisIndex, int thatIndex, Vector3 offset, float weight)
        {
            Vector3 complement = Vector3Util.Negate(offset);
            AtomAdjacencyMatrix[offset][thisIndex, thatIndex] = true;
            AtomAdjacencyMatrix[complement][thatIndex, thisIndex] = true;
            AtomAdjacencyMatrixW[offset][thisIndex, thatIndex] = weight;
            AtomAdjacencyMatrixW[complement][thatIndex, thisIndex] = weight;
            return true;
        }

        private static (int, int) CalcSliderRange(int maxSlider, int maxBase, int shift)
        {
            /*
            Calculates the slider sliding window range, for a given shift d.
            Do not exceed the slider window's bounds.
            When the slider's coverage is smaller than the base window y, the base window should not exceed the y length of the slider.

            There are two main cases: (1) d <= max_d_slider and (2) d > max_d_slider.
            (1) indicates the case where the overlap of the two windows is a subset of the base.
            (2) indicates the moment when the base index >= sliding starting index.
            In case (1), max_d_slider - d is positive and corresponds to the first index where the sliding window moves over the base window.
            In case (2), the former would be negative, which outside the bounds. Hence, cap it off at 0.
            */
            int start = Math.Max(0, maxSlider - shift);
            /*
            Same cases as before. In case (1), the last index of the slider still overlaps with an index of the base. Hence, the cap.
            In case (2), the length of the slider window should not exceed the length of the base.
            +1 is to compensate for numpy range handling.
            */
            int end = Math.Min(maxSlider, maxSlider + maxBase - shift) + 1;
            return (start, end);
        }

        private static (int, int) CalcBaseRange(int maxBase, int startSlider, int endSlider, int shift)
        {
            /*
            Calculates the base sliding window range, for a given shift d.
            Slider window range is used to infer base window range, since the two should not differ in length.
            Stays 0 until y is larger than the slider, at which point the base start should start to increase as well.

            Do not exceed the slider window's bounds.
            When the slider's coverage is smaller than the base window y, the base window should not exceed the y length of the slider.
            Note that the second argument does not contain +1, since this is already incorporated in the end slider range.
            */
            int start = Math.Max(shift - endSlider, 0);
            int end = Math.Min(maxBase + 1, start + endSlider - startSlider);
            return (start, end);
        }

        private static Vector2 CalcShifts(Vector2 thisShape, Vector2 thatShape)
        {
            return thisShape + thatShape - new Vector2(1, 1);
        }

        private void InferTileAdjacencyConstraints(HashSetAdjacency tileAdjacencies)
        {
            // Implement the logic for allowing adjacencies
            foreach (Adjacency a in tileAdjacencies)
            {
                Vector3 complement = Vector3Util.Negate(a.Offset);
                int sourceIndex = TileToIndexMapping[a.Source];
                foreach (Relation r in a.Relations)
                {
                    int relationIndex = TileToIndexMapping[r.Other];
                    TileAdjacencyMatrix[a.Offset][sourceIndex, relationIndex] = true;
                    TileAdjacencyMatrix[complement][sourceIndex, relationIndex] = true;
                    TileAdjacencyMatrixWeights[a.Offset][sourceIndex, relationIndex] = r.Weight;
                    TileAdjacencyMatrixWeights[complement][sourceIndex, relationIndex] = r.Weight;

                }
            }
        }

        private IEnumerable<T> GetRow<T>(T[,] matrix, int row)
        {
            for (int i = 0; i < matrix.GetLength(1); i++)
                yield return matrix[row, i];
        }

        public bool[] GetAdj(Vector3 offset, int choiceId)
        {
            // Span2D<bool> span = AtomAdjacencyMatrix[offset];
            // return span.GetRowSpan(choiceId).ToArray();
            var span = AtomAdjacencyMatrix[offset];
            return GetRow(span, choiceId).ToArray();
        }
        public float[] GetAdjW(Vector3 offset, int choiceId)
        {
            // Span2D<float> span = AtomAdjacencyMatrixW[offset];
            // return span.GetRowSpan(choiceId).ToArray();
            var span = AtomAdjacencyMatrixW[offset];
            return GetRow(span, choiceId).ToArray();
        }

        public Terminal GetTerminalFromAtomId(int atomId)
        {
            return TileSet[AtomMapping.Get(atomId).Item1];
        }
    }

    public record Range
    {
        public int XStart { get; }
        public int XEnd { get; }
        public int YStart { get; }
        public int YEnd { get; }

        public Range(int xStart, int xEnd, int yStart, int yEnd)
        {
            XStart = xStart;
            XEnd = xEnd;
            YStart = yStart;
            YEnd = yEnd;
            if (XStart > XEnd || YStart > YEnd) throw new Exception("Range start must not be larger than end.");
        }

        public int GetYLength()
        {
            return YEnd - YStart;
        }
        
        public int GetXLength()
        {
            return XEnd - XStart;
        }
    }
    public record VmData
    {
        public int Orientation { get; }
        public Terminal Terminal { get; }
        public int TerminalId { get; }
        public int[] ShapeXyz { get; }
        public int[,] VoidMask { get; }
        public int MaxY { get; }
        public int MaxX { get; }
        // public int MaxDepth { get; }
        public Vector2 VmShapeVector { get; }

        public VmData(VoidMaskAdjacencyData vmData, int rotation)
        {
            Terminal = vmData.Terminal;
            TerminalId = vmData.TerminalId;
            Orientation = Terminal.GetRotationOrientation(rotation);
            var vm = Terminal.OrientedMask[Orientation];
            ShapeXyz = new int[] { vm.GetLength(1), vm.GetLength(0), vm.GetLength(2) };

            VoidMask = vmData.VoidMasks[Orientation];
            VmShapeVector = new Vector2(VoidMask.GetLength(1), VoidMask.GetLength(0));
            MaxY = VoidMask.GetLength(0) - 1;
            MaxX = VoidMask.GetLength(1) - 1;
        }
    }
}