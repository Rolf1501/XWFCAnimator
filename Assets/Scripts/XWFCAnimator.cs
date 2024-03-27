using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using XWFC;

public class XWFCAnimator : MonoBehaviour
{
    [SerializeField] private GameObject unitTilePrefab;
    [SerializeField] public Vector3 extent;
    public float stepSize = 10;

    public float delay;

    private float updateDeltaTime = 0;
    // public float iterationsPerSecond = 1;
    private float _iterationsPerSecondInv;
    private float _iterationDeltaTime;

    private Vector3 _unitSize;
    public static XWFCAnimator Instance { get; private set; }

    private Timer _timer = new Timer();

    private int _iterationsDone;

    private StateFlag _activeStateFlag = 0;
    
    [Flags]
    private enum StateFlag
    {
        Collapsing = 1 << 0
    }

    private ExpressiveWFC _xwfc;

    private Grid<Drawing> _drawnGrid;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this;
    }

    void Start()
    {
        _iterationsPerSecondInv = 1.0f / stepSize;
        _iterationDeltaTime = _iterationsPerSecondInv;
        var terminals = new Dictionary<int, Terminal>();
        // var extent = new Vector3(2,1,2);
        // var extent = new Vector3(3,1,3);
        // var extent = new Vector3(10,1,10);
        // var extent = new Vector3(10,10,10);
        // extent = new Vector3(20,20,20);
        // var extent = new Vector3(50, 50, 50);
        var defaultWeights = new Dictionary<int, float>();
        
        var t0 = new Terminal(
            new Vector3(2,1, 2), 
            new Color(.8f, 0, .2f) ,
            null,
            // new bool[,,]{ { {true, false}, {true, true} } }, 
            // new bool[,,]{ { {true}, {false}}, {{true}, {true} } }, 
            null
            );
        var t1 = new Terminal(new Vector3(2, 1, 2), new Color(.2f, 0, .8f), new bool[,,]{ { {true, true}, {true, false} } }, null);
        var t2 = new Terminal(new Vector3(2,1,1), new Color(.2f, .4f, .3f), null, null);
        // var t2 = new Terminal(new Vector3(2,1,1), new Color(.2f, 0, .8f), null, null);
        
        terminals.Add(0, t0);
        terminals.Add(1, t1);
        terminals.Add(2, t2);
        
        var NORTH = new Vector3(0, 0, 1);
        var SOUTH = new Vector3(0, 0, -1);
        var EAST = new Vector3(1, 0, 0);
        var WEST = new Vector3(-1, 0, 0);
        var TOP = new Vector3(0, 1, 0);
        var BOTTOM = new Vector3(0, -1, 0);
        
        var adj = new HashSetAdjacency(){
            // 0-0
            new(0, new List<Relation>() { new(0, null) }, NORTH),
            new(0, new List<Relation>() { new(0, null) }, EAST),
            new(0, new List<Relation>() { new(0, null) }, SOUTH),
            new(0, new List<Relation>() { new(0, null) }, WEST),
            new(0, new List<Relation>() { new(0, null) }, TOP),
            new(0, new List<Relation>() { new(0, null) }, BOTTOM),
            // 1-0
            new(1, new List<Relation>() { new(0, null) }, NORTH),
            new(1, new List<Relation>() { new(0, null) }, EAST),
            new(1, new List<Relation>() { new(0, null) }, SOUTH),
            new(1, new List<Relation>() { new(0, null) }, WEST),
            new(1, new List<Relation>() { new(0, null) }, TOP),
            new(1, new List<Relation>() { new(0, null) }, BOTTOM),
            // 1-1
            // new(1, new List<Relation>() { new(1, null) }, NORTH),
            // new(1, new List<Relation>() { new(1, null) }, EAST),
            // new(1, new List<Relation>() { new(1, null) }, SOUTH),
            // new(1, new List<Relation>() { new(1, null) }, WEST),
            // new(1, new List<Relation>() { new(1, null) }, TOP),
            // new(1, new List<Relation>() { new(1, null) }, BOTTOM),
            // 2-0
            new(2, new List<Relation>() { new(0, null) }, NORTH),
            new(2, new List<Relation>() { new(0, null) }, EAST),
            new(2, new List<Relation>() { new(0, null) }, SOUTH),
            new(2, new List<Relation>() { new(0, null) }, WEST),
            new(2, new List<Relation>() { new(0, null) }, TOP),
            new(2, new List<Relation>() { new(0, null) }, BOTTOM),
            // 2-1
            new(2, new List<Relation>() { new(1, null) }, NORTH),
            new(2, new List<Relation>() { new(1, null) }, EAST),
            new(2, new List<Relation>() { new(1, null) }, SOUTH),
            new(2, new List<Relation>() { new(1, null) }, WEST),
            new(2, new List<Relation>() { new(1, null) }, TOP),
            new(2, new List<Relation>() { new(1, null) }, BOTTOM),
            // 2-2
            new(2, new List<Relation>() { new(2, null) }, NORTH),
            new(2, new List<Relation>() { new(2, null) }, EAST),
            new(2, new List<Relation>() { new(2, null) }, SOUTH),
            new(2, new List<Relation>() { new(2, null) }, WEST),
            new(2, new List<Relation>() { new(2, null) }, TOP),
            new(2, new List<Relation>() { new(2, null) }, BOTTOM),
        };
        
        _xwfc = new ExpressiveWFC(terminals, adj, extent);
        Debug.Log("Initialized XWFC");
        
        // Grid for keeping track of drawn atoms.
        _drawnGrid = InitDrawGrid();
        
        // Console.WriteLine("Time taken init: {0:f2} seconds", w.ElapsedMilliseconds * 0.001);
        foreach (var o in _xwfc.Offsets)
        {
            var x = _xwfc.AdjMatrix.AtomAdjacencyMatrix[o];
            // Debug.Log(o);
            var s = "" + o + "\n";
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
        
        foreach (var (k,v) in _xwfc.AdjMatrix.AtomMapping.Dict)
        {
            Debug.Log(k.ToString() +  v.ToString());
        }
        
        // var watch = Stopwatch.StartNew();
        // wfc.AdjMatrix.AtomMapping.Dict.ToString());
        // xwfc.CollapseAutomatic();
        // watch.Stop();
        // Debug.Log($"Time taken {watch.ElapsedMilliseconds * 0.001} seconds");
        
        _unitSize = unitTilePrefab.GetComponent<Renderer>().bounds.size;
        
        // Debug.Log(xwfc.GridManager.Grid.GridToString());
        
        
        
        // ToggleState(StateFlag.Collapsing);

        Debug.Log($"Size of object: {_unitSize}");
    }

    private Grid<Drawing> InitDrawGrid()
    {
        return new Grid<Drawing>(
            _xwfc.GridExtent, new Drawing(_xwfc.GridManager.Grid.DefaultFillValue, null));
    }

    public Vector3 GetUnitSize()
    {
        return _unitSize;
    }

    public void ToggleCollapseMode()
    {
        ToggleState(StateFlag.Collapsing);
    }

    private void ToggleState(StateFlag flag)
    {
        // If the flag is not raised, raise it. Otherwise, lower is.
        if ((_activeStateFlag & flag) == 0)
            _activeStateFlag |= flag;
        else
            _activeStateFlag &= ~flag;
        string s = $"Toggled active states of {flag.ToString()} to {_activeStateFlag.HasFlag(flag)}";
        Debug.Log(s);
    }

    public void UpdateIterationsPerSecond(float value)
    {
        stepSize = value;
        _iterationsPerSecondInv = 1 / value;
    }

    private int CalcIterations()
    {
        if (_iterationsPerSecondInv < 0) return -1;
        if (_iterationDeltaTime < _iterationsPerSecondInv) return 0;
            
        int nIterations = (int)Math.Floor(_iterationDeltaTime / _iterationsPerSecondInv);
        return nIterations;
    }

    // private IEnumerator Iterate()
    // {
    //     if (!MayCollapse()) yield return null;
    //     float interval = 0.0f;
    //
    //     if ((_activeStateFlag & StateFlag.Collapsing) != 0)
    //     {
    //         _iterationDeltaTime += Time.deltaTime;
    //         int nIterations = CalcIterations();
    //         // if (_iterationDeltaTime < interval) return;
    //         if (nIterations == 0) yield return null;
    //         _iterationDeltaTime -= nIterations * _iterationsPerSecondInv;
    //         _iterationsDone = 0;
    //
    //         var affectedCells = new HashSet<Occupation>();
    //         // while (MayIterate() && MayCollapse())
    //         while ((_iterationsDone < stepSize || nIterations < 0) && MayCollapse())
    //         {
    //             // var cells = CollapseAndDrawOnce();
    //             var cells = CollapseOnce();
    //             affectedCells.UnionWith(cells);
    //             _iterationsDone++;
    //         }
    //         Draw(affectedCells);
    //     }
    // }
    private void Iterate()
    {
        if (!MayCollapse()) return;
        float interval = 0.0f;
        
        if ((_activeStateFlag & StateFlag.Collapsing) != 0)
        {
            // _iterationDeltaTime += Time.deltaTime;
            // int nIterations = CalcIterations();
            // // if (_iterationDeltaTime < interval) return;
            // if (nIterations == 0) return;
            // _iterationDeltaTime -= nIterations * _iterationsPerSecondInv;
            _iterationsDone = 0;
        
            var affectedCells = new HashSet<Occupation>();
            // while (MayIterate() && MayCollapse())
            while ((_iterationsDone < stepSize || stepSize < 0) && MayCollapse())
            {
                // var cells = CollapseAndDrawOnce();
                var cells = CollapseOnce();
                affectedCells.UnionWith(cells);
                _iterationsDone++;
            }
            Draw(affectedCells);
        }
    }

    private void Update()
    {
        if (updateDeltaTime >= delay)
        {
            Iterate();
            updateDeltaTime = 0;
        }
        else
        {
            updateDeltaTime += Time.deltaTime;
        }
    }

    // Update is called once per frame
    // private void Update()
    // {
    //     
    //         
    //         /*
    //          * Either
    //          * (a): iterations per second <= max framerate
    //          * Need to limit drawing and calling.
    //          * Need no more than one call to collapse per frame.
    //          * Calc delta time to achieve the speed. That is, floor(dt / iterPerFrame).
    //          * (b): iterations per second > max framerate
    //          * Need to compensate.
    //          * Need more than one call to collapse per frame.
    //          * Delta time is always larger than time window for one collapse. floor(dt/iterPerFrame)?
    //          */
    //         
    //         // _timer.Stop();
    //         
    //         // var s = affectedCells.Aggregate("", (current, occupation) => current + $"{occupation.Coord}, ");
    //
    //         // s = $"{affectedCells.Count} cell(s) affected: " + s;
    //         // Debug.Log(s);
    //     }
    //
    //     if (!MayCollapse())
    //     {
    //         var grid = _drawnGrid.GetGrid();
    //         string s = "";
    //         for (int y = 0; y < grid.GetLength(0); y++)
    //         {
    //             s += "[\n";
    //             for (int x = 0; x < grid.GetLength(1); x++)
    //             {
    //                 s += "[";
    //                 for (int z = 0; z < grid.GetLength(2); z++)
    //                 {
    //                     s += grid[y, x, z].Id + ", ";
    //                 }
    //                 s += "]\n";
    //             }
    //             s += "\n]\n";
    //         }
    //         Debug.Log("DRAWN GRID:\n" + s);
    //         Debug.Log("XWFC GRID:\n" + _xwfc.GridManager.Grid.GridToString());
    //     }
    // }

    public HashSet<Occupation> CollapseOnce()
    {
        return !MayCollapse() ? new HashSet<Occupation>() : _xwfc.CollapseOnce();
    }

    public HashSet<Occupation> CollapseAndDrawOnce()
    {
        /*
         * Performs a single collapse and draws the resulting atoms.
         * Returns the set of affected cells.
         */
        var affectedCells = CollapseOnce();
        Draw(affectedCells);
        return affectedCells;
    }

    private bool MayCollapse()
    {
        /*
         * Determines whether a single collapse may be done.
         */
        return !_xwfc.IsDone();
    }

    private bool MayIterate()
    {
        /*
         * Determines whether a single collapse may still be done per cycle.
         */
        // If the number of iteration is negative, assume to go keep going until the xwfc is done.
        // Otherwise, limit the number of iterations per frame.
        
        return _iterationsDone < stepSize || stepSize < 0;
    }

    private void Draw(HashSet<Occupation> affectedCells)
    {
        foreach (var occupation in affectedCells)
        {
            var coord = occupation.Coord;
            var (x,y,z) = Vector3Util.CastInt(coord);
            if (!_drawnGrid.WithinBounds(x,y,z)) continue;
            var grid = _xwfc.GridManager.Grid;
            var status = grid.Get(coord);
            var drawnStatus = _drawnGrid.Get(coord);
            
            // Only update if there is a change.
            if (status == drawnStatus.Id) continue;
            
            // Clear the cells content to be replaced with a new drawing.
            if (drawnStatus.Id != grid.DefaultFillValue) Destroy(drawnStatus.Atom);
            
            // Empty cell should not show an object. Drawn grid restored to default.
            if (status == grid.DefaultFillValue)
            {
                _drawnGrid.Set(coord, new Drawing(grid.DefaultFillValue, null));
            }
            else
            {
                // Drawn atom should be updated to match the new atom's specification.
                // TODO: reference the prefab corresponding to the atom, instead of assuming the same is used for all.
                var atom = Instantiate(unitTilePrefab);
                var drawing = new Drawing(status, atom);
                atom.transform.position = CalcAtomPosition(coord);
                atom.GetComponent<Renderer>().material.color = GetTerminalColor(status);
                _drawnGrid.Set(coord, drawing);
            }
        }
    }

    private Color GetTerminalColor(int atomId)
    {
        return _xwfc.AdjMatrix.GetTerminalFromAtomId(atomId).Color;
    }

    private Vector3 CalcAtomPosition(Vector3 coord)
    {
        return Vector3Util.Mult(coord, _unitSize);
    }

    private record Drawing
    {
        public int Id;
        public GameObject Atom;

        public Drawing(int id, GameObject atom)
        {
            Id = id;
            Atom = atom;
        }

        public static void DestroyAtom(Drawing drawing)
        {
            if (drawing != null && drawing.Atom != null) Destroy(drawing.Atom);
        }
    }

    public void UpdateExtent(Vector3 newExtent)
    {
        if (extent.Equals(newExtent)) return;
        extent = newExtent;
        _xwfc.UpdateExtent(newExtent);
        _drawnGrid.Map(Drawing.DestroyAtom);
        _drawnGrid = InitDrawGrid();
        InitDrawGrid();
        _activeStateFlag = 0;
        Debug.Log("UPDATED!!");
    }
}
