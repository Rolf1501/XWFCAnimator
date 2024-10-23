using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using PatternWave = XWFC.Grid<bool[]>;
using Random = System.Random;

namespace XWFC
{
    public class XwfcOverlappingModel : XwfcStm
    {
        public readonly PatternMatrix PatternMatrix;
        public AdjacencyMatrix AdjacencyMatrix;
        private PatternWave _patternWave;
        private Grid<int> _atomGrid;
        private readonly Vector3Int _kernelSize;

        private Queue<Vector3Int> _propagationQueue = new();
        
        // This coordinate is used to set the base for the patterns; due to how patterns are constructed, the relative coordinate is always the same for each pattern. 
        private readonly Vector3Int _patternAtomCoord = new Vector3Int(0, 0, 0);

        private Random _random;
        public readonly Dictionary<int, HashSet<(Vector3Int, bool[])>> TilePatternMasks;

        private (Grid<int> atoms, PatternWave wave) _rootSave;
        private int _blockedCellId;
        private Grid<int> _seededGrid;
        private float[] _weights;
        
        public XwfcOverlappingModel(IEnumerable<AtomGrid> atomizedSamples, [NotNull] AdjacencyMatrix adjacencyMatrix, [NotNull] ref Grid<int> seededGrid, Vector3Int kernelSize, int randomSeed, bool forceCompleteTiles = true) : base(adjacencyMatrix, ref seededGrid, randomSeed, forceCompleteTiles)
        {
            PatternMatrix = new PatternMatrix(atomizedSamples, kernelSize, adjacencyMatrix.AtomMapping);
            AdjacencyMatrix = adjacencyMatrix;

            _kernelSize = kernelSize;
            CollapseQueue = new CollapsePriorityQueue();

            RandomSeed = randomSeed;
            Debug.Log($"Random Seed: {RandomSeed}");
            
            _random = new Random(RandomSeed);

            _seededGrid = seededGrid;
            // Initialize wave in superposition.
            var extent = seededGrid.GetExtent();
            
            InitGrids(extent);

            _weights = new float[PatternMatrix.Patterns.Length];
            for (var i = 0; i < PatternMatrix.Patterns.Length; i++)
            {
                var pattern = PatternMatrix.Patterns[i];
                var w = 0.0f;
                for (int x = 0; x < pattern.GetLength(1); x++)
                {
                    for (int y = 0; y < pattern.GetLength(0); y++)
                    {
                        for (int z = 0; z < pattern.GetLength(0); z++)
                        {
                            var (tileId, coord, _) = adjacencyMatrix.AtomMapping.GetKey(pattern[y, x, z]);
                            w += adjacencyMatrix.TileWeigths[tileId];
                        }
                    }
                }

                _weights[i] = w;
            }
            
            StartCoord = CalcStartCoord();
            
            // PrintAdjacencyData();
            
            
            TilePatternMasks = CalcNutPatternPropagation();
            
            // Find first unoccupied cell in grid.
            _blockedCellId = BlockedCellId(_atomGrid.DefaultFillValue, AdjacencyMatrix.TileSet.Keys);
            CollapseQueue.Insert(StartCoord, AdjacencyMatrix.CalcEntropy(GetNPatterns()));

            
            // while (!CollapseQueue.IsDone())
            // {
            //     Collapse(CollapseQueue.DeleteHead().Coord);
            // }
            //
            // if (CollapseQueue.IsDone())
            // {
            //     FillInBlanks();
            // }
        }

        private void InitGrids(Vector3Int extent)
        {
            _patternWave = new PatternWave(extent, SuperImposedWave());
            if (!extent.Equals(_seededGrid.GetExtent()))
            {
                var sE = _seededGrid.GetExtent();
                _atomGrid = new Grid<int>(extent, _seededGrid.DefaultFillValue);
                for (int x = 0; x < Math.Min(extent.x, sE.x); x++)
                {
                    for (int y = 0; y < Math.Min(extent.y, sE.y); y++)
                    {
                        for (int z = 0; z < Math.Min(extent.z, sE.z); z++)
                        {
                            _atomGrid.Set(x,y,z, _atomGrid.Get(x,y,z));
                        }
                    }
                }
            }
            else
            {
                _atomGrid = _seededGrid.Deepcopy();
            }
            EliminateIncompletePatterns();
            InitRootSave();
        }

        private void InitRootSave()
        {
            _rootSave = (_atomGrid.Deepcopy(), _patternWave.Deepcopy());
        }

        private void FillInBlanks()
        {
            var e = _atomGrid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        if (_atomGrid.IsOccupied(x,y,z)) continue;
                        var coord = new Vector3Int(x, y, z);
                        var v = e - _kernelSize - coord;
                        var kernelCoordDelta = new Vector3Int(0, 0, 0);
                        if (v.x < 0) kernelCoordDelta.x = v.x;
                        if (v.y < 0) kernelCoordDelta.y = v.y;
                        if (v.z < 0) kernelCoordDelta.z = v.z;
                        
                        Collapse(coord + kernelCoordDelta);
                        Propagate();

                    }
                }
            }
        }

        private bool IsAtKernelBoundary(Vector3Int coord)
        {
            var e = _atomGrid.GetExtent();
            var v = e - _kernelSize - coord;
            return v.x <= 0 || v.y <= 0 || v.z <= 0;
            
        }

        public override Grid<int> GetGrid()
        {
            return _atomGrid;
        }

        public override Grid<bool[]> GetWave()
        {
            return _patternWave;
        }

        private Vector3Int CalcStartCoord()
        {
            var e = _atomGrid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        if (!_atomGrid.IsOccupied(x, y, z)) return new Vector3Int(x, y, z);
                    }
                }
            }

            return new Vector3Int(-1, -1, -1);
        }

        public override void UpdateExtent(Vector3Int extent)
        {
            if (!extent.Equals(_atomGrid.GetExtent()))
            {
                InitGrids(extent);
            }
            Reset();
        }

        protected override void Reset()
        {
            _atomGrid = _rootSave.atoms.Deepcopy();
            _patternWave = _rootSave.wave.Deepcopy();
            _random = new Random(RandomSeed);
            CollapseQueue.Clear();
            CollapseQueue.Insert(StartCoord, AdjacencyMatrix.CalcEntropy(GetNPatterns()));
            _propagationQueue.Clear();
            Debug.Log($"RESET OVERLAPPING MODEL; RANDOM SEED: {RandomSeed}");
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
            return Enumerable.Repeat(true, GetNPatterns()).ToArray();
        }

        private int GetNPatterns()
        {
            return PatternMatrix.Patterns.Length;
        }
        
        private bool[] EmptyWave()
        {
            return new bool[GetNPatterns()];
        }

        private void EliminateIncompletePatterns()
        {
            Debug.Log("Eliminating patterns...");
            var timer = new Timer();
            timer.Start();
            var e = _patternWave.GetExtent();
            var propQueue = new Queue<Vector3Int>();
            var pending = new HashSet<Vector3Int>();
            
            for (int x = 0; x < e.x - _kernelSize.x + 1; x++)
            {
                for (int y = 0; y < e.y - _kernelSize.y + 1; y++)
                {
                    for (int z = 0; z < e.z - _kernelSize.z + 1; z++)
                    {
                        var coord = new Vector3Int(x, y, z);
                        var patterns = _patternWave.Get(coord);
                        var wave = EmptyWave();
                        var preIsPost = true;
                        for (var i = 0; i < patterns.Length; i++)
                        {
                            if (!patterns[i]) continue;
                            if (PatternFits(i, coord))
                            {
                                wave[i] = true;
                            }
                            else
                            {
                                preIsPost = false;
                            }
                        }

                        if (!preIsPost)
                        {
                            pending.Add(coord);
                            _patternWave.Set(x,y,z, wave);
                        }
                    }
                }
            }
            
            foreach (var i in pending)
            {
                propQueue.Enqueue(i);
            }

            timer.Stop();
            var p = PatternMatrix.Patterns.ToArray();
            timer.Start();
            // Propagate the changes imposed by pattern elimination.
            Propagate(propQueue, ref _patternWave, Offsets, ref _atomGrid, PatternMatrix);
            timer.Stop();
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
                        
                        // If the origin of a tile is already occupied, then the pattern is still allowed, since that tile will not be placed.
                        if (atomCoord.Equals(new Vector3Int(0,0,0)) && _atomGrid.Get(coord + patternCoord) == _blockedCellId) continue;
                        
                        var tile = AdjacencyMatrix.TileSet[tileId];
                        
                        // Check tile bounds.
                        var minCoord = coord + patternCoord - atomCoord;
                        var maxCoord = coord + patternCoord - atomCoord + tile.Extent - new Vector3Int(1,1,1);
                        if (!_patternWave.WithinBounds(minCoord)) return false;
                        if (!_patternWave.WithinBounds(maxCoord)) return false;
                        
                        // If it is within bounds, check if the tile is not obstructed by occupied cells.
                        var indices = tile.OrientedIndices[0];
                        var tileOrigin = coord + patternCoord - atomCoord;

                        var fullyCovered = true;
                        var coveredSelf = true;
                        // If all coordinates of the tiles are already occupied, the tile will not be placed and the pattern is still allowed. 
                        foreach (var index in indices)
                        {
                            var cellValue = _atomGrid.Get(tileOrigin + index);

                            var expectedValue = PatternMatrix.AtomMapping.Get((tileId, index, 0));
                            // Another tile does not occlude this tile if the cell is not occupied, or if the cell is occupied by the corresponding atom.
                            
                            // If there is a cell where a foreign atom occludes tile placement, then the tile cannot be placed.
                            // If all atoms of the to-be-placed tile are occupied, then the pattern should still be allowed.
                            if (cellValue != _atomGrid.DefaultFillValue && cellValue != expectedValue)
                            {
                                coveredSelf = false;
                            }
                            else
                            {
                                fullyCovered = false;
                            }
                            
                        }
                        if (!(fullyCovered || coveredSelf)) return false;
                        
                    }
                }
            }

            return true;
        }

        private bool MayCollapse(Vector3Int coord)
        {
            return !_atomGrid.IsOccupied(coord) ||
                   (_atomGrid.Get(coord) == _blockedCellId && IsAtKernelBoundary(coord));
        }
        protected sealed override void Collapse(Vector3Int coord)
        {
            if (!MayCollapse(coord)) return;
            
            var choices = _patternWave.Get(coord);

            var uniformWeights = Enumerable.Repeat(1.0f, GetNPatterns()).ToArray();
            var chosenPatternId = RandomChoice(choices, _weights, _random);

            var wave = EmptyWave();
            if (chosenPatternId < 0) throw new NoMoreChoicesException("No more choices...");
            wave[chosenPatternId] = true;
            _patternWave.Set(coord, wave);
            _propagationQueue.Enqueue(coord);

            SetOccupied(coord, chosenPatternId);
            
            
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
            
            var collapseItems = Propagate();
            
            foreach (var (cell, entropy) in collapseItems)
            {
                CollapseQueue.Insert(cell, entropy);
            }
            
            if (CollapseQueue.IsDone()) FillInBlanks();
        }

        protected override void SetOccupied(Vector3Int coord, int id)
        {
            /*
             * If near the bounds of the grid, collapse the remaining atoms immediately. Assumes pattern atom coordinate 0,0,0.
             */
            var coordPatterns = coord + _kernelSize;
            var e = _atomGrid.GetExtent();
            var xBound = coordPatterns.x == e.x ? _kernelSize.x : 1;
            var yBound = coordPatterns.y == e.y ?  _kernelSize.y : 1;
            var zBound = coordPatterns.z == e.z ? _kernelSize.z : 1;
            var pattern = PatternMatrix.Patterns[id];

            for (int x = 0; x < xBound; x++)
            {
                for (int y = 0; y < yBound; y++)
                {
                    for (int z = 0; z < zBound; z++)
                    {
                        var n = new Vector3Int(coord.x + x, coord.y + y, coord.z + z);
                        if (!_atomGrid.IsOccupied(n)) _atomGrid.Set(n, pattern[y,x,z]);
                    }
                }
            }
        }

        protected override HashSet<(Vector3Int, float)> Propagate()
        {
            return Propagate(_propagationQueue, ref _patternWave, Offsets, ref _atomGrid, PatternMatrix);
        }
        
        public override void CollapseOnce()
        {
            /*
             * Performs a single collapse and outputs the affected cells' coordinates.
             * It is up to the caller to refer to the grid to find the cell's contents.
             */
            if (CollapseQueue.IsDone())
            {
                Debug.Log($"Complete! Seed: {RandomSeed}");
                return;
            }


            var nAttempts = 100;
            var i = 0;
            while (i < nAttempts)
            {
                var head = CollapseQueue.DeleteHead();
                if (CollapseList.IsDefaultCoord(head.Coord)) return;
                 
                var coll = head.Coord;
                var grid = GetGrid();
                if (!grid.WithinBounds(coll)) return;
                
                while ((!grid.WithinBounds(coll) || grid.IsOccupied(coll)) && !CollapseQueue.IsDone())
                {
                    coll = CollapseQueue.DeleteHead().Coord;
                }
                try
                {
                    Collapse(coll);
                    break;
                }
                catch (NoMoreChoicesException)
                {

                    RandomSeed = new Random().Next();
                    UpdateRandom(RandomSeed);
                    Reset();
                    i++;
                    // When rewinding, pass the coordinates of the cells that changed.
                    // The caller can then refer the grid manager to find what the values of the cells should be. 

                }
            }

            var collapseItems = Propagate();
            foreach (var (coord, entropy) in collapseItems)
            {
                CollapseQueue.Insert(coord, entropy);
            }
        }

        private HashSet<(Vector3Int, float)> Propagate(Queue<Vector3Int> propQueue, ref PatternWave patternWave, IEnumerable<Vector3Int> offsets, ref Grid<int> atomGrid, PatternMatrix patternMatrix, bool ignoreConflict=false)
        {
            if (patternWave == null) return new HashSet<(Vector3Int, float)>();
            
            var nPatterns = GetNPatterns();
            var collapseItems = new HashSet<(Vector3Int, float)>();
            var offsetArray = offsets.ToArray();

            var enqueued = new HashSet<Vector3Int>();
            foreach (var i in propQueue)
            {
                enqueued.Add(i);
            }

            var unionTime = 0f;
            
            while (propQueue.Count > 0)
            {
                var coord = propQueue.Dequeue();
                // enqueued.Remove(coord);
                var choices = patternWave.Get(coord);
                var choiceInts =new List<int>();
                for (var i = 0; i < choices.Length; i++)
                {
                    if (choices[i]) choiceInts.Add(i);
                }
                
                
                foreach (var offset in offsetArray)
                {
                    var neighbor = coord + offset;
                    
                    // Only consider cells within bounds.
                    // The outer positive layers can be ignored, those are filled in post-processing. Assumes pattern atom coord of 0,0,0.
                    if (!atomGrid.WithinBounds(neighbor) || !atomGrid.WithinBounds(neighbor + _kernelSize - new Vector3Int(1,1,1))) continue;
                    
                    // Edge case: if using a seeded grid, then the cells at the most positive layers are not propagated to.
                    // It could be that there's a gap smaller than the kernel size, resulting in an uncollapse cell.
                    // So, perform propagation to those cells and perform tile fitting check.
                    var coordExtent = neighbor + _kernelSize;
                    var atBounds = !atomGrid.WithinBounds(coordExtent);
                    
                    // Only consider uncollapsed cells not at the boundaries.
                    if (!atBounds && atomGrid.IsOccupied(neighbor)) continue;
                    
                    // Get union of allowed neighbors of the current cell.
                    var allowedNeighbors = patternMatrix.GetRowVectors(choiceInts[0], offset);
                    // var allowedNeighbors = new bool[nPatterns];
                    
                    var timer = new Timer();
                    timer.Start(false);
                    for (var i = 0; i < choiceInts.Count; i++)
                    {
                        var other = patternMatrix.GetRowVectors(choiceInts[i], offset);
                        allowedNeighbors = Vectorizor.Or(allowedNeighbors, other);
                    }

                    unionTime += timer.Stop(false);
                    var neighborChoices = patternWave.Get(neighbor);

                    var post = new bool[nPatterns];
                    var preIsPost = true;
                    var remainingChoiceCount = 0;
                    var latestChoice = -1;
                    
                    for (int i = 0; i < neighborChoices.Length; i++)
                    {
                        var allowed = Vectorizor.GetAtIndex(i, allowedNeighbors) == 1;
                        var isPatternAllowed = allowed && neighborChoices[i];
                        // var isPatternAllowed = allowedNeighbors[i] && neighborChoices[i];
                        post[i] = isPatternAllowed;
                        if (isPatternAllowed != neighborChoices[i])
                        {
                            preIsPost = false;
                        }

                        if (isPatternAllowed)
                        {
                            remainingChoiceCount++;
                            latestChoice = i;
                        }
                    }
                    
                    if (!atomGrid.IsOccupied(neighbor) && (!CollapseQueue.Contains(neighbor) || !preIsPost)) 
                        collapseItems.Add((neighbor, AdjacencyMatrix.CalcEntropy(remainingChoiceCount)));
                    
                    if (preIsPost) continue;
                    
                    /*
                     * Update neighbor.
                     */
                    
                    if (remainingChoiceCount == 1)
                    {
                        SetOccupied(neighbor, latestChoice);
                    }

                    if (remainingChoiceCount == 0)
                    {
                        /*
                         * Conflict...
                         */
                        if (ignoreConflict) continue;
                        var p = PatternMatrix;
                        Debug.Log(GridToString(_atomGrid));
                        throw new NoMoreChoicesException($"No more choices remain for {neighbor}");
                    }
                    
                    patternWave.Set(neighbor, post);

                    if (!enqueued.Contains(neighbor)) 
                        propQueue.Enqueue(neighbor);
                }
            }

            Debug.Log($"Time taken on union: {unionTime}");
            return collapseItems;
        }

        public override Vector3Int GetExtent()
        {
            return _atomGrid.GetExtent();
        }
    }
    
}