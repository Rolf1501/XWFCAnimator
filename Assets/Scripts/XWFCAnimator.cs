using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using XWFC;

public class XWFCAnimator : MonoBehaviour
{
    [SerializeField] private GameObject unitTilePrefab;
    [SerializeField] private Vector3 extent;
    [SerializeField] private int iterationsPerCycle = 1;

    private Vector3 _unitSize;
    private static XWFCAnimator _instance;

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
        if (_instance != null && _instance != this) Destroy(this); 
        else _instance = this;
    }

    public XWFCAnimator GetInstance()
    {
        return _instance;
    }
    void Start()
    {
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
        _drawnGrid = new Grid<Drawing>(
            _xwfc.GridExtent, new Drawing(_xwfc.GridManager.Grid.DefaultFillValue, null));
        
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
        
        
        
        ToggleState(StateFlag.Collapsing);

        Debug.Log($"Size of object: {_unitSize}");
    }

    public Vector3 GetUnitSize()
    {
        return _unitSize;
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

    // Update is called once per frame
    void Update()
    {
        if ((_activeStateFlag & StateFlag.Collapsing) != 0 )
        {
            _iterationsDone = 0;
            var affectedCells = new HashSet<Occupation>();
            // If the number of iteration is negative, assume to go keep going until the xwfc is done.
            // Otherwise, limit the number of iterations per frame.
            
            _timer.Start();
            while (!_xwfc.IsDone() && (_iterationsDone < iterationsPerCycle || iterationsPerCycle < 0))
            {
                affectedCells.UnionWith(_xwfc.CollapseOnce());
                _iterationsDone++;
            }
            _timer.Stop();
            
            var s = affectedCells.Aggregate("", (current, occupation) => current + $"{occupation.Coord}, ");

            s = $"{affectedCells.Count} cell(s) affected: " + s;
            Debug.Log(s);

            Draw(affectedCells);
            
            ToggleState(StateFlag.Collapsing);
        }
        
    }

    private void Draw(HashSet<Occupation> affectedCells)
    {
        foreach (var occupation in affectedCells)
        {
            var coord = occupation.Coord;
            var grid = _xwfc.GridManager.Grid;
            var status = grid.Get(coord);
            var drawnStatus = _drawnGrid.Get(coord);
            
            // Only update if there is a change.
            if (status == drawnStatus.Id) continue;
            
            // Only destroy if the something was drawn.
            if (drawnStatus.Id != grid.DefaultFillValue) Destroy(drawnStatus.Atom);
            
            if (status == grid.DefaultFillValue)
            {
                // Destroy loaded asset.
                _drawnGrid.Set(coord, new Drawing(grid.DefaultFillValue, null));
            }
            else
            {
                var atom = Instantiate(unitTilePrefab);
                atom.transform.position = CalcAtomPosition(coord);
                atom.GetComponent<Renderer>().material.color = GetTerminalColor(status);
                _drawnGrid.Set(coord, new Drawing(status, atom));
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
    }
}
