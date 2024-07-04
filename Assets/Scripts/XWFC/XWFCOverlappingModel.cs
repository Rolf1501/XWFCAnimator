using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using PatternWave = XWFC.Grid<bool[]>;
using Random = System.Random;

namespace XWFC
{
    public class XWFCOverlappingModel : XWFC
    {
        public readonly PatternMatrix PatternMatrix;
        public AdjacencyMatrix AdjacencyMatrix;
        private PatternWave _patternWave;
        private int _nPatterns;
        private Grid<int> _atomGrid;
        private readonly Vector3Int _kernelSize;

        private Queue<Vector3Int> _propagationQueue = new();
        
        // This coordinate is used to set the base for the patterns; due to how patterns are constructed, the relative coordinate is always the same for each pattern. 
        private readonly Vector3Int _patternAtomCoord = new Vector3Int(0, 0, 0);

        private Random _random;
        private int _randomSeed = 1;

        public XWFCOverlappingModel(AtomGrid[] atomizedSamples, [NotNull] AdjacencyMatrix adjacencyMatrix, [NotNull] ref Grid<int> seededGrid, Vector3Int kernelSize, bool forceCompleteTiles = true) : base(adjacencyMatrix, ref seededGrid, forceCompleteTiles)
        {
            PatternMatrix = new PatternMatrix(atomizedSamples, kernelSize, adjacencyMatrix.AtomMapping);
            AdjacencyMatrix = adjacencyMatrix;

            _kernelSize = kernelSize;
            _nPatterns = PatternMatrix.Patterns.Count;
            CollapseQueue = new CollapsePriorityQueue();

            // _randomSeed = new Random().Next();
            _random = new Random(_randomSeed);
            
            // Initialize wave in superposition.
            var extent = seededGrid.GetExtent();
            // extent = new Vector3Int(5, 1, 5);
            _patternWave = new PatternWave(extent, SuperImposedWave());
            _atomGrid = new Grid<int>(extent, seededGrid.DefaultFillValue);
            
            PrintAdjacencyData();
            
            CollapseQueue.Insert(new Vector3Int(0,0,0), AdjacencyMatrix.CalcEntropy(_nPatterns));

            while (!CollapseQueue.IsDone())
            {
                Collapse(CollapseQueue.DeleteHead().Coord);
            }
            
            Debug.Log(GridToString(_atomGrid));
            CalcNutPatternPropagation();
        }

        private void CalcNutPatternPropagation()
        {
            var tileSet = AdjacencyMatrix.TileSet;
            foreach (var (tileId, tile) in tileSet)
            {
                /*
                 * For each open bond of each tile's atom, i.e. is not surrounded by atoms of the same tile,
                 * determine the patterns that may be placed next to it in the direction of the bond.
                 * Create grid with filled in atoms in center. Set grid size to be two larger than tile extent.
                 * Then, go for sliding window to obtain patterns tile. Check which patterns that tile pattern corresponds to. 
                 */
                
                // Match the dimensionality of the kernel.
                // Extension also corresponds to the origin o
                var tileOrigin = new Vector3Int(_kernelSize.x > 1 ? 1 : 0, _kernelSize.y > 1 ? 1 : 0,
                    _kernelSize.z > 1 ? 1 : 0);
                
                // Two additional layers are needed.
                var tileGridExtent = tile.Extent + tileOrigin * 2;
                
                var tileAtomGrid = new Grid<int>(tileGridExtent, -1);
                var orientation = 0;
                var tileWave = new PatternWave(tileAtomGrid.GetExtent(), SuperImposedWave());
                var propQueue = new Queue<Vector3Int>();
                
                foreach (var atomCoord in tile.OrientedIndices[orientation])
                {
                    var atomId = PatternMatrix.AtomMapping.Get((tileId, atomCoord, orientation));
                    tileAtomGrid.Set(tileOrigin + atomCoord, atomId);

                    foreach (var offset in Offsets)
                    {
                        var n = atomCoord + offset;
                        var gridCoord = n + tileOrigin;

                        if (!tileAtomGrid.WithinBounds(gridCoord)) continue;
                        // If neighbor is another tile atom, then pattern finding is redundant. Atoms are immediately collapsed. 
                        if (PatternMatrix.AtomMapping.ContainsKey((tileId, n, orientation))) continue;
                        
                        var offsetDirectionIndex = AdjacencyMatrix.GetOffsetDirectionIndex(offset);
                        var sign = offset[offsetDirectionIndex];
                        var localAtomCoord = _patternAtomCoord;

                        localAtomCoord[offsetDirectionIndex] = sign < 0 ? 1 : 0;
                        
                        // If the atom coordinate is not within the bounds of a pattern, continue.
                        if (localAtomCoord[offsetDirectionIndex] >= _kernelSize[offsetDirectionIndex]) continue;
                        
                        /*
                         * Find the patterns that contain the atom.
                         * The set of allowed patterns for its neighbors depends on the sets allowed for itself.
                         * The set of neighbor patterns are those where the neighbors are at the 0,0,0 coord.
                         */
                        var allowedPatterns = PatternMatrix.AtomPatternMapping[atomId].Get(localAtomCoord);
                        var wave = EmptyWave();
                        foreach (var allowedPattern in allowedPatterns)
                        {
                            // Find the patterns where the neighbor atom id is at the reference coordinate.
                            if (sign < 0)
                            {
                                wave[allowedPattern] = true;
                            }
                            else
                            {
                                // If the current atom is at the pattern atom coord, then the patterns that may be adjacent are those that are allowed at the given offset.
                                var row = PatternMatrix.GetRow(allowedPattern, offset).ToArray();
                                for (var i = 0; i < row.Length; i++)
                                {
                                    if (row[i]) wave[i] = true;
                                }
                            }
                        }

                        tileWave.Set(gridCoord, wave);
                        
                        propQueue.Enqueue(gridCoord);
                    }
                }

                Propagate(propQueue, ref tileWave, Offsets, ref tileAtomGrid, PatternMatrix);
                /*
                 * TODO: inspect result of propagation here. 
                 */
            }
        }

        private string GridToString<T>(Grid<T> grid)
        {
            string s = "";
            var matrix = grid.GetGrid();
            for (int y = 0; y < matrix.GetLength(0); y++)
            {
                s += "[\n";
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    s += "[";
                    for (int z = 0; z < matrix.GetLength(2); z++)
                    {
                        s += matrix[y, x, z] + ", ";
                    }
                    s += "]\n";
                }
                s += "\n]\n";
            }

            return s;
        }
        
        private void PrintAdjacencyData()
        {
            foreach (var o in Offsets)
            {
                var x = PatternMatrix.PatternAdjacencyMatrix[o];
                var s = "" + o + "\n";
                var ss = "\t";
                for (int k = 0; k < x.GetLength(0); k++)
                {
                    ss += k + "\t";
                }
                s += ss + "\n";
                for (var i = 0; i < x.GetLength(0);i++)
                {
                    s += $"{i}\t";
                    for (var j = 0; j < x.GetLength(1); j++)
                    {
                        s += x[i, j] + "\t";
                    }
                    s += "\n";
                }
                Debug.Log(s);
        
            }
            
            foreach (var (k,v) in PatternMatrix.AtomMapping.Dict)
            {
                Debug.Log($"{k}: {v}");
            }
        }
        
        private bool[] SuperImposedWave()
        {
            return Enumerable.Repeat(true, _nPatterns).ToArray();
        }
        
        private bool[] EmptyWave()
        {
            return new bool[_nPatterns];
        }

        private void Collapse(Vector3Int coord)
        {
            if (_atomGrid.Get(coord) != _atomGrid.DefaultFillValue) return;
            
            var choices = _patternWave.Get(coord);

            var uniformWeights = Enumerable.Repeat(1.0f, _nPatterns).ToArray();
            var chosenPatternId = RandomChoice(choices, uniformWeights, _random);
            var chosenAtom = PatternMatrix.GetPatternAtomAtCoordinate(chosenPatternId, _patternAtomCoord);
            
            /*
             * TODO: propagation after tile placement.
             * TODO: precompute the pattern masks per tile.
             */
            
            var updatedWave = EmptyWave();
            updatedWave[chosenPatternId] = true;
            _patternWave.Set(coord, updatedWave);
            _atomGrid.Set(coord, chosenAtom);

            _propagationQueue.Enqueue(coord);
            var collapseItems = Propagate(_propagationQueue, ref _patternWave, Offsets, ref _atomGrid, PatternMatrix);
            
            foreach (var (cell, entropy) in collapseItems)
            {
                CollapseQueue.Insert(cell, entropy);
            }
        }

        private static HashSet<(Vector3Int, float)> Propagate(Queue<Vector3Int> propQueue, ref PatternWave patternWave, IEnumerable<Vector3Int> offsets, ref Grid<int> atomGrid, PatternMatrix patternMatrix)
        {
            var nPatterns = patternWave.Get(0, 0, 0).Length;
            var collapseItems = new HashSet<(Vector3Int, float)>();
            var offsetArray = offsets.ToArray();
            
            while (propQueue.Count > 0)
            {
                var coord = propQueue.Dequeue();
                var choices = patternWave.Get(coord);
                var patternIds = new HashSet<int>();
                
                for (var i = 0; i < choices.Length; i++)
                {
                    if (choices[i]) patternIds.Add(i);
                }
                
                foreach (var offset in offsetArray)
                {
                    var neighbor = coord + offset;
                    
                    // Only consider cells within bounds.
                    if (!atomGrid.WithinBounds(neighbor)) continue;
                    
                    // Only consider uncollapsed cells.
                    if (atomGrid.Get(neighbor) != atomGrid.DefaultFillValue) continue;

                    
                    // Get union of allowed neighbors of the current cell.
                    var allowedNeighbors = new bool[nPatterns];
                    foreach (var patternId in patternIds)
                    {
                        for (int i = 0; i < nPatterns; i++)
                        {
                            allowedNeighbors[i] |= patternMatrix.GetAdjacency(patternId, i, offset);
                        }
                    }
                    
                    var neighborChoices = patternWave.Get(neighbor);

                    var post = new bool[nPatterns];
                    var preIsPost = true;
                    var remainingChoiceCount = 0;
                    
                    for (int i = 0; i < neighborChoices.Length; i++)
                    {
                        var isPatternAllowed = allowedNeighbors[i] && neighborChoices[i];
                        post[i] = isPatternAllowed;
                        if (isPatternAllowed != neighborChoices[i])
                        {
                            preIsPost = false;
                        }

                        if (isPatternAllowed)
                        {
                            remainingChoiceCount++;
                        }
                    }

                    if (preIsPost) continue;
                    
                    /*
                     * Update neighbor.
                     */
                    patternWave.Set(neighbor, post);
                    
                    if (remainingChoiceCount == 1)
                    {
                        /*
                         * TODO: Can already collapse the neighbor if there's only one choice remaining.
                         */
                        //continue;
                    }

                    if (remainingChoiceCount == 0)
                    {
                        /*
                         * Conflict...
                         */
                        throw new NoMoreChoicesException($"No more choices remain for {neighbor}");
                    }

                    collapseItems.Add((neighbor, AdjacencyMatrix.CalcEntropy(remainingChoiceCount)));// .Insert(neighbor, AdjacencyMatrix.CalcEntropy(remainingChoiceCount));
                    propQueue.Enqueue(neighbor);
                }
            }

            return collapseItems;
        }

        private HashSet<int> GetValidAtomsInPattern(int patternId, Vector3Int offset, Vector3Int atomCoord)
        {
            /*
             * Given a context of a pattern id in the cell of question and an atom id in a neighbouring cell at the given offset,
             * find the set of atom ids that fit the context.
             */
            
            var neighborCoord = atomCoord + offset;
            
            var pattern = PatternMatrix.Patterns[patternId];
            
            /*
             * TODO: move this to precomputation.
             */
            var patternIndexMapping = new Dictionary<Vector3Int, int>();
            var patternIndexMappingInv = new Dictionary<int, List<Vector3Int>>();
            
            for (var y = 0; y < pattern.GetLength(0); y++)
            {
                for (var x = 0; x < pattern.GetLength(1); x++)
                {
                    for (var z = 0; z < pattern.GetLength(2); z++)
                    {
                        var atomId = pattern[y, x, z];
                        patternIndexMapping[new Vector3Int(x, y, z)] = atomId;
                        
                        if (patternIndexMappingInv[pattern[y, x, z]] == null)
                        {
                            patternIndexMappingInv[atomId] = new List<Vector3Int>() { new Vector3Int(x, y, z) };
                        }
                        else
                        {
                            patternIndexMappingInv[atomId].Add(new Vector3Int(x,y,z));
                        }
                    }
                }
            }

            var neighborAtomId = _atomGrid.Get(neighborCoord); 
                
            var neighborPatterns = _patternWave.Get(neighborCoord);

            var atoms = new HashSet<int>();

            // If the neighbor was not set yet, obtain the of remaining valid atom ids.
            // These mus be part of the pattern given, by construction of the pattern overlap.
            if (neighborAtomId != _atomGrid.DefaultFillValue)
            {
                atoms.Add(neighborAtomId);
            }
            else
            {
                foreach (var neighborPattern in neighborPatterns)
                {
                    // if (neighborPattern.Count > 0)
                    // {
                    //     
                    // }
                }
            }
            var validAtoms = new HashSet<int>();
            var neighborPositions = patternIndexMappingInv[neighborAtomId];
            foreach (var neighborPosition in neighborPositions)
            {
                var position = neighborPosition - offset;
                if (patternIndexMapping.ContainsKey(position))
                {
                    validAtoms.Add(pattern[position.y, position.x, position.z]);
                }
            }

            return validAtoms;


        }
    }
}