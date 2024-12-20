using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace XWFC
{
    public class XwfcStm
    {
        private readonly TileSet _tileSet;
        public Vector3Int GridExtent;
        protected Vector3Int StartCoord;
        private readonly Dictionary<int, float> _defaultWeights;
        private float _progress = 0;
        public AdjacencyMatrix AdjMatrix;
        private GridManager _gridManager;
        private readonly float _maxEntropy;
        protected CollapsePriorityQueue CollapseQueue;
        public readonly Vector3Int[] Offsets;
        private Queue<Vector3Int> _propQueue = new();
        private SavePointManager _savePointManager;
        private int _counter;
        private bool _forceCompleteTiles;
        private SavePoint _rootSave;
        public int RandomSeed = 3;
        private Random _random;

        #nullable enable
        public XwfcStm(TileSet tileSet, HashSetAdjacency adjacencyConstraints,
            Vector3Int gridExtent, Dictionary<int, float>? defaultWeights = null, bool forceCompleteTiles = true)
        {
            /*
             * Constructor for XWFC with predefined adjacency constraints between NUTs.
             */
            _tileSet = tileSet;
            GridExtent = gridExtent;
            _forceCompleteTiles = forceCompleteTiles;
            
            AdjMatrix = new AdjacencyMatrix(adjacencyConstraints, _tileSet, defaultWeights);
            
            _maxEntropy = AdjMatrix.MaxEntropy();
            _defaultWeights = AdjMatrix.TileWeigths;
            CollapseQueue = new CollapsePriorityQueue();
            CollapseQueue.Insert(StartCoord, _maxEntropy);
            
            Offsets = OffsetFactory.GetOffsets(3);

            Clean();
            _rootSave = new SavePoint(_gridManager, CollapseQueue, _counter);
        }

        public virtual Grid<int> GetGrid()
        {
            return _gridManager.Grid;
        }

        public virtual Vector3Int GetExtent()
        {
            return _gridManager.Grid.GetExtent();
        }

        public virtual Grid<bool[]> GetWave()
        {
            return _gridManager.Wave;
        }

        public XwfcStm(TileSet tileSet, Vector3Int extent, SampleGrid[] inputGrids, Dictionary<int, float>? defaultWeights = null, bool forceCompleteTiles = true)
        {
            /*
             * Constructor for XWFC with a list of grids with preset tile ids and instance ids to learn from.
             * Note that each cell may contain multiple tiles as valid occupants, due to ambiguity.
             * One layer of abstraction.
             */
            GridExtent = extent;
            _tileSet = tileSet;
            _forceCompleteTiles = forceCompleteTiles;
            AdjMatrix = new AdjacencyMatrix(tileSet, inputGrids, defaultWeights);
            _maxEntropy = AdjMatrix.MaxEntropy();
            _defaultWeights = AdjMatrix.TileWeigths;
            
            Offsets = OffsetFactory.GetOffsets(3);
            Clean();
            _rootSave = new SavePoint(_gridManager, CollapseQueue, _counter);
        }

        public XwfcStm(AdjacencyMatrix adjacencyMatrix, ref Grid<int> seededGrid, int randomSeed, bool forceCompleteTiles = true)
        {
            AdjMatrix = adjacencyMatrix;
            GridExtent = seededGrid.GetExtent();
            _tileSet = adjacencyMatrix.TileSet;
            _forceCompleteTiles = forceCompleteTiles;
            _defaultWeights = adjacencyMatrix.TileWeigths;
            _maxEntropy = AdjMatrix.MaxEntropy();
            
            Debug.Log($"Random Seed: {RandomSeed}");
            // 316068766
            //874075968
            
            Offsets = OffsetFactory.GetOffsets(3);
            
            _gridManager = new GridManager(seededGrid, _defaultWeights, _maxEntropy);
            StartCoord = CenterCoord();
            CleanState();
            
            RemoveEmpty(ref seededGrid);
            var blockedCells = BlockOccupiedSeeds(ref seededGrid);
            // seededGrid = EliminateIncompleteBlockedCellNeighbors(blockedCells, seededGrid);
            
            _gridManager.Grid = seededGrid;
            // CleanIncompleteTiles();
            
            _rootSave = new SavePoint(_gridManager, CollapseQueue, _counter);
            
            UpdateRandom(randomSeed);
        }

        public static int BlockedCellId(int defaultFillValue, IEnumerable<int> tileIds)
        {
            var minId = Math.Min(defaultFillValue, tileIds.Min()) - 1;
            return minId;
        }

        private HashSet<Vector3Int> BlockOccupiedSeeds(ref Grid<int> seededGrid)
        {
            var blockedCells = new HashSet<Vector3Int>();

            var blockedCellId = BlockedCellId(seededGrid.DefaultFillValue, _defaultWeights.Keys);

            // For a seeded grid, the seeded tiles do not correspond to any tile in the tile set used for the seeded grid.
            // So, set the occupied cells to an id that is not part of the tile set. 
            // That prevents propagation from accessing those cells.
            var e = seededGrid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        var value = seededGrid.Get(x, y, z);
                        
                        if (value == seededGrid.DefaultFillValue) continue;
                        
                        seededGrid.Set(x,y,z, blockedCellId);
                        blockedCells.Add(new Vector3Int(x, y, z));
                    }
                }
            }

            return blockedCells;
        }

        public void RemoveEmpty(ref Grid<int> grid)
        {
            var emptyIds = AdjMatrix.GetEmptyAtomIds();
            var e = grid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        if (emptyIds.Contains(grid.Get(x, y, z)))
                        {
                            grid.Set(x,y,z,grid.DefaultFillValue);
                        }
                    }
                }
            }
        }

        private Grid<int> EliminateIncompleteBlockedCellNeighbors(HashSet<Vector3Int> cells, Grid<int> seededGrid)
        {
            var neighbors = new HashSet<Vector3Int>();
            foreach (var cell in cells)
            {
                foreach (var offset in Offsets)
                {
                    var n = cell + offset;
                    if (!neighbors.Contains(n) && !cells.Contains(n) && _gridManager.WithinBounds(n))
                    {
                        neighbors.Add(n);
                    }
                }
            }
            
            // Elimination is expensive, so only perform it on cells for which it's strictly necessary.
            foreach (var neighbor in neighbors)
            {
                seededGrid = EliminateIncompleteAtoms(neighbor, seededGrid);
            }

            return seededGrid;
        }
        
        private Grid<int> EliminateIncompleteTiles(Grid<int> grid)
        {
            /*
             * After a grid is superimposed, the cells at the borders allow atoms of tile that would extend beyond the grid's boundaries.
             * If only complete tiles are desired, eliminate all atoms from the cells at the border that would not satisfy that property.
             * This is akin to eliminating choices from a cell, thus propagation waves are performed to propagate those effects.
             * Thus:
             * For each atom in the boundary cells: Eliminate atom if corresponding tile does not fit. Propagate. Repeat.
             */

            var e = grid.GetExtent();
            
            // Z layers.
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    var (zStart, zEnd) = (0, e.z - 1);
                    var start = new Vector3Int(x, y, zStart);
                    var end = new Vector3Int(x, y, zEnd);
                    grid = EliminateIncompleteAtoms(start, grid);
                    grid = EliminateIncompleteAtoms(end, grid);
                }
            }

            // Y layers.
            for (int x = 0; x < e.x; x++)
            {
                for (int z = 0; z < e.z; z++)
                {
                    var (yStart, yEnd) = (0, e.y - 1);
                    var start = new Vector3Int(x, yStart, z);
                    var end = new Vector3Int(x, yEnd, z);
                    grid = EliminateIncompleteAtoms(start, grid);
                    grid = EliminateIncompleteAtoms(end, grid);
                }
            }
            
            // X layers.
            for (int y = 0; y < e.y; y++)
            {
                for (int z = 0; z < e.z; z++)
                {
                    var (xStart, xEnd) = (0, e.x - 1);
                    var start = new Vector3Int(xStart, y, z);
                    var end = new Vector3Int(xEnd, y, z);
                    grid = EliminateIncompleteAtoms(start, grid);
                    grid = EliminateIncompleteAtoms(end, grid);
                }
            }

            return grid;
        }
        
        private Grid<int> EliminateIncompleteAtoms(Vector3Int coord, Grid<int> grid)
        {
            var choices = _gridManager.Wave.Get(coord);
            for (int i = 0; i < choices.Length; i++)
            {
                if (!choices[i] || TileFits(i, coord, grid)) continue;
                // If the allowed atom's tile does not fit, eliminate from choices and propagate. 
                choices[i] = false;
            }
            _gridManager.Wave.Set(coord, choices);
            var choiceList = new List<int>();
            for (var j = 0; j < choices.Length; j++)
            {
                if (choices[j]) choiceList.Add(j);
            }
            _propQueue.Enqueue(coord);
            Propagate();

            return grid;
        }

        private bool TileFits(int atomId, Vector3Int coord, Grid<int> seededGrid)
        {
            var  (tileId, atomCoord, _) = AdjMatrix.AtomMapping.GetKey(atomId);
            var tileSource = coord - atomCoord;
            var tileEnd = tileSource + AdjMatrix.TileSet[tileId].Extent - new Vector3Int(1, 1, 1);
            
            if (!(_gridManager.WithinBounds(tileSource) && _gridManager.WithinBounds(tileEnd))) return false;

            // Also check if all other cells in the tile's area are not blocked.
            for (int x = tileSource.x; x <= tileEnd.x; x++)
            {
                for (int y = tileSource.y; y <= tileEnd.y; y++)
                {
                    for (int z = tileSource.z; z <= tileEnd.z; z++)
                    {
                        if (seededGrid.Get(x, y, z) != seededGrid.DefaultFillValue) return false;
                    }
                }
            }
            
            return true;
        }

        private Vector3Int CenterCoord()
        {
            return Vector3Util.VectorToVectorInt(Vector3Util.Mult(GridExtent, new Vector3(0.5f, 0, 0.5f)));
        }

        private void Clean()
        {
            CleanGrids();
            CleanState();
            // CleanIncompleteTiles();
        }

        private void CleanState()
        {
            CollapseQueue = new CollapsePriorityQueue();
            CollapseQueue.Insert(StartCoord, _gridManager.Entropy.Get(StartCoord));
            _savePointManager = new SavePointManager();

            _counter = 0;
            _progress = 0;

            _savePointManager.Save(_progress, _gridManager, CollapseQueue, _counter);
        }

        private void CleanGrids()
        {
            var (x, y, z) = Vector3Util.CastInt(GridExtent);
            _gridManager = new GridManager(x, y, z);
            _gridManager.InitEntropy(_maxEntropy);
            _gridManager.InitChoiceWeights(_defaultWeights);
            StartCoord = CenterCoord();
        }

        private void CleanIncompleteTiles()
        {
            if (_forceCompleteTiles)
            {
                _gridManager.Grid = EliminateIncompleteTiles(_gridManager.Grid);
            }
        }

        public void CollapseAutomatic()
        {
            while (!CollapseQueue.IsDone())
            {
                CollapseOnce();
            }

            Debug.Log("All done!");
        }

        public virtual void CollapseOnce()
        {
            /*
             * Performs a single collapse and outputs the affected cells' coordinates.
             * It is up to the caller to refer to the grid to find the cell's contents.
             */
            if (CollapseQueue.IsDone()) return;

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
            }
            catch (NoMoreChoicesException)
            {
                RestoreSavePoint();
                Debug.Log($"Restoring to earlier state... At progress {_progress}");
                
                // When rewinding, pass the coordinates of the cells that changed.
                // The caller can then refer the grid manager to find what the values of the cells should be. 
                
                return;
            }

            var collapseItems = Propagate();
            foreach (var (coord, entropy) in collapseItems)
            {
                CollapseQueue.Insert(coord, entropy);
            }
        }

        protected virtual void Collapse(Vector3Int coord)
        {
            /*
             * Collapse the cell at the given coordinate.
             */
            if (!_gridManager.WithinBounds(coord)) return;

            if (_gridManager.Grid.IsOccupied(coord)) return;

            var choiceId = Choose(coord); // Throws exception; handled by CollapsedOnce.

            SetOccupied(coord, choiceId);
        }

        protected virtual void SetOccupied(Vector3Int coord, int id)
        {
            _gridManager.Grid.Set(coord, id);
            var updatedWave = new bool[AdjMatrix.GetNAtoms()];
            updatedWave[id] = true;
            _gridManager.Wave.Set(coord, updatedWave);
            _gridManager.Entropy.Set(coord, AdjacencyMatrix.CalcEntropy(1));
            _propQueue.Enqueue(coord);
            
            // Whenever a cell is set to be occupied, progress is made and needs to be updated.
            UpdateProgress();
        }

        private int Choose(Vector3Int coord)
        {
            /*
             * Finds an allowed atom to place at the given coordinate.
             */
            var wave = _gridManager.Wave.Get(coord);
            var choiceIds = _gridManager.ChoiceIds.Get(coord);
            var choiceWeights = _gridManager.ChoiceWeights.Get(coord);

            int chosenIndex = RandomChoice(wave, choiceWeights, _random);

            // TODO: move this to propagation instead for early conflict detection.
            if (chosenIndex < 0) throw new NoMoreChoicesException($"No more choice remain for cell {coord}.");

            return choiceIds[chosenIndex];
        }

        private int RestoreSavePoint()
        {
            Debug.Log("Restoring savepoint...");
            SavePoint savePoint = _savePointManager.Restore();
            
            int undoneCells = _counter - savePoint.Counter;
            
            LoadSavePoint(savePoint);
            return undoneCells;
        }

        private void LoadSavePoint(SavePoint savePoint)
        {
            _gridManager = savePoint.GridManager.Deepcopy();
            CollapseQueue = savePoint.CollapseQueue.Copy();
            _counter = savePoint.Counter;
       
            // Important: clean the propagation queue.
            _propQueue.Clear();
        }

        public void UpdateRandom(int newSeed)
        {
            Debug.Log($"Updated random to: {newSeed}");
            RandomSeed = newSeed;
            _random = new Random(RandomSeed);
        }

        protected static int RandomChoice(IEnumerable<bool> choices, IEnumerable<float> weights, Random? random = null)
        {
            /*
             * Chooses a random atom from the set of allowed atom choices.
             */
            var choiceBooleansIntMask = choices.Select(b => b ? 1 : 0).ToArray();
            var choiceWeights = weights.Zip(choiceBooleansIntMask, (w, b) => w * b).ToArray();
            var total = choiceWeights.Sum();

            if (total <= 0) return -1;

            random ??= new Random();
            var randChoice = random.NextDouble() * total;
            float acc = 0;
            for (int i = 0; i < choiceWeights.Length; i++)
            {
                acc += choiceWeights[i];
                if (acc >= randChoice) return i;
            }

            return -1;
        }

        private void PrintProgressUpdate(float progress, int percentageIntervals = 10)
        {
            if (_progress % percentageIntervals == 0)
                Debug.Log(
                    $"STATUS: {progress}%. Processed {_counter}/{GridExtent.x * GridExtent.y * GridExtent.z} cells");
        }

        private void UpdateProgress(int increment = 1)
        {
            _counter += increment;
            _progress = CalcProgress();
            // PrintProgressUpdate(_progress);

            _savePointManager.Save(_progress, _gridManager, CollapseQueue, _counter);
        }

        private float CalcProgress()
        {
            return 100 * _counter / (GridExtent.x * GridExtent.y * GridExtent.z);
        }

        private bool[] UnionChoices(int[] cs, Vector3Int offset)
        {
            // var remainingChoices = new bool[AdjMatrix.GetNAtoms()];
            // // TODO: consider the use of SIMD.
            // // Find the union of allowed neighbors terminals given the set of choices of the current cell.
            // foreach (int c in cs)
            // {
            //     bool[] cAdj = AdjMatrix.GetAdj(offset, c);
            //     for (int i = 0; i < cAdj.Length; i++) remainingChoices[i] |= cAdj[i];
            // }

            var choices = AdjMatrix.GetRowVectors(cs[0], offset);
            for (var i = 1; i < cs.Length; i++)
            {
                var other = AdjMatrix.GetRowVectors(cs[i], offset);
                choices = Vectorizor.Or(choices, other);
            }

            var remainingChoices = new bool[AdjMatrix.GetNAtoms()];
            for (var j = 0; j < AdjMatrix.GetNAtoms(); j++)
            {
                remainingChoices[j] = Vectorizor.GetAtIndex(j, choices) == 1;
            }
            return remainingChoices;
        }

        private static bool ArrayEquals<T>(T[] pre, T[] post)
        {
            // No need to continue if there was no change.
            for (int i = 0; i < pre.Count(); i++)
            {
                var a = pre[i];
                var b = post[i];
                if (!Equals(a, b)) return false;
            }
            return true;
        }

        protected virtual HashSet<(Vector3Int, float)> Propagate()
        {
            var collapseItems = new HashSet<(Vector3Int, float)>();
            while (_propQueue.Count > 0)
            {
                var coord = _propQueue.Dequeue();
                var wave = _gridManager.Wave.Get(coord);
                var list = new List<int>();
                for (var i = 0; i < wave.Length; i++)
                {
                    if (wave[i]) list.Add(i);
                }

                var cs = list.ToArray();
                
                foreach (var offset in Offsets)
                {
                    var n = coord + offset;

                    // No need to consider out of bounds or occupied neighbors.
                    if (!_gridManager.WithinBounds(n) || _gridManager.Grid.IsOccupied(n))
                        continue;

                    var remainingChoices = UnionChoices(cs, offset);

                    // Find the set of choices currently allowed for the neighbor.
                    float[] neighborWChoices = _gridManager.ChoiceWeights.Get(n);
                    bool[] neighborWave = _gridManager.Wave.Get(n);

                    var post = new bool[AdjMatrix.GetNAtoms()];
                    for (int i = 0; i < remainingChoices.Length; i++)
                        post[i] = neighborWave[i] & remainingChoices[i];

                    foreach (int c in cs)
                    {
                        float[] cW = AdjMatrix.GetAdjW(offset, c);
                        for (int i = 0; i < neighborWChoices.Length; i++) neighborWChoices[i] += cW[i];
                    }

                    _gridManager.ChoiceWeights.Set(n, neighborWChoices);

                    // If pre is not post, update.
                    if (!ArrayEquals(neighborWave, post))
                    {
                        _gridManager.Wave.Set(n, post);

                        // Calculate entropy and get indices of allowed neighbor terminals.
                        var neighborWChoicesI = new List<int>();

                        for (int i = 0; i < post.Length; i++)
                            if (post[i])
                                neighborWChoicesI.Add(i);

                        var nChoices = neighborWChoicesI.Count;

                        _gridManager.Entropy.Set(n, AdjacencyMatrix.CalcEntropy(nChoices));

                        // If all terminals are allowed, then the entropy does not change. There is nothing to propagate.
                        if (Math.Abs(_gridManager.Entropy.Get(n) - _maxEntropy) < 0.0001)
                            continue;

                        if (nChoices == 1)
                        {
                            SetOccupied(n, neighborWChoicesI[0]);
                        }

                        if (!_gridManager.Grid.IsOccupied(n))
                            _propQueue.Enqueue(n);
                    }

                    if (!_gridManager.Grid.IsOccupied(n))
                        // CollapseQueue.Insert(n, _gridManager.Entropy.Get(n));
                        collapseItems.Add((n, _gridManager.Entropy.Get(n)));
                }
            }

            return collapseItems;
        }

        public bool IsDone()
        {
            return CollapseQueue.IsDone();
        }

        public virtual void UpdateExtent(Vector3Int extent)
        {
            GridExtent = extent;
            Reset();
        }

        protected virtual void Reset()
        {
            var q = new CollapsePriorityQueue();
            q.Insert(StartCoord, _maxEntropy);
            _rootSave = new SavePoint(new GridManager(GridExtent.x, GridExtent.y, GridExtent.z), q, 0);
            LoadSavePoint(_rootSave);
            // Clean();
        }
    }
}