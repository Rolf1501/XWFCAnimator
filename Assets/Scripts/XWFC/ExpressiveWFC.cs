using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Output;
using Random = System.Random;

namespace XWFC
{
    public class ExpressiveWFC
    {
        private readonly Dictionary<int, Terminal> _terminals;
        public Vector3 GridExtent;
        private Vector3 _startCoord;
        private readonly Dictionary<int, float> _defaultWeights;
        private float _progress = 0;
        public readonly AdjacencyMatrix AdjMatrix;
        public GridManager GridManager;
        private readonly float _maxEntropy;
        private CollapsePriorityQueue _collapseQueue;
        // private CollapseQueue _collapseQueue;
        public readonly Vector3[] Offsets;
        private Queue<Propagation> _propQueue = new();
        private SavePointManager _savePointManager;
        private int _counter;
        public Stack<Occupation> OccupationLog = new();
        public bool WriteResults;
        private OutputParser _outputParser;
        private static Random _seededRandom;
        private int _seed;


        #nullable enable
        public ExpressiveWFC(Dictionary<int, Terminal> terminals, HashSetAdjacency adjacencyConstraints,
            Vector3 gridExtent, Dictionary<int, float>? defaultWeights = null, bool writeResults = false, int? seed = null)
        {
            _terminals = terminals;
            GridExtent = gridExtent;
            
            _seed = seed ?? new Random().Next();
            _seededRandom = new Random(_seed);
            Debug.Log($"Seed:{_seed}");
            
            int[] keys = _terminals.Keys.ToArray();
            AdjMatrix = new AdjacencyMatrix(keys, adjacencyConstraints, _terminals);

            _maxEntropy = CalcEntropy(AdjMatrix.GetNAtoms());
            _defaultWeights = ExpandDefaultWeights(defaultWeights);
            
            Offsets = OffsetFactory.GetOffsets(3);
            
            CleanGrids(GridExtent, _defaultWeights, _maxEntropy);
            CleanState();
            
            WriteResults = writeResults;
            if (!WriteResults) return;
            var dir = Directory.GetCurrentDirectory();
            var outPath = Path.Join(dir, "/Assets/Scripts/Output");
            Debug.Log($"outpath: {outPath}");
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            
            _outputParser = new OutputParser(outPath,"output.csv");
            _outputParser.WriteConfig(GridExtent, _seed.ToString(), AdjMatrix);
        }

        private Vector3 CenterCoord()
        {
            return Vector3Util.Mult(GridExtent, new Vector3(0.5f, 0, 0.5f));
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

        private void CleanGrids(Vector3 gridExtent, Dictionary<int, float> defaultWeights, float maxEntropy)
        {
            var (x, y, z) = Vector3Util.CastInt(gridExtent);
            GridManager = new GridManager(x, y, z);
            GridManager.InitEntropy(maxEntropy);
            GridManager.InitChoiceWeights(defaultWeights);
            _startCoord = CenterCoord();
        }

        private Dictionary<int, float> ExpandDefaultWeights(Dictionary<int, float>? defaultWeights)
        {
            /*
             * Expands the default weights when the weights are specified per tile. Returns the default weights instead.
             */

            // If the number of weights is neither equal to the number of atoms or the number of terminals, set all weights of all atoms to 1.
            if (defaultWeights == null || defaultWeights.Count != _terminals.Count)
            {
                return Enumerable.Range(0, AdjMatrix.GetNAtoms()).ToDictionary(k => k, v => 1f);
            }

            // No need to expand if weights are specified per atom already.
            if (defaultWeights.Count == AdjMatrix.GetNAtoms()) return defaultWeights;

            // Otherwise, expand the weights of the terminals to atoms. 
            var aug = new Dictionary<int, float>();
            foreach (var (k, v) in defaultWeights)
            {
                var (start, end) = AdjMatrix.TileAtomRangeMapping[k];
                for (int i = start; i < end; i++)
                {
                    aug[i] = v;
                }
            }

            return aug;
        }

        private static float CalcEntropy(int nChoices)
        {
            /*
             * Entropy is taken as the log of the number of choices.
             */
            return nChoices > 0 ? (float)Math.Log(nChoices) : -1;
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
                (tCoord, tId, _) = Collapse(x, y, z);
                _propQueue.Enqueue(new Propagation(new int[] { tId }, tCoord));

                if (WriteResults)
                {
                    var action = new int[AdjMatrix.GetNAtoms()];
                    action[tId] = 1;
                    _outputParser.WriteStateAction(GridManager, action);
                    _outputParser.WriteStateActionQueue(GridManager, action, _collapseQueue);
                }
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

        private (Vector3, int, int) Collapse(int x, int y, int z)
        {
            /*
             * Collapse the cell at the given coordinate.
             */
            var coord = new Vector3(x, y, z);
            if (!GridManager.WithinBounds(coord)) return (new Vector3(), -1, -1);

            if (GridManager.Grid.IsChosen(x, y, z)) return (new Vector3(), -1, -1);

            var choiceId = Choose(x, y, z); // Throws exception; handled by CollapsedOnce.
            var (_, _, tO) = AdjMatrix.AtomMapping.Get(choiceId);

            SetOccupied(coord, choiceId);

            return (coord, choiceId, tO);
        }

        private void SetOccupied(Vector3 coord, int id)
        {
            GridManager.Grid.Set(coord, id);
            GridManager.Entropy.Set(coord, CalcEntropy(1));
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
            var updatedBool = new bool[AdjMatrix.GetNAtoms()];
            updatedBool[chosenIndex] = true;
            GridManager.ChoiceBooleans.Set(x,y,z,updatedBool);

            // TODO: move this to propagation instead for early conflict detection.
            if (chosenIndex < 0) throw new NoMoreChoicesException($"No more choice remain for cell {x}, {y}, {z}.");

            return choiceIds[chosenIndex];
        }

        private int RestoreSavePoint()
        {
            Debug.Log("Restoring savepoint...");
            SavePoint savePoint = _savePointManager.Restore();
            
            int undoneCells = _counter - savePoint.Counter;
            
            GridManager = savePoint.GridManager.Deepcopy();
            _collapseQueue = savePoint.CollapseQueue.Copy();
            _counter = savePoint.Counter;
            
            // Important: clean the propagation queue.
            _propQueue.Clear();
            return undoneCells;
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

            double random = _seededRandom.NextDouble() * total;
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

                        GridManager.Entropy.Set(n, CalcEntropy(nChoices));

                        // If all terminals are allowed, then the entropy does not change. There is nothing to propagate.
                        if (Math.Abs(GridManager.Entropy.Get(n) - _maxEntropy) < 0.0001)
                            continue;

                        if (!GridManager.Grid.IsChosen(n))
                            _propQueue.Enqueue(new Propagation(neighborWChoicesI.ToArray(), n));
                    }

                    if (!GridManager.Grid.IsChosen(n))
                        _collapseQueue.Insert(n, GridManager.Entropy.Get(n));
                        // _collapseQueue.Enqueue(n, GridManager.Entropy.Get(n));
                }
            }
        }

        public bool IsDone()
        {
            return _progress >= 100;
        }

        public void UpdateExtent(Vector3 extent)
        {
            GridExtent = extent;
            Reset();
        }

        public void Reset()
        {
            CleanGrids(GridExtent, _defaultWeights, _maxEntropy);
            CleanState();
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