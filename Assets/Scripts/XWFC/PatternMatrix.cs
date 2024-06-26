using UnityEngine;
using System.Collections.Generic;

namespace XWFC
{
    public class PatternMatrix
    {
        public Dictionary<int, Bidict<Vector3Int, HashSet<int>>> AtomPatternMapping;
        public List<int[,,]> Patterns;
        private readonly AtomGrid _atomizedSample;
        private readonly Vector3Int _kernelSize;
        private readonly AdjacencyMatrix _adjacencyMatrix;
        

        public PatternMatrix(AtomGrid atomizedSample, Vector3Int kernelSize, AdjacencyMatrix adjacencyMatrix)
        {
            _atomizedSample = atomizedSample;
            _kernelSize = kernelSize;
            _adjacencyMatrix = adjacencyMatrix;
            Patterns = new List<int[,,]>();
            InitMapping();
            CalcPatterns();
        }

        private void InitMapping()
        {
            AtomPatternMapping = new Dictionary<int, Bidict<Vector3Int, HashSet<int>>>();
            foreach (var atom in _adjacencyMatrix.AtomMapping.GetValues())
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

            var offsets = GetPatternOffsets();
            var maxOffsetIndex = _kernelSize - new Vector3Int(1, 1, 1);
            
            /*
             * Map each atom id to a list of patterns said atom occurs in.
             * Index by the relative location of the atom in the pattern for easy retrieval.
             */
            var e = _atomizedSample.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        var c = new Vector3Int(x, y, z);
                        var atom = _atomizedSample.Get(c)[0]; // TODO: consider multiple atoms per cell.
                        
                        // Only consider patterns that completely fit in the grid.
                        if (!_atomizedSample.WithinBounds(c + maxOffsetIndex)) continue;
                        
                        var pattern = new int[_kernelSize.y, _kernelSize.x, _kernelSize.z];
                        
                        // Construct pattern.
                        foreach (var offset in offsets)
                        {
                            var patternValue = _atomizedSample.Get(c + offset)[0]; // TODO: consider multiple atoms per cell.
                            pattern[offset.y, offset.x, offset.z] = patternValue;
                        }

                        // If the pattern already exists, the atom pattern mapping was already done.
                        if (Patterns.Contains(pattern))
                        {
                            continue;
                        }

                        var patternId = Patterns.Count;

                        // Add the pattern to the mapping for the corresponding atom ids and add to pattern list.
                        foreach (var offset in offsets)
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
}