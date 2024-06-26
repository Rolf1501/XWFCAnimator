using System;
using System.Linq;
using UnityEngine;

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace XWFC
{
    public class AdjacencyMatrix
    {
        public HashSetAdjacency TileAdjacencyConstraints { get; private set; } // Set of tile adjacency constraints.
        public TileSet TileSet { get; private set; }
        private int _offsetsDimensions; // Set the number of dimensions to operate in.
        public HashSetAdjacency AtomAdjacencyConstraints { get; private set; } // Set of atom adjacency constraints.
        private Vector3Int[] _offsets;
        private Dictionary<Vector3, bool[,]> _tileAdjacencyMatrix; // 2D matrix for tile adjacency constraints per offset. 
        private Dictionary<Vector3, float[,]> _tileAdjacencyMatrixWeights; // 2D matrix for tile adjacency constraint weights per offset.
        private Dictionary<int, int> _tileIdToIndexMapping; // Mapping of parts to their index.
        public Bidict<(int tileId, Vector3 atomCoord, int orientation), int> AtomMapping { get; private set; } // Mapping of atom indices to relative atom coordinate, corresponding terminal id and orientation.
        public Dictionary<Vector3, bool[,]> AtomAdjacencyMatrix { get; private set; }
        private Dictionary<Vector3, float[,]> AtomAdjacencyMatrixW;
        public Dictionary<int, Range> TileAtomRangeMapping { get; private set; } // Reserves an index Range2D for the atoms of the tile.

        public Dictionary<int, float> TileWeigths;
        
        public AdjacencyMatrix(HashSetAdjacency tileAdjacencyConstraints, TileSet tileSet, [CanBeNull] Dictionary<int, float> defaultWeights, int offsetsDimensions = 3)
        {
            
            Init(tileSet, offsetsDimensions);

            TileWeigths = ExpandDefaultWeights(defaultWeights);
            TileAdjacencyConstraints = tileAdjacencyConstraints;
            
            InitTileAdjacency();
            InferTileAdjacencyConstraints(TileAdjacencyConstraints);
            InferAtomAdjacencyConstraints();
        }
        
        public AdjacencyMatrix(TileSet tiles, InputGrid[] grids, [CanBeNull] Dictionary<int, float> tileWeights)
        {
            /*
             * Infer adjacency constraints from input.
             */
            var offsetDimensions = 3;
            Init(tiles, offsetDimensions);
            TileWeigths = ExpandDefaultWeights(tileWeights);
            
            InnerAtomAdjacency();

            /*
             * TODO: go over all grids.
             */
            Debug.Log("Atomized...");

            foreach (var grid in grids)
            {
                var atomizedGrid = AtomizeInputGrid(grid);
                AtomAdjacencyFromGrid(atomizedGrid);
            }

            var builder = new StringBuilder();
            foreach (var (k,v) in AtomAdjacencyMatrix)
            {
                builder.Append($"key: {k}\n");
                for (int i = 0; i < v.GetLength(0); i++)
                {
                    builder.Append($"value: {v}");
                    for (int j = 0; j < v.GetLength(1); j++)
                    {
                        builder.Append($"{AtomAdjacencyMatrix[k][i,j]},");
                    }

                    builder.Append("\n");
                }
            }
            Debug.Log($"{builder}");
            // Process atomized grid.
            Debug.Log("Atom adjacency derived from grid.");
        }
        
        public static Dictionary<int, float> ToWeightDictionary(float[] weights, TileSet tileSet)
        {
            var dictionary = new Dictionary<int, float>();
            int i = 0;
            foreach (var tilesKey in tileSet.Keys)
            {
                dictionary[tilesKey] = weights[i];
                i++;
            }

            return dictionary;
        }
        
        private Dictionary<int, float> ExpandDefaultWeights([CanBeNull] Dictionary<int, float> defaultWeights)
        {
            /*
             * Expands the default weights when the weights are specified per tile. Returns the default weights instead.
             */

            // If the number of weights is neither equal to the number of atoms or the number of terminals, set all weights of all atoms to 1.
            if (defaultWeights == null || defaultWeights.Count != TileSet.Count)
            {
                return Enumerable.Range(0, GetNAtoms()).ToDictionary(k => k, v => 1f);
            }

            // No need to expand if weights are specified per atom already.
            if (defaultWeights.Count == GetNAtoms()) return defaultWeights;

            // Otherwise, expand the weights of the terminals to atoms. 
            var aug = new Dictionary<int, float>();
            foreach (var (k, v) in defaultWeights)
            {
                var range = TileAtomRangeMapping[k];
                for (int i = range.Start; i < range.End; i++)
                {
                    aug[i] = v;
                }
            }

            return aug;
        }
        
        
        public float MaxEntropy()
        {
            
            return CalcEntropy(GetNAtoms());
        }

        public static float CalcEntropy(int nChoices)
        {
            /*
             * Entropy is taken as the log of the number of choices.
             */
            return nChoices > 0 ? (float)Math.Log(nChoices) : -1;
        }

        private void Init(TileSet tileSet, int offsetsDimensions)
        {
            /*
             * Set of actions that must always be performed to initialize the adjacency matrix.
             * Initializes mappings and matrices.
             */
            TileSet = tileSet;
            _offsetsDimensions = offsetsDimensions;
            _offsets = OffsetFactory.GetOffsets(_offsetsDimensions);
            
            AtomAdjacencyConstraints = new HashSetAdjacency();
            AtomMapping = new Bidict<(int, Vector3, int), int>();
            AtomAdjacencyMatrix = new Dictionary<Vector3, bool[,]>();
            AtomAdjacencyMatrixW = new Dictionary<Vector3, float[,]>();
            TileAtomRangeMapping = new Dictionary<int, Range>();
            
            _tileIdToIndexMapping = MapTileIdToIndex(TileSet);
            
            // Create tile to atom Range2D mapping.
            int nAtoms;
            (TileAtomRangeMapping, nAtoms) = MapTileAtomRange(TileSet);
            InitAtomAdjacencyMatrix(nAtoms);
        }

        private void InitTileAdjacency()
        {
            var nTiles = TileSet.Keys.Count;
            _tileAdjacencyMatrix = new Dictionary<Vector3, bool[,]>();
            _tileAdjacencyMatrixWeights = new Dictionary<Vector3, float[,]>();
            foreach (Vector3 offset in _offsets)
            {
                _tileAdjacencyMatrix[offset] = new bool[nTiles, nTiles];
                _tileAdjacencyMatrixWeights[offset] = new float[nTiles, nTiles];
            }
        }

        private void InitAtomAdjacencyMatrix(int nAtoms)
        {
            // Initialize the initial nd arrays.
            foreach (Vector3 offset in _offsets)
            {
                AtomAdjacencyMatrix[offset] = new bool[nAtoms, nAtoms];
                AtomAdjacencyMatrixW[offset] = new float[nAtoms, nAtoms];
            }
        }
        
        private Dictionary<int, int> MapTileIdToIndex(TileSet tiles)
        {
            var mapping = new Dictionary<int, int>();
            var keyArray = tiles.Keys.ToArray();
            for (int i = 0; i < keyArray.Length; i++)
            {
                mapping[keyArray[i]] = i;
            }

            return mapping;
        }

        private AtomGrid AtomizeInputGrid(InputGrid inputGrid)
        {
            var e = inputGrid.GetExtent();
            var atomizedGrid = new AtomGrid(e);
            
            for (int y = 0; y < e.y; y++)
            {
                for (int x = 0; x < e.x; x++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        /*
                         * TODO: support multiple tile ids per cell.
                         */
                        var value = inputGrid.Get(x, y, z);
                        if (value == null || value.Equals(inputGrid.DefaultFillValue)) continue;
                        
                        var coord = new Vector3(x, y, z);

                        var orientation = 0;
                        /*
                         * IDEA:
                         * - Start at 000.
                         * - Try to overlay all tiles on that cell:
                         *      - This means that all atoms for each tile is already checked, meaning that the atoms can already be assigned in this phase.
                         *      - Each cell can host multiple atoms.
                         *      - For each NUT, only consider the occupied entries in the mask.
                         *      - A tile fits iff the coordinate + tile extent is within bounds of the grid. No partial tiles are considered. 
                         *      - As soon as at least one atom does not match, consider the placement invalid.
                         * - This is essentially a sliding window approach, brute force.
                         * - Though, it all happens in pre computation and is negligible compared to running WFC.
                         */
                        foreach (var (tileId, tile) in TileSet)
                        {
                            var maxCoord = coord + tile.Extent - new Vector3(1,1,1); // Must subtract 1, index starts at 0.
                            if (!inputGrid.WithinBounds(maxCoord)) continue;
                            var valid = true;
                            // Check if atom values match those present in the grid.
                            foreach (var atomCoord in tile.OrientedIndices[orientation])
                            {
                                var gridCoord = atomCoord + coord;
                                var atomGridValue = inputGrid.Get(gridCoord);
                                var atomTileValue = tile.GetAtomValue(atomCoord);

                                // Make sure to exclude cases where a cell can contain the same tile twice. This must happen in the output and may result in partial tile placement.
                                var cellContainsTile = atomizedGrid.Get(gridCoord).Count(v => v >= TileAtomRangeMapping[tileId].Start && v < TileAtomRangeMapping[tileId].End) > 0;
                                if (
                                    !atomTileValue.Equals(inputGrid.DefaultFillValue)
                                    && atomGridValue != null
                                    && atomGridValue.Equals(atomTileValue)
                                    && !cellContainsTile
                                    )
                                {
                                    continue;
                                }
                                valid = false;
                                break;
                            }

                            if (!valid) continue;
                            
                            // If tile placement is a match, then add the atom ids to the corresponding grid cells.
                            foreach (var atomCoord in tile.OrientedIndices[orientation])
                            {
                                var gridCoord = atomCoord + coord;
                                var atomId = AtomMapping.GetValue((tileId, atomCoord, orientation));
                                atomizedGrid.Get(gridCoord).Add(atomId);
                            }
                        }
                        
                    }
                }
            }
            
            Debug.Log(atomizedGrid.ToString());

            return atomizedGrid;
        }

        private void AtomAdjacencyFromGrid(AtomGrid atomGrid)
        {
            var e = atomGrid.GetExtent();
            var positiveOffsets = (from offset in _offsets where offset is { x: >= 0, y: >= 0, z: >= 0 } select offset).ToList();
            for (int y = 0; y < e.y; y++)
            {
                for (int x = 0; x < e.x; x++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        var coord = new Vector3(x, y, z);
                        var atoms = atomGrid.Get(coord);
                        
                        if (atoms == null || atoms.Count == 0) continue;
                        
                        var atomId = atomGrid.Get(coord)[0]; // TODO: consider all atoms in the list.
                        var (tileId, atomCoord, orientation) = AtomMapping.GetKey(atomId);
                        var tileAtoms = TileSet[tileId].OrientedIndices[orientation];
                        
                        if (!AtomMapping.ContainsValue(atomId)) continue;
                        
                        foreach (var offset in positiveOffsets)
                        {
                            var otherCoord = coord + offset;
                            if (!atomGrid.WithinBounds(otherCoord)) continue;

                            var others = atomGrid.Get(otherCoord);
                            if (others == null || others.Count == 0) continue;

                            var innerAtomCoord = atomCoord + offset;
                            
                            // Atoms belong to same tile.
                            if (tileAtoms.Contains(innerAtomCoord))
                            {
                                // Must follow inner atom adjacency constraints.
                                var otherId = AtomMapping.GetValue((tileId, innerAtomCoord, orientation));
                                SetAtomAdjacency(atomId, otherId, offset);
                            }
                            else
                            {
                                // Atoms belong to distinct tiles.
                                
                                var complement = Vector3Util.Negate(offset);
                                // If there are no inner atom adjacency constraints to be enforced, consider all other atom ids.
                                foreach (var otherAtomId in others)
                                {
                                    // In case this atom and other atom belong to two distinct tiles, make sure not to include atoms of the other tile if that causes the presence of partial tiles.
                                    // i.e. exclude all other atoms who have an atom. this can be done by checking the other potential atoms ids in this cell and excluding any in the others
                                    var (otherTileId, otherAtomCoord, otherOrientation) = AtomMapping.Get(otherAtomId);
                                    var otherAtoms = TileSet[otherTileId].OrientedIndices[otherOrientation];
                                    
                                    // Do not allow adjacency between atoms who expect an inner atom adjacency.
                                    if (otherAtoms.Contains(otherAtomCoord + complement)) continue;
                                    
                                    SetAtomAdjacency(atomId, otherAtomId, offset);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetAtomAdjacency(int thisAtomId, int thatAtomId, Vector3 offset)
        {
            AtomAdjacencyMatrix[offset][thisAtomId, thatAtomId] = true;
            AtomAdjacencyMatrixW[offset][thisAtomId, thatAtomId] = 1;

            // Adjacency constraints are symmetric.
            var complement = Vector3Util.Negate(offset);
            AtomAdjacencyMatrix[complement][thatAtomId, thisAtomId] = true;
            AtomAdjacencyMatrixW[complement][thatAtomId, thisAtomId] = 1;
        }
        
        public int GetNAtoms()
        {
            return AtomMapping.GetNEntries();
        }

        private static int CalcAtomRange(Tile tile)
        {
            return tile.NAtoms;
        }
        private void InferAtomAdjacencyConstraints()
        {
            InnerAtomAdjacency(); // With atoms from within a molecule.
            OuterAtomAdjacency(); // With atoms from another molecule.
        }

        private (Dictionary<int, Range> mapping, int nAtoms) MapTileAtomRange(TileSet tiles)
        {
            int nAtoms = 0;
            var mapping = new Dictionary<int, Range>();
            // Map each terminal to a range in the mapping list.
            foreach (int tileId in tiles.Keys)
            {
                Tile t = TileSet[tileId];
                int tAtoms = CalcAtomRange(t);
                mapping[tileId] = new Range(nAtoms, nAtoms + tAtoms);
                nAtoms += tAtoms;
            }

            return (mapping, nAtoms);
        }

        private void InnerAtomAdjacency()
        {
            /*
             * Formalizes the atom atom adjacency constraints within a molecule.
             */
            foreach (int p in TileSet.Keys.ToArray())
            {
                Tile t = TileSet[p];
                CreatePartToIndexEntry(p, t);

                foreach (int d in t.DistinctOrientations)
                {
                    for (int i = 0; i < t.OrientedIndices[d].Length; i++)
                    {
                        Vector3 atomIndex = t.OrientedIndices[d][i];
                        int thisIndex = AtomMapping.Get((p, atomIndex, d));
                        foreach (Vector3 offset in _offsets)
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

        private void CreatePartToIndexEntry(int tileId, Tile t)
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
                    AtomMapping.AddPair((tileId, index, d), newKeyEntry);
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
            Tile thisT = TileSet[thisId];
            Tile thatT = TileSet[thatId];
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

        private static T[,] SelectRegion2D<T>(T[,] matrix, Range2D Range2D)
        {
            var output = new T[Range2D.GetYLength(), Range2D.GetXLength()];
            for (int i = 0; i < Range2D.GetYLength(); i++)
            {
                for (int j = 0; j < Range2D.GetXLength(); j++)
                {
                    output[i, j] = matrix[Range2D.YRange.Start + i, Range2D.XRange.Start + j];
                }
            }

            return output;
        }

        private int[] FlattenedSliceMask(Range2D Range2D, int[,] mask)
        // private int[] FlattenedSliceMask(Range2D Range2D, Span2D<int> mask)
        {
            // var sliced = mask.Slice(Range2D.YStart, Range2D.XStart, Range2D.GetYLength(), Range2D.GetXLength());
            int[,] sliced = SelectRegion2D(mask, Range2D);
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
            if (baseSlice.Length == 0 || sliderSlice.Length == 0) return -1;
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

            var (up, looking) = VoidMasks.CalcUpAndLookingDirections(offsetDirectionIndex, false);
            
            int sign = offset[offsetDirectionIndex] < 0 ? -1 : 1;
            
            // Map the base coordinate to be the relative coordinate to the origin of the slider.
            // This is needed for inferring which specific atoms are touching.
            var sliderMinPositionFromBase = GetSliderMinPosition(sliderData.ShapeXyz, baseData.ShapeXyz, configOffset, offset, sign);
            
                             
            for (int y = 0; y < shifts.y; y++)
            {
                // Find the overlapping regions of the overlaid void void masks in up-direction (i.e. relative Y).
                var (startYSlider, endYSlider) = CalcSliderRange2D(sliderData.MaxY, baseData.MaxY, y);
                var (startYBase, endYBase) = CalcBaseRange2D(baseData.MaxY, startYSlider, endYSlider, y);

                for (int x = 0; x < shifts.x; x++)
                {
                    // Find the overlapping regions of the overlaid void void masks in looking-direction (i.e. relative X).
                    var (startXSlider, endXSlider) = CalcSliderRange2D(sliderData.MaxX, baseData.MaxX, x);
                    var (startXBase, endXBase) = CalcBaseRange2D(baseData.MaxX, startXSlider, endXSlider, x);
                    
                    var sliderRange2D = new Range2D(startXSlider, endXSlider, startYSlider, endYSlider);
                    var sliderSlice = FlattenedSliceMask(sliderRange2D, sliderData.VoidMask);
                    
                    var baseRange2D = new Range2D(startXBase, endXBase, startYBase, endYBase);
                    var baseSlice = FlattenedSliceMask(baseRange2D, baseData.VoidMask);
                    
                    int minSum = CalcMinSum(baseSlice, baseData.ShapeXyz[offsetDirectionIndex], 
                        sliderSlice, sliderData.ShapeXyz[offsetDirectionIndex]);

                    if (minSum < 0)
                    {
                        continue; 
                    }

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
                        
                        if (!baseData.Tile.ContainsAtom(baseData.Orientation, baseAtomCoord)) continue;
                        
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
            foreach (Vector3 io in _offsets)
            {
                Vector3 relativeOffset = baseAtomCoord + io;
                            
                // Find the vector pointing to the relative slider atom coordinate.
                Vector3 sliderAtomCoord = relativeOffset - relativeSliderPosition;

                if (!sliderData.Tile.WithinBounds(sliderAtomCoord)) continue;
                            
                // Bounds check implicitly done with contains atom. If the mapped position is not an atom, continue.
                if (!sliderData.Tile.ContainsAtom(sliderData.Orientation, sliderAtomCoord)) continue;

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

        private static (int, int) CalcSliderRange2D(int maxSlider, int maxBase, int shift)
        {
            /*
            Calculates the slider sliding window Range2D, for a given shift d.
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
            +1 is to compensate for numpy Range2D handling.
            */
            int end = Math.Min(maxSlider, maxSlider + maxBase - shift) + 1;
            return (start, end);
        }

        private static (int, int) CalcBaseRange2D(int maxBase, int startSlider, int endSlider, int shift)
        {
            /*
            Calculates the base sliding window Range2D, for a given shift d.
            Slider window Range2D is used to infer base window Range2D, since the two should not differ in length.
            Stays 0 until y is larger than the slider, at which point the base start should start to increase as well.

            Do not exceed the slider window's bounds.
            When the slider's coverage is smaller than the base window y, the base window should not exceed the y length of the slider.
            Note that the second argument does not contain +1, since this is already incorporated in the end slider Range2D.
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
                int sourceIndex = _tileIdToIndexMapping[a.Source];
                foreach (Relation r in a.Relations)
                {
                    int relationIndex = _tileIdToIndexMapping[r.Other];
                    _tileAdjacencyMatrix[a.Offset][sourceIndex, relationIndex] = true;
                    _tileAdjacencyMatrix[complement][sourceIndex, relationIndex] = true;
                    _tileAdjacencyMatrixWeights[a.Offset][sourceIndex, relationIndex] = r.Weight;
                    _tileAdjacencyMatrixWeights[complement][sourceIndex, relationIndex] = r.Weight;
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

        public Tile GetTerminalFromAtomId(int atomId)
        {
            return TileSet[AtomMapping.Get(atomId).Item1];
        }

        public HashSet<int> GetEmptyAtomIds()
        {
            var emptyAtomIds = new HashSet<int>();
            foreach (var (id, tile) in TileSet)
            {
                if (!tile.IsEmptyTile) continue;
                
                var range = TileAtomRangeMapping[id];
                for (int i = range.Start; i < range.End; i++)
                {
                    emptyAtomIds.Add(i);
                }
            }

            return emptyAtomIds;
        }
    }

    public record VmData
    {
        public int Orientation { get; }
        public Tile Tile { get; }
        public int TerminalId { get; }
        public int[] ShapeXyz { get; }
        public int[,] VoidMask { get; }
        public int MaxY { get; }
        public int MaxX { get; }
        // public int MaxDepth { get; }
        public Vector2 VmShapeVector { get; }

        public VmData(VoidMaskAdjacencyData vmData, int rotation)
        {
            Tile = vmData.Tile;
            TerminalId = vmData.TerminalId;
            Orientation = Tile.GetRotationOrientation(rotation);
            var vm = Tile.OrientedMask[Orientation];
            ShapeXyz = new int[] { vm.GetLength(1), vm.GetLength(0), vm.GetLength(2) };

            VoidMask = vmData.VoidMasks[Orientation];
            VmShapeVector = new Vector2(VoidMask.GetLength(1), VoidMask.GetLength(0));
            MaxY = VoidMask.GetLength(0) - 1;
            MaxX = VoidMask.GetLength(1) - 1;
        }
    }

    public class AdjacencyMatrixJsonFormatter
    {
        private HashSetAdjacency _tileAdjacencyConstraints;
        private TileSet _tiles;
        public AdjacencyMatrixJsonFormatter(HashSetAdjacency tileAdjacencyConstraints, TileSet tileSet)
        {
            _tileAdjacencyConstraints = tileAdjacencyConstraints;
            _tiles = tileSet;
        }
        public string ToJson()
        {
            var dict = new Dictionary<string, string>();
            var tileString = new Dictionary<string, string>();
            foreach (var (k,v) in _tiles)
            {
                tileString[k.ToString()] = JsonConvert.SerializeObject(v.ToJson());
            }
            
            dict["tileset"] = JsonConvert.SerializeObject(tileString);
            dict["tileAdjacencyConstraints"] = _tileAdjacencyConstraints.ToJson();
            
            return JsonConvert.SerializeObject(dict);
        }

        public static AdjacencyMatrix FromJson(string s)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
            var tilesetString = dict["tileset"];
            var tileset = TileSet.FromJson(tilesetString);
            var adjConstraintsString = dict["tileAdjacencyConstraints"];
            var adjConstraints = HashSetAdjacency.FromJson(adjConstraintsString);

            return new AdjacencyMatrix(adjConstraints, tileset, null);
        }
    }
}