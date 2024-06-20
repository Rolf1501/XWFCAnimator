using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace XWFC
{
    public class ExpressiveWFC
    {
        private readonly TileSet _tileSet;
        public Vector3Int GridExtent;
        private Vector3 _startCoord;
        private readonly Dictionary<int, float> _defaultWeights;
        private float _progress = 0;
        public readonly AdjacencyMatrix AdjMatrix;
        public GridManager GridManager;
        private readonly float _maxEntropy;
        private CollapsePriorityQueue _collapseQueue;
        public readonly Vector3Int[] Offsets;
        private Queue<Propagation> _propQueue = new();
        private SavePointManager _savePointManager;
        private int _counter;
        public Stack<Occupation> OccupationLog = new();
        private bool _forceCompleteTiles;
        private SavePoint _rootSave;

        #nullable enable
        public ExpressiveWFC(TileSet tileSet, HashSetAdjacency adjacencyConstraints,
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
            _collapseQueue = new CollapsePriorityQueue();
            
            Offsets = OffsetFactory.GetOffsets(3);

            Clean();
            _rootSave = new SavePoint(GridManager, _collapseQueue, _counter);
        }

        public ExpressiveWFC(TileSet tileSet, Vector3Int extent, InputGrid[] inputGrids, Dictionary<int, float>? defaultWeights = null, bool forceCompleteTiles = true)
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
            _rootSave = new SavePoint(GridManager, _collapseQueue, _counter);

        }

        public ExpressiveWFC(AdjacencyMatrix adjacencyMatrix, Grid<int> seededGrid, int emptyId, bool forceCompleteTiles = true)
        {
            AdjMatrix = adjacencyMatrix;
            GridExtent = seededGrid.GetExtent();
            _forceCompleteTiles = forceCompleteTiles;
            _defaultWeights = adjacencyMatrix.TileWeigths;
            _maxEntropy = AdjMatrix.MaxEntropy();
            
            Clean();
            
            RemoveEmptyInSeededGrid(ref seededGrid, emptyId);
            var blockedCells = BlockOccupiedSeeds(ref seededGrid);
            EliminateIncompleteBlockedCellNeighbors(blockedCells);
            
            GridManager = new GridManager(seededGrid, _defaultWeights, _maxEntropy);
            
            // Grid's shape changes because of this, so eliminate all tiles that do not fit.
            // The only cells that need to be checked are those adjacent to replaced cell ids.
            //TODO:
            
            _rootSave = new SavePoint(GridManager, _collapseQueue, _counter);
        }

        private HashSet<Vector3Int> BlockOccupiedSeeds(ref Grid<int> seededGrid)
        {
            var minId = Math.Min(seededGrid.DefaultFillValue, _defaultWeights.Keys.Min()) - 1;
            var blockedCells = new HashSet<Vector3Int>();

            // For a seeded grid, the seeded tiles do not correspond to any tile in the tile set used for the seeded grid.
            // So, set the occupied cells to an id that is not part of the tile set. 
            // That prevents propagation from accessing those cells.
            var blockedCellId = minId;
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

        private void RemoveEmptyInSeededGrid(ref Grid<int> grid, int emptyId)
        {
            var e = grid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        if (grid.Get(x,y,z) == emptyId) grid.Set(x,y,z,grid.DefaultFillValue);
                    }
                }
            }
        }

        private void EliminateIncompleteBlockedCellNeighbors(HashSet<Vector3Int> cells)
        {
            foreach (var cell in cells)
            {
                foreach (var offset in Offsets)
                {
                    var n = cell + offset;
                    if (GridManager.WithinBounds(n)) EliminateIncompleteAtoms(n);
                }
                
            }
        }
        
        private void EliminateIncompleteTiles()
        {
            /*
             * After a grid is superimposed, the cells at the borders allow atoms of tile that would extend beyond the grid's boundaries.
             * If only complete tiles are desired, eliminate all atoms from the cells at the border that would not satisfy that property.
             * This is akin to eliminating choices from a cell, thus propagation waves are performed to propagate those effects.
             * Thus:
             * For each atom in the boundary cells: Eliminate atom if corresponding tile does not fit. Propagate. Repeat.
             */
            
            // Z layers.
            for (int x = 0; x < GridExtent.x; x++)
            {
                for (int y = 0; y < GridExtent.y; y++)
                {
                    var (zStart, zEnd) = (0, GridExtent.z - 1);
                    var start = new Vector3Int(x, y, zStart);
                    var end = new Vector3Int(x, y, zEnd);
                    EliminateIncompleteAtoms(start);
                    EliminateIncompleteAtoms(end);
                }
            }

            // Y layers.
            for (int x = 0; x < GridExtent.x; x++)
            {
                for (int z = 0; z < GridExtent.z; z++)
                {
                    var (yStart, yEnd) = (0, GridExtent.y - 1);
                    var start = new Vector3Int(x, yStart, z);
                    var end = new Vector3Int(x, yEnd, z);
                    EliminateIncompleteAtoms(start);
                    EliminateIncompleteAtoms(end);
                }
            }
            
            // X layers.
            for (int y = 0; y < GridExtent.y; y++)
            {
                for (int z = 0; z < GridExtent.z; z++)
                {
                    var (xStart, xEnd) = (0, GridExtent.x - 1);
                    var start = new Vector3Int(xStart, y, z);
                    var end = new Vector3Int(xEnd, y, z);
                    EliminateIncompleteAtoms(start);
                    EliminateIncompleteAtoms(end);
                }
            }
        }
        
        private void EliminateIncompleteAtoms(Vector3Int coord)
        {
            var choices = GridManager.ChoiceBooleans.Get(coord);
            for (int i = 0; i < choices.Length; i++)
            {
                if (!choices[i] || TileFits(i, coord)) continue;
                // If the allowed atom's tile does not fit, eliminate from choices and propagate. 
                choices[i] = false;
                GridManager.ChoiceBooleans.Set(coord, choices);
            }
            var choiceList = new List<int>();
            for (var j = 0; j < choices.Length; j++)
            {
                if (choices[j]) choiceList.Add(j);
            }
            _propQueue.Enqueue(new Propagation(choiceList.ToArray(), coord));
            Propagate();
        }

        private bool TileFits(int atomId, Vector3 coord)
        {
            var  (tileId, atomCoord, _) = AdjMatrix.AtomMapping.GetKey(atomId);
            var tileSource = coord - atomCoord;
            var tileEnd = tileSource + _tileSet[tileId].Extent - new Vector3(1, 1, 1);
            return GridManager.WithinBounds(tileSource) && GridManager.WithinBounds(tileEnd);
        }

        private Vector3 CenterCoord()
        {
            return Vector3Util.Mult(GridExtent, new Vector3(0.5f, 0, 0.5f));
        }

        private void Clean()
        {
            CleanGrids();
            CleanState();
            CleanIncompleteTiles();
        }

        private void CleanState()
        {
            _collapseQueue = new CollapsePriorityQueue();
            _collapseQueue.Insert(_startCoord, GridManager.Entropy.Get(_startCoord));
            _savePointManager = new SavePointManager();

            _counter = 0;
            _progress = 0;

            _savePointManager.Save(_progress, GridManager, _collapseQueue, _counter);
        }

        private void CleanGrids()
        {
            var (x, y, z) = Vector3Util.CastInt(GridExtent);
            GridManager = new GridManager(x, y, z);
            GridManager.InitEntropy(_maxEntropy);
            GridManager.InitChoiceWeights(_defaultWeights);
            _startCoord = CenterCoord();
        }

        private void CleanIncompleteTiles()
        {
            if (_forceCompleteTiles)
            {
                EliminateIncompleteTiles();
            }
        }

        public void CollapseAutomatic()
        {
            while (!_collapseQueue.IsDone())
            {
                CollapseOnce();
            }

            Debug.Log("All done!");
        }

        public HashSet<Occupation> CollapseOnce()
        {
            /*
             * Performs a single collapse and outputs the affected cells' coordinates.
             * It is up to the caller to refer to the grid to find the cell's contents.
             */
            var affectedCells = new HashSet<Occupation>();
            if (_collapseQueue.IsDone()) return affectedCells;

            var head = _collapseQueue.DeleteHead();
            if (CollapseList.IsDefaultCoord(head.Coord)) return affectedCells;
            
            Vector3 coll = head.Coord;
            if (!GridManager.WithinBounds(coll)) return affectedCells;
            
            while ((!GridManager.WithinBounds(coll) || GridManager.Grid.IsChosen(coll)) && !_collapseQueue.IsDone())
            {
                coll = _collapseQueue.DeleteHead().Coord;
            }

            var (x, y, z) = Vector3Util.CastInt(coll);

            Vector3 tCoord;
            int tId;
            try
            {
                (tCoord, tId) = Collapse(x, y, z);
            }
            catch (NoMoreChoicesException)
            {
                int undoneCells = RestoreSavePoint();
                Debug.Log($"Restoring to earlier state... At progress {_progress}");
                
                // When rewinding, pass the coordinates of the cells that changed.
                // The caller can then refer the grid manager to find what the values of the cells should be. 
                while (undoneCells > 0 && OccupationLog.Count > 0)
                {
                    affectedCells.Add(OccupationLog.Pop());
                    undoneCells--;
                }
                return affectedCells;
            }

            Propagate();
            var occupation = new Occupation(tCoord, tId);
            affectedCells.Add(occupation);
            
            // Keep track of cell occupation by atom placement.
            OccupationLog.Push(occupation);
            
            return affectedCells;
        }

        private (Vector3, int) Collapse(int x, int y, int z)
        {
            /*
             * Collapse the cell at the given coordinate.
             */
            var coord = new Vector3(x, y, z);
            if (!GridManager.WithinBounds(coord)) return (new Vector3(), -1);

            if (GridManager.Grid.IsChosen(x, y, z)) return (new Vector3(), -1);

            var choiceId = Choose(x, y, z); // Throws exception; handled by CollapsedOnce.
            var (_, _, tO) = AdjMatrix.AtomMapping.Get(choiceId);

            SetOccupied(coord, choiceId);
            return (coord, choiceId);
        }

        private void SetOccupied(Vector3 coord, int id)
        {
            GridManager.Grid.Set(coord, id);
            GridManager.Entropy.Set(coord, AdjacencyMatrix.CalcEntropy(1));
            _propQueue.Enqueue(new Propagation(new int[] { id }, coord));
            
            // Whenever a cell is set to be occupied, progress is made and needs to be updated.
            UpdateProgress();
        }

        private int Choose(int x, int y, int z)
        {
            /*
             * Finds an allowed atom to place at the given coordinate.
             */
            var choiceBooleans = GridManager.ChoiceBooleans.Get(x, y, z);
            var choiceIds = GridManager.ChoiceIds.Get(x, y, z);
            var choiceWeights = GridManager.ChoiceWeights.Get(x, y, z);

            int chosenIndex = RandomChoice(choiceBooleans, choiceWeights);

            // TODO: move this to propagation instead for early conflict detection.
            if (chosenIndex < 0) throw new NoMoreChoicesException($"No more choice remain for cell {x}, {y}, {z}.");

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
            GridManager = savePoint.GridManager.Deepcopy();
            _collapseQueue = savePoint.CollapseQueue.Copy();
            _counter = savePoint.Counter;

        
            // Important: clean the propagation queue.
            _propQueue.Clear();
        }

        private static int RandomChoice(bool[] choices, float[] weights)
        {
            /*
             * Chooses a random atom from the set of allowed atom choices.
             */
            int[] choiceBooleansIntMask = choices.Select(b => b ? 1 : 0).ToArray();
            float[] choiceWeights = weights.Zip(choiceBooleansIntMask, (w, b) => w * b).ToArray();
            float total = choiceWeights.Sum();

            if (total <= 0) return -1;

            double random = new Random().NextDouble() * total;
            float acc = 0;
            for (int i = 0; i < choiceWeights.Length; i++)
            {
                acc += choiceWeights[i];
                if (acc >= random) return i;
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
            PrintProgressUpdate(_progress);

            _savePointManager.Save(_progress, GridManager, _collapseQueue, _counter);
        }

        private float CalcProgress()
        {
            return 100 * _counter / (GridExtent.x * GridExtent.y * GridExtent.z);
        }

        private bool[] UnionChoices(int[] cs, Vector3 offset)
        {
            var remainingChoices = new bool[AdjMatrix.GetNAtoms()];

            // TODO: consider the use of SIMD.
            // Find the union of allowed neighbors terminals given the set of choices of the current cell.
            foreach (int c in cs)
            {
                bool[] cAdj = AdjMatrix.GetAdj(offset, c);
                for (int i = 0; i < cAdj.Length; i++) remainingChoices[i] |= cAdj[i];
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

        public void Propagate()
        {
            while (_propQueue.Count > 0)
            {
                var p = _propQueue.Dequeue();
                var (cs, coord) = (p.Choices, p.Coord);
                
                foreach (Vector3 offset in Offsets)
                {
                    Vector3 n = coord + offset;

                    // No need to consider out of bounds or occupied neighbors.
                    if (!GridManager.WithinBounds(n) || GridManager.Grid.IsChosen(n))
                        continue;

                    var remainingChoices = UnionChoices(cs, offset);

                    // Find the set of choices currently allowed for the neighbor.
                    float[] neighborWChoices = GridManager.ChoiceWeights.Get(n);
                    bool[] neighborBChoices = GridManager.ChoiceBooleans.Get(n);

                    var post = new bool[AdjMatrix.GetNAtoms()];
                    for (int i = 0; i < remainingChoices.Length; i++)
                        post[i] = neighborBChoices[i] & remainingChoices[i];

                    foreach (int c in cs)
                    {
                        float[] cW = AdjMatrix.GetAdjW(offset, c);
                        for (int i = 0; i < neighborWChoices.Length; i++) neighborWChoices[i] += cW[i];
                    }

                    GridManager.ChoiceWeights.Set(n, neighborWChoices);

                    // If pre is not post, update.
                    if (!ArrayEquals(neighborBChoices, post))
                    {
                        GridManager.ChoiceBooleans.Set(n, post);

                        // Calculate entropy and get indices of allowed neighbor terminals.
                        var neighborWChoicesI = new List<int>();

                        for (int i = 0; i < post.Length; i++)
                            if (post[i])
                                neighborWChoicesI.Add(i);

                        var nChoices = neighborWChoicesI.Count;

                        GridManager.Entropy.Set(n, AdjacencyMatrix.CalcEntropy(nChoices));

                        // If all terminals are allowed, then the entropy does not change. There is nothing to propagate.
                        if (Math.Abs(GridManager.Entropy.Get(n) - _maxEntropy) < 0.0001)
                            continue;

                        if (nChoices == 1)
                        {
                            SetOccupied(n, neighborWChoicesI[0]);
                        }

                        if (!GridManager.Grid.IsChosen(n))
                            _propQueue.Enqueue(new Propagation(neighborWChoicesI.ToArray(), n));
                    }

                    if (!GridManager.Grid.IsChosen(n))
                        _collapseQueue.Insert(n, GridManager.Entropy.Get(n));
                }
            }
        }

        public bool IsDone()
        {
            return _collapseQueue.IsDone();
        }

        public void UpdateExtent(Vector3Int extent)
        {
            if (GridExtent != extent)
            {
                GridExtent = extent;
                var (x, y, z) = Vector3Util.CastInt(extent);
                _rootSave = new SavePoint(new GridManager(x, y, z), new CollapsePriorityQueue(), 0); 
            }
            Reset();
        }

        public void Reset()
        {
            LoadSavePoint(_rootSave);
            // Clean();
        }
    }

    public record Propagation
    {
        public int[] Choices; 
        public Vector3 Coord;

        public Propagation(int[] choices, Vector3 coord)
        {
            Choices = choices;
            Coord = coord;
        }
    };

    public record Occupation
    {
        /*
         * Record for storing information about cell occupation.
         * Contains a coordinate, atom id and addition flag (true = addition, false = removal).
         */
        public Vector3 Coord;
        public int Id;
        public bool Addition; 
        
        public Occupation(Vector3 coord, int id, bool addition = true)
        {
            Coord = coord;
            Id = id;
            Addition = addition;
        }
    }
}