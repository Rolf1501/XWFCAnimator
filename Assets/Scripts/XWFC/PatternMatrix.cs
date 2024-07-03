using UnityEngine;
using System.Collections.Generic;
using AtomMapping = XWFC.Bidict<(int tileId, UnityEngine.Vector3Int atomCoord, int orientation),int>;
namespace XWFC
{
    public class PatternMatrix
    {
        public Dictionary<int, Bidict<Vector3Int, HashSet<int>>> AtomPatternMapping;
        public List<int[,,]> Patterns;
        private readonly AtomGrid[] _atomizedSamples;
        private readonly Vector3Int _kernelSize;
        public readonly  AtomMapping AtomMapping;
        private Dictionary<Vector3Int, Range3D> _offsetRangeMapping;
        private IEnumerable<Vector3Int> _offsets;
        public Dictionary<Vector3Int, bool[,]> PatternAdjacencyMatrix;


        public PatternMatrix(AtomGrid[] atomizedSamples, Vector3Int kernelSize, AtomMapping atomMapping)
        {
            _atomizedSamples = atomizedSamples;
            _kernelSize = kernelSize;
            AtomMapping = atomMapping;
            _offsets = OffsetFactory.GetOffsets(3);
            Patterns = new List<int[,,]>();
            PatternAdjacencyMatrix = new Dictionary<Vector3Int, bool[,]>();
            InitMapping();
            CalcPatterns();
            InitPatternMatrix();
            CalcLayerRangesPerOffset();
            CalcPatternAdjacency();
        }

        private void CalcLayerRangesPerOffset()
        {
            /*
             * Precalculates the layer ranges given an offset.
             */
            var offsets = OffsetFactory.GetOffsets(3);
            _offsetRangeMapping = new Dictionary<Vector3Int, Range3D>();
            foreach (var offset in offsets)
            {
                var offsetDirectionIndex = AdjacencyMatrix.GetOffsetDirectionIndex(offset);
                var sign = offset[offsetDirectionIndex];
                
                var ranges = CalcLayerRanges3D(sign, offsetDirectionIndex);
                var range3d = new Range3D(ranges[0], ranges[1], ranges[2]);
                _offsetRangeMapping[offset] = range3d;
            }
        }

        private void CalcPatternAdjacency()
        {
            for (int i = 0; i < Patterns.Count; i++)
            {
                var thisPattern = Patterns[i];
                /*
                 * Find compatible patterns...
                 * Two patterns are said to be compatible if, given an offset, the layers orthogonal to that offset given the sign of direction are shared.
                 */
                foreach (var offset in _offsets)
                {
                    // Note no  i + 1 here, pattern can be compatible with itself.
                    for (int j = i; j < Patterns.Count; j++)
                    {
                        var thatPattern = Patterns[j];
                        
                        if (!IsCompatible(thisPattern, thatPattern, offset)) continue;
                        
                        PatternAdjacencyMatrix[offset][i, j] = true;
                        PatternAdjacencyMatrix[Vector3Util.Negate(offset)][j, i] = true;
                    }
                }
            }
        }

        private bool IsCompatible(int[,,] thisPattern, int[,,] thatPattern, Vector3Int offset)
        {
            var ranges = _offsetRangeMapping[offset];
            var rangeComplement = _offsetRangeMapping[Vector3Util.Negate(offset)];
            
            // Ranges denote min and max indices here. So, length is max index + 1.
            var xLength = ranges.XRange.GetLength() + 1;
            var yLength = ranges.YRange.GetLength() + 1;
            var zLength = ranges.ZRange.GetLength() + 1;

            for (int x = 0; x < xLength; x++)
            {
                var thisX = ranges.XRange.Start + x;
                var thatX = rangeComplement.XRange.Start + x;
                if (thisX >= thisPattern.GetLength(1) || thatX >= thatPattern.GetLength(1)) 
                    return false;
                
                for (int y = 0; y < yLength; y++)
                {
                    var thisY = ranges.YRange.Start + y;
                    var thatY = rangeComplement.YRange.Start + y;
                    if (thisY >= thisPattern.GetLength(0) || thatY >= thatPattern.GetLength(0)) 
                        return false;
                    
                    for (int z = 0; z < zLength; z++)
                    {
                        var thisZ = ranges.ZRange.Start + z;
                        var thatZ = rangeComplement.ZRange.Start + z;
                        if (thisZ >= thisPattern.GetLength(2) || thatZ >= thatPattern.GetLength(2)) 
                            return false;
                        
                        if (thisPattern[thisY, thisX, thisZ] != thatPattern[thatY, thatX, thatZ])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private Range CalcLayerRange(int sign, int offsetDirectionIndex)
        {
            /*
             * Denote the layer range indices. Subtract 2 to account for index < length and for one missing layer.
             */
            var firstLayer = sign < 0 ? 0 : 1;
            var compensation = _kernelSize[offsetDirectionIndex] > 1 ? 2 : 1;
            return new Range(firstLayer, firstLayer + _kernelSize[offsetDirectionIndex] - compensation);
        }

        private Range[] CalcLayerRanges3D(int sign, int offsetDirectionIndex)
        {
            var layerRanges = CalcLayerRange(sign, offsetDirectionIndex);

            var ranges = new Range[3];
                    
            // Find full set of ranges of the exposed layers in the pattern.
            for (int j = 0; j < 3; j++)
            {
                if (j == offsetDirectionIndex) ranges[j] = layerRanges;
                else ranges[j] = new Range(0, _kernelSize[j] - 1);
            }

            return ranges;
        }

        private void InitPatternMatrix()
        {
            PatternAdjacencyMatrix = new Dictionary<Vector3Int, bool[,]>();
            var nPatterns = Patterns.Count;
            foreach (var offset in _offsets)
            {
                PatternAdjacencyMatrix[offset] = new bool[nPatterns, nPatterns];
            }
        }

        private void InitMapping()
        {
            AtomPatternMapping = new Dictionary<int, Bidict<Vector3Int, HashSet<int>>>();
            foreach (var atom in AtomMapping.GetValues())
            {
                var value = new Bidict<Vector3Int, HashSet<int>>();
                foreach (var offset in GetPatternOffsets())
                {
                    value.AddPair(offset, new HashSet<int>());
                }

                AtomPatternMapping[atom] = value;
            }
        }

        public HashSet<Vector3Int> GetPatternOffsets()
        {
            var offsets = new HashSet<Vector3Int>();
            for (int i = 0; i < _kernelSize.x; i++)
            {
                for (int j = 0; j < _kernelSize.y; j++)
                {
                    for (int k = 0; k < _kernelSize.z; k++)
                    {
                        offsets.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            return offsets;
        }

        private void CalcPatterns()
        {

            var patternOffsets = GetPatternOffsets();
            
            /*
             * Map each atom id to a list of patterns said atom occurs in.
             * Index by the relative location of the atom in the pattern for easy retrieval.
             */
            // TODO: iterate over all patterns.

            foreach (var atomizedSample in _atomizedSamples)
            {
                var e = atomizedSample.GetExtent();
                
                // The last layers cannot contain full patterns. Minimal pattern size is 2x2x2 in 3D. 2x1x2 in 2D. 2x1x1 in 1D.
                // Infer dimension from kernel size.
                for (int x = 0; x < e.x - (_kernelSize.x > 1 ? 1 : 0); x++)
                {
                    for (int y = 0; y < e.y - (_kernelSize.y > 1 ? 1 : 0); y++)
                    {
                        for (int z = 0; z < e.z - (_kernelSize.z > 1 ? 1 : 0); z++)
                        {
                            var c = new Vector3Int(x, y, z);

                            var pattern = new int[_kernelSize.y, _kernelSize.x, _kernelSize.z];

                            var validPattern = true;
                            // Construct pattern.
                            foreach (var offset in patternOffsets)
                            {
                                Debug.Log($"{c}, {offset}, {e}");
                                var atomIds = atomizedSample.Get(c + offset);
                                if (atomIds.Count == 0)
                                {
                                    validPattern = false;
                                    break;
                                }
                                // TODO: consider multiple atoms per cell.
                                pattern[offset.y, offset.x, offset.z] = atomIds[0];
                            }
                            
                            // If the input contains cells with no specified atom, pattern is invalid.
                            if (!validPattern) continue;

                            // If the pattern already exists, the atom pattern mapping was already done.
                            if (Patterns.Contains(pattern)) continue;

                            var patternId = Patterns.Count;

                            // Add the pattern to the mapping for the corresponding atom ids and add to pattern list.
                            foreach (var offset in patternOffsets)
                            {
                                var patternAtom = pattern[offset.y, offset.x, offset.z];
                                AtomPatternMapping[patternAtom].GetValue(offset).Add(patternId);
                            }

                            Patterns.Add(pattern);
                        }
                    }
                }
            }
        }
        public float MaxEntropy()
        {
            return AdjacencyMatrix.CalcEntropy(Patterns.Count);
        }

        public int GetPatternAtomAtCoordinate(int patternId, Vector3Int coord)
        {
            return Patterns[patternId][coord.y, coord.x, coord.z];
        }

        public IEnumerable<bool> GetRow(int patternId, Vector3Int offset)
        {
            var adjacency = PatternAdjacencyMatrix[offset];
            for (int i = 0; i < adjacency.GetLength(1); i++)
            {
                yield return adjacency[patternId, i];
            }
        }

        public bool GetAdjacency(int patternId, int otherId, Vector3Int offset)
        {
            return PatternAdjacencyMatrix[offset][patternId, otherId];
        }
    }
    
}