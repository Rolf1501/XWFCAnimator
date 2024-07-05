using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Unity.Jobs.LowLevel.Unsafe;
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
        public readonly Dictionary<int, HashSet<(Vector3Int, bool[])>> TilePatternMasks;

        private List<(int[,,] patternWave, Grid<int> atomGrid)> _saveStates = new();

        public XWFCOverlappingModel(IEnumerable<AtomGrid> atomizedSamples, [NotNull] AdjacencyMatrix adjacencyMatrix, [NotNull] ref Grid<int> seededGrid, Vector3Int kernelSize, bool forceCompleteTiles = true) : base(adjacencyMatrix, ref seededGrid, forceCompleteTiles)
        {
            PatternMatrix = new PatternMatrix(atomizedSamples, kernelSize, adjacencyMatrix.AtomMapping);
            AdjacencyMatrix = adjacencyMatrix;

            _kernelSize = kernelSize;
            _nPatterns = PatternMatrix.Patterns.Count;
            CollapseQueue = new CollapsePriorityQueue();

            // _randomSeed = new Random().Next();
            _random = new Random(_randomSeed);
            
            // Initialize wave in superposition.
            var extent = seededGrid.GetExtent() * 2  - 1 * new Vector3Int(0,seededGrid.GetExtent().y, 0);
            extent = new Vector3Int(18,1,10);
            _patternWave = new PatternWave(extent, SuperImposedWave());
            _atomGrid = new Grid<int>(extent, seededGrid.DefaultFillValue);
            
            PrintAdjacencyData();
            
            EliminateIncompletePatterns();
            
            TilePatternMasks = CalcNutPatternPropagation();
            
            CollapseQueue.Insert(new Vector3Int(0,0,0), AdjacencyMatrix.CalcEntropy(_nPatterns));

            while (!CollapseQueue.IsDone())
            {
                Collapse(CollapseQueue.DeleteHead().Coord);
            }
            
            Debug.Log(GridToString(_atomGrid));
        }

        private Dictionary<int, HashSet<(Vector3Int, bool[])>> CalcNutPatternPropagation()
        {
            var tilePatternMasks = new Dictionary<int, HashSet<(Vector3Int, bool[])>>();
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

                try
                {
                    // In case a tile yields a case where no patterns are allowed at a given location, that is still a valid conclusion.
                    // This can be achieved by ignoring conflicts, stopping the propagation trace as soon as a conflict is reached. 
                    Propagate(propQueue, ref tileWave, Offsets, ref tileAtomGrid, PatternMatrix, ignoreConflict:true);
                }
                catch
                {
                    continue;
                }
                var e = tileWave.GetExtent(); 
                tilePatternMasks[tileId] = new HashSet<(Vector3Int, bool[])>();
                for (int x = 0; x < e.x; x++)
                {
                    for (int y = 0; y < e.y; y++)
                    {
                        for (int z = 0; z < e.z; z++)
                        {
                            var gridCoord = new Vector3Int(x, y, z);
                            var relativeCoord = gridCoord - tileOrigin;
                            
                            // Atoms belonging to the tile require no precomputed mask.
                            if (PatternMatrix.AtomMapping.ContainsKey((tileId, relativeCoord, orientation))) continue;
                            tilePatternMasks[tileId].Add((relativeCoord, tileWave.Get(gridCoord)));
                        }
                    }
                }
            }

            return tilePatternMasks;
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

        private void EliminateIncompletePatterns()
        {
            var e = _patternWave.GetExtent();
            var propQueue = new Queue<Vector3Int>();
            
            for (int x = 0; x < e.x - _kernelSize.x + 1; x++)
            {
                for (int y = 0; y < e.y - _kernelSize.y + 1; y++)
                {
                    for (int z = 0; z < e.z - _kernelSize.z + 1; z++)
                    {
                        var coord = new Vector3Int(x, y, z);
                        var patterns = _patternWave.Get(coord);
                        var emptyWave = EmptyWave();
                        var preIsPost = true;
                        for (var i = 0; i < patterns.Length; i++)
                        {
                            if (!patterns[i]) continue;
                            if (PatternFits(i, coord))
                            {
                                emptyWave[i] = true;
                            }
                            else
                            {
                                preIsPost = false;
                            }
                        }

                        if (!preIsPost)
                        {
                            propQueue.Enqueue(coord);
                            _patternWave.Set(x,y,z, emptyWave);
                        }
                    }
                }
            }

            // Propagate the changes imposed by pattern elimination.
            Propagate(propQueue, ref _patternWave, Offsets, ref _atomGrid, PatternMatrix);
        }

        private bool PatternFits(int patternId, Vector3Int coord)
        {
            var pattern = PatternMatrix.Patterns[patternId];
            
            // All atoms in the patterns must fit.
            for (int x = 0; x < _kernelSize.x; x++)
            {
                for (int y = 0; y < _kernelSize.y; y++)
                {
                    for (int z = 0; z < _kernelSize.z; z++)
                    {
                        var patternCoord = new Vector3Int(x, y, z);
                        var atomId = pattern[y, x, z];
                        var (tileId, atomCoord, _) = PatternMatrix.AtomMapping.Get(atomId);
                        var tile = AdjacencyMatrix.TileSet[tileId];
                        
                        // Check tile bounds.
                        var minCoord = coord + patternCoord - atomCoord;
                        var maxCoord = coord + patternCoord - atomCoord + tile.Extent - new Vector3Int(1,1,1);
                        if (!_patternWave.WithinBounds(minCoord)) return false;
                        if (!_patternWave.WithinBounds(maxCoord)) return false;
                        
                        // If it is within bounds, check if the tile is not obstructed by occupied cells containing atoms of other tiles.
                        var indices = tile.OrientedIndices[0];
                        var tileOrigin = coord + patternCoord - atomCoord;
                        
                        foreach (var index in indices)
                        {
                            var cellValue = _atomGrid.Get(tileOrigin + index);
                            var expectedValue = PatternMatrix.AtomMapping.Get((tileId, index, 0));
                            if (cellValue != _atomGrid.DefaultFillValue && cellValue != expectedValue) return false;
                        }
                    }
                }
            }

            return true;
        }

        private void Collapse(Vector3Int coord)
        {
            if (_atomGrid.Get(coord) != _atomGrid.DefaultFillValue) return;
            
            var choices = _patternWave.Get(coord);

            var uniformWeights = Enumerable.Repeat(1.0f, _nPatterns).ToArray();
            var chosenPatternId = RandomChoice(choices, uniformWeights, _random);

            var wave = EmptyWave();
            wave[chosenPatternId] = true;
            _atomGrid.Set(coord, PatternMatrix.GetPatternAtomAtCoordinate(chosenPatternId, _patternAtomCoord));
            _patternWave.Set(coord, wave);
            _propagationQueue.Enqueue(coord);
            
            
            
            /*
             * Collapse an entire pattern at once; this is allowed because when the pattern reference may be placed, so may the others by construction of the patterns.
             * For each atom in the pattern, collapse its entire tile immediately and update neighbors.
             */
            // var pattern = PatternMatrix.Patterns[chosenPatternId];
            // var (x, y, z) = (pattern.GetLength(1), pattern.GetLength(0), pattern.GetLength(2));

            // for (int px = 0; px < x; px++)
            // {
            //     for (int py = 0; py < y; py++)
            //     {
            //         for (int pz = 0; pz < z; pz++)
            //         {
            //             var patternCoord = new Vector3Int(px, py, pz);
            //             if (_atomGrid.IsChosen(patternCoord + coord)) continue;
            //             var atomId = pattern[py, px, pz];
            //             _atomGrid.Set(px,py,pz,atomId);
            //             
            //             /*
            //              * Immediately collapse other atoms belonging to same tile.
            //              */
            //             var (tileId, chosenAtomCoord, orientation) = PatternMatrix.AtomMapping.GetKey(atomId);
            //             var tile = AdjacencyMatrix.TileSet[tileId];
            //             
            //             foreach (var atomCoord in tile.OrientedIndices[orientation])
            //             {
            //                 // Find the grid coordinate corresponding to the relative position of the chosen atom coordinate and the other atom coordinate.
            //                 var gridCoord = coord + atomCoord - chosenAtomCoord;
            //                 _atomGrid.Set(gridCoord, PatternMatrix.AtomMapping.GetValue((tileId, atomCoord, orientation)));
            //             }
            //             
            //             /*
            //              * For each atom affected by tile placement, update with precomputed propagated pattern wave.
            //              */
            //             var tilePatternMask = TilePatternMasks[tileId];
            //             foreach (var (relativeCoord, wave) in tilePatternMask)
            //             {
            //                 var gridCoord = coord + relativeCoord - chosenAtomCoord;
            //
            //                 if (!_patternWave.WithinBounds(gridCoord)) continue;
            //
            //                 var updatedWave = EmptyWave();
            //                 var pre = _patternWave.Get(gridCoord);
            //                 var preIsPost = true;
            //                 for (var i = 0; i < wave.Length; i++)
            //                 {
            //                     updatedWave[i] = pre[i] && wave[i];
            //                     if (updatedWave[i] != pre[i]) preIsPost = false;
            //                 }
            //
            //                 if (!preIsPost) _propagationQueue.Enqueue(gridCoord);
            //
            //                 // Cells added through mask overlay have to be added manually.
            //                 CollapseQueue.Insert(gridCoord, AdjacencyMatrix.CalcEntropy(updatedWave.Count(b =>b)));
            //                 _patternWave.Set(gridCoord, updatedWave);
            //             }
            //         }
            //     }
            // }
            
            // var chosenAtom = PatternMatrix.GetPatternAtomAtCoordinate(chosenPatternId, _patternAtomCoord);
            //
            // /*
            //  * For each tile atom, find its grid coordinate and collapse those to the corresponding atom id. 
            //  */
            // var (tileId, chosenAtomCoord, orientation) = PatternMatrix.AtomMapping.GetKey(chosenAtom);
            // var tile = AdjacencyMatrix.TileSet[tileId];
            // var atomCoords = tile.OrientedIndices[orientation];
            // var tilePatternMask = TilePatternMasks[tileId];
            //
            // foreach (var atomCoord in atomCoords)
            // {
            //     var gridCoord = coord + atomCoord - chosenAtomCoord;
            //     _atomGrid.Set(gridCoord, PatternMatrix.AtomMapping.GetValue((tileId, atomCoord, orientation)));
            // }
            //
            // /*
            //  * For each atom affected by tile placement, update with precomputed propagated pattern wave.
            //  */ 
            // foreach (var (relativeCoord, wave) in tilePatternMask)
            // {
            //     var gridCoord = coord + relativeCoord - chosenAtomCoord;
            //     
            //     if (!_patternWave.WithinBounds(gridCoord)) continue;
            //     
            //     var updatedWave = EmptyWave();
            //     var pre = _patternWave.Get(gridCoord);
            //     var preIsPost = true;
            //     for (var i = 0; i < wave.Length; i++)
            //     {
            //         updatedWave[i] = pre[i] && wave[i];
            //         if (updatedWave[i] != pre[i]) preIsPost = false;
            //     }
            //     if (!preIsPost) _propagationQueue.Enqueue(gridCoord);
            //     
            //     // Cells added through mask overlay have to be added manually.
            //     CollapseQueue.Insert(gridCoord, AdjacencyMatrix.CalcEntropy(updatedWave.Count(x=>x)));
            //     _patternWave.Set(gridCoord, updatedWave);
            // }
            //
            
            var collapseItems = Propagate(_propagationQueue, ref _patternWave, Offsets, ref _atomGrid, PatternMatrix);
            
            foreach (var (cell, entropy) in collapseItems)
            {
                CollapseQueue.Insert(cell, entropy);
            }
        }

        private HashSet<(Vector3Int, float)> Propagate(Queue<Vector3Int> propQueue, ref PatternWave patternWave, IEnumerable<Vector3Int> offsets, ref Grid<int> atomGrid, PatternMatrix patternMatrix, bool ignoreConflict=false)
        {
            var nPatterns = patternWave.Get(0, 0, 0).Length;
            var collapseItems = new HashSet<(Vector3Int, float)>();
            var offsetArray = offsets.ToArray();
            
            while (propQueue.Count > 0)
            {
                var coord = propQueue.Dequeue();
                var choices = patternWave.Get(coord);
                
                foreach (var offset in offsetArray)
                {
                    var neighbor = coord + offset;
                    
                    // Only consider cells within bounds.
                    // The outer positive layers can be ignored, those are filled in post-processing. Assumes pattern atom coord of 0,0,0.
                    if (!atomGrid.WithinBounds(neighbor) || !atomGrid.WithinBounds(neighbor + _kernelSize - new Vector3Int(1,1,1))) continue;
                    
                    // Only consider uncollapsed cells.
                    if (atomGrid.Get(neighbor) != atomGrid.DefaultFillValue) continue;
                    
                    // Get union of allowed neighbors of the current cell.
                    var allowedNeighbors = new bool[nPatterns];
                    for (var i = 0; i < choices.Length; i++)
                    {
                        if (!choices[i]) continue;
                        for (int j = 0; j < nPatterns; j++)
                        {
                            allowedNeighbors[j] |= patternMatrix.GetAdjacency(i, j, offset);
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
                    
                    if (!CollapseQueue.Contains(neighbor) || !preIsPost) collapseItems.Add((neighbor, AdjacencyMatrix.CalcEntropy(remainingChoiceCount)));
                    
                    if (preIsPost) continue;
                    
                    /*
                     * Update neighbor.
                     */
                    
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
                        if (ignoreConflict) continue;
                        int i = 0;
                        foreach (var patternMatrixPattern in PatternMatrix.Patterns)
                        {
                            var builder = new StringBuilder();
                            builder.Append($"{i}:\t");
                            i++;
                            foreach (var i1 in patternMatrixPattern)
                            {
                                builder.Append($"{i1},");
                            }
                            Debug.Log(builder.ToString());
                        }
                        
                        Debug.Log(GridToString(_atomGrid));
                        throw new NoMoreChoicesException($"No more choices remain for {neighbor}");
                    }
                    
                    patternWave.Set(neighbor, post);

                    propQueue.Enqueue(neighbor);
                }
            }

            return collapseItems;
        }
    }
}