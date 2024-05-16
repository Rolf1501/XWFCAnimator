using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using XWFC;
using Canvas = UnityEngine.Canvas;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class XWFCAnimator : MonoBehaviour
{
    [SerializeField] private GameObject unitTilePrefab;
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private Canvas tileLabelPrefab;
    public Vector3 extent;
    public float stepSize;
    public TileSet TileSet;
    public TileSet CompleteTerminalSet = new();
    public Dictionary<int, Vector3> drawnTilePositions = new();
    public float delay;

    private float _updateDeltaTime;
    private float _iterationDeltaTime;

    private Vector3 _unitSize;
    public static XWFCAnimator Instance { get; private set; }
    
    private int _iterationsDone;

    private StateFlag _activeStateFlag = 0;
    
    private ExpressiveWFC _xwfc;

    private Grid<Drawing> _drawnGrid;
    private HashSetAdjacency _adjacency;

    private HashSet<GameObject> _drawnTiles;
    
    [Flags]
    private enum StateFlag
    {
        Collapsing = 1 << 0
    }
    
    // Singleton assurance.
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        TileSet = new TileSet();

        var defaultWeights = new Dictionary<int, float>();

        var borderOutline = new BorderOutline();
        
        // var t0 = new Terminal(
        //     new Vector3(2,1, 2), 
        //     new Color(.8f, 0, .2f) ,
        //     null,
        //     null
        //     );
        // var t1 = new Terminal(new Vector3(2, 1, 2), new Color(.2f, 0, .8f), new bool[,,]{ { {true, true}, {true, false} } }, null);
        // var t2 = new Terminal(new Vector3(2,1,1), new Color(.2f, .4f, .3f), new bool[,,] { { {true}, {true} } }, null);

        // var t2 = new Terminal(
        var tileL = new Terminal(
            new Vector3(2, 1, 3),
            new Color(240 / 255.0f, 160 / 255.0f, 0),
            new bool[,,] { { { true, true, true }, { true, false, false } } },
            null,
            computeAtomEdges:true
        );
        var tileT = new Terminal(
            new Vector3(2, 1, 3),
            new Color(160 / 255.0f, 0, 240/255.0f),
            new bool[,,] { { { false, true, false }, { true, true, true }} },
            null,
            computeAtomEdges:true
        );
        
        var tileJ = new Terminal(
            new Vector3(3, 1, 2),
            new Color(0, 0, 240 / 255.0f),
            new bool[,,] { { { true, true }, { false, true }, { false, true } } },
            null,
            computeAtomEdges:true
        );
        
        var tileI = new Terminal(
            new Vector3(4, 1, 1),
            new Color(0, 240 / 255.0f, 240 / 255.0f),
            new bool[,,] { { { true }, { true }, { true }, { true } } },
            null,
            computeAtomEdges:true
        );
        
        var tileS = new Terminal(
            new Vector3(2, 1, 3),
            new Color(0, 240 / 255.0f, 0),
            new bool[,,] { { { true, true, false }, { false, true, true }} },
            null,
            computeAtomEdges:true
        );
        var tileZ = new Terminal(
            new Vector3(2, 1, 3),
            new Color(240 / 255.0f, 0, 0),
            new bool[,,] { { { false, true, true }, { true, true, false }} },
            null,
            computeAtomEdges:true
        );
        var tileO = new Terminal(
            new Vector3(2, 1, 2),
            new Color(0, 240 / 255.0f, 240 / 255.0f),

            // new Color(240 / 255.0f, 240 / 255.0f, 0),
            new bool[,,] { { { true, true }, { true, true }} },
            null,
            computeAtomEdges:true
        );

        // var atomEdges = borderOutline.GetEdgesPerAtom(tileJ.Mask);
        // Debug.Log("TileJ");
        // foreach (var (k,v) in atomEdges)
        // {
        //     Debug.Log($"Atom:{k}");
        //     foreach (var val in v) Debug.Log(val + ",");
        // }

        var tetrisTiles = new Terminal[] { tileO, tileS, tileZ, tileL, tileI, tileJ, tileO };
        
        // var t2 = new Terminal(new Vector3(2,1,1), new Color(.2f, 0, .8f), null, null);
        
        for(int i = 0; i < tetrisTiles.Length; i++)
        {
            CompleteTerminalSet.Add(i, tetrisTiles[i]);
        }
        
        TileSet.Add(0, tetrisTiles[0]);
        TileSet.Add(1, tetrisTiles[1]);
        TileSet.Add(2, tetrisTiles[2]);
        
        // TileSet.Add(0, t0);
        // TileSet.Add(1, t1);
        // TileSet.Add(2, t2);
        //
        // CompleteTerminalSet.Add(0, t0);
        // CompleteTerminalSet.Add(1, t1);
        // CompleteTerminalSet.Add(2, t2);
        
        var NORTH = new Vector3(0, 0, 1);
        var SOUTH = new Vector3(0, 0, -1);
        var EAST = new Vector3(1, 0, 0);
        var WEST = new Vector3(-1, 0, 0);
        var TOP = new Vector3(0, 1, 0);
        var BOTTOM = new Vector3(0, -1, 0);
        
        _adjacency = new HashSetAdjacency(){
            // 0-0
            // new(0, new List<Relation>() { new(0, null) }, NORTH),
            new(0, new List<Relation>() { new(0, null) }, EAST),
            // new(0, new List<Relation>() { new(0, null) }, SOUTH),
            new(0, new List<Relation>() { new(0, null) }, WEST),
            new(0, new List<Relation>() { new(0, null) }, TOP),
            new(0, new List<Relation>() { new(0, null) }, BOTTOM),
            // 1-0
            new(1, new List<Relation>() { new(0, null) }, NORTH),
            // new(1, new List<Relation>() { new(0, null) }, EAST),
            new(1, new List<Relation>() { new(0, null) }, SOUTH),
            // new(1, new List<Relation>() { new(0, null) }, WEST),
            new(1, new List<Relation>() { new(0, null) }, TOP),
            new(1, new List<Relation>() { new(0, null) }, BOTTOM),
            // 1-1
            // new(1, new List<Relation>() { new(0, null) }, NORTH),
            new(1, new List<Relation>() { new(0, null) }, EAST),
            // new(1, new List<Relation>() { new(0, null) }, SOUTH),
            new(1, new List<Relation>() { new(0, null) }, WEST),
            new(1, new List<Relation>() { new(0, null) }, TOP),
            new(1, new List<Relation>() { new(0, null) }, BOTTOM),
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
        
        InitXWFC();
        
        // Grid for keeping track of drawn atoms.
        _drawnGrid = InitDrawGrid();
        
        foreach (var o in _xwfc.Offsets)
        {
            var x = _xwfc.AdjMatrix.AtomAdjacencyMatrix[o];
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
        
        _unitSize = unitTilePrefab.GetComponent<Renderer>().bounds.size;
        
        // Set for keeping track of drawn terminals.
        _drawnTiles = new HashSet<GameObject>();
        
        DrawTiles();
        
        // SaveConfig();
        var zzz = new AdjacencyMatrixJsonFormatter(_xwfc.AdjMatrix.TileAdjacencyConstraints, TileSet);
        var json = zzz.ToJson();
        zzz.FromJson(json);
        
    }

    private void InitXWFC()
    {
        try
        {
            _xwfc = new ExpressiveWFC(TileSet, _adjacency, extent);

        }
        catch (Exception exception)
        {
            var s = exception.ToString();
            Debug.Log(s);
            Debug.Log("Ran into an error...");
        }
        Debug.Log("Initialized XWFC");
    }

    public void UpdateAdjacencyConstraints(HashSetAdjacency adjacency)
    {
        var tempXWFC = _xwfc;
        try
        {
            _adjacency = adjacency;
            Reset();
            InitXWFC();
        }
        catch
        {
            Debug.Log("Failed to update adjacency constraints.");
            _xwfc = tempXWFC;
        }
    }

    public void UpdateTerminals(TileSet newTileSet)
    {
        var tempXWFC = _xwfc;
        try
        {
            TileSet = newTileSet;
            _adjacency = new HashSetAdjacency();
            Reset();
            InitXWFC();
        }
        catch
        {
            Debug.Log("Failed to update terminals.");
            _xwfc = tempXWFC;
        }
    }

    public void DrawTiles()
    {
        // Draw in z-axis.
        var start = new Vector3(-100,-100,-5);
        var gap = new Vector3(5, 0, 0);
        foreach (var (key, value) in CompleteTerminalSet)
        {
            var maxIndex = new Vector3();
            bool labeled = false;
            foreach (var (index, _) in value.AtomIndexToIdMapping)
            {
                if (maxIndex.x < index.x) maxIndex.x = index.x;
                var drawnAtom = Instantiate(unitTilePrefab);
                drawnAtom.transform.position = CalcAtomPosition(start + index);
                
                UpdateColorFromTerminal(drawnAtom, key);
                _drawnTiles.Add(drawnAtom);

                if (labeled) continue;
                LabelTiles(drawnAtom, key.ToString());
                labeled = true;
                drawnTilePositions[key] = drawnAtom.transform.position;
            }

            start += maxIndex;
            start += gap;
        }
        
    }

    private void LabelTiles(GameObject parent, string terminalName)
    {
        var label = Instantiate(tileLabelPrefab, parent.transform, false);
        label.GetComponent<RectTransform>().localPosition = new Vector3(0, -2, 2);
        // offset the position by half the parent size.
        
        // TODO: find out how to position dynamically.
        var textComp = label.GetComponentInChildren<TMP_Text>();
        textComp.text = "Tile: " + terminalName;
        var rect = textComp.GetComponent<RectTransform>();
        rect.localPosition = new Vector3();
        rect.eulerAngles = new Vector3(0, 0, 0); 
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

    public Vector3[] GetOffsets()
    {
        return _xwfc.Offsets;
    }

    public Dictionary<int, Terminal> GetTiles()
    {
        return TileSet;
    }

    public HashSetAdjacency GetTileAdjacencyConstraints()
    {
        return _xwfc.AdjMatrix.TileAdjacencyConstraints;
    }

    public bool IsDone()
    {
        return _xwfc.IsDone();
    }

    public bool ToggleCollapseMode()
    {
        return ToggleState(StateFlag.Collapsing);
    }

    private bool ToggleState(StateFlag flag)
    {
        // If the flag is not raised, raise it. Otherwise, lower is.
        if ((_activeStateFlag & flag) == 0)
            _activeStateFlag |= flag;
        else
            _activeStateFlag &= ~flag;
        string s = $"Toggled active states of {flag.ToString()} to {_activeStateFlag.HasFlag(flag)}";
        return _activeStateFlag.HasFlag(flag);
    }

    public void UpdateStepSize(float value)
    {
        stepSize = value;
    }
    
    private void Iterate()
    {
        if (!MayCollapse()) return;
        
        if ((_activeStateFlag & StateFlag.Collapsing) != 0)
        {
            _iterationsDone = 0;
        
            var affectedCells = new HashSet<Occupation>();
            // while (MayIterate() && MayCollapse())
            while (MayIterate() && MayCollapse())
            {
                // var cells = CollapseAndDrawOnce();
                var cells = CollapseOnce();
                affectedCells.UnionWith(cells);
                _iterationsDone++;
            }
            Draw(affectedCells);
        }
        
        if (!MayCollapse())
        {
            string s = DrawnGridIdsToString();
            Debug.Log("DRAWN GRID:\n" + s);
            Debug.Log("XWFC GRID:\n" + _xwfc.GridManager.Grid.GridToString());
        }
    }

    private string DrawnGridIdsToString()
    {
        var grid = _drawnGrid.GetGrid();
        string s = "";
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            s += "[\n";
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                s += "[";
                for (int z = 0; z < grid.GetLength(2); z++)
                {
                    s += grid[y, x, z].Id + ", ";
                }
                s += "]\n";
            }
            s += "\n]\n";
        }

        return s;
    }

    private void Update()
    {
        if (_updateDeltaTime >= delay)
        {
            Iterate();
            _updateDeltaTime = 0;
        }
        else
        {
            _updateDeltaTime += Time.deltaTime;
        }
    }

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
            if (drawnStatus.Id != grid.DefaultFillValue)
            {
                Drawing.DestroyAtom(drawnStatus);
            }
            
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
                
                atom.transform.position = CalcAtomPosition(coord);
                var edges = DrawEdges(status, atom);
                var drawing = new Drawing(status, atom, edges);
                UpdateColorFromAtom(atom, status);
                // atom.GetComponent<Renderer>().material.color = GetTerminalColor(status);
                _drawnGrid.Set(coord, drawing);
            }
        }
    }

    private HashSet<GameObject> DrawEdges(int atomId, GameObject atom)
    {
        var bounds = atom.GetComponent<Renderer>().bounds.size;
        var edgeExtent = new Vector3(0.05f,0.05f,0.05f);
        var origin = atom.transform.position;
        var halfBounds = Vector3Util.Scale(bounds, 0.5f);
        var zeroOrigin = origin - halfBounds;
        
        var (terminalId, atomIndex, _) = _xwfc.AdjMatrix.AtomMapping.Get(atomId);
        
        var edges = TileSet[terminalId].AtomEdges;
        var edgeObjects = new HashSet<GameObject>();
        if (edges == null) return edgeObjects;

        var atomEdges = edges[new Vector3Int((int)atomIndex.x, (int)atomIndex.y, (int)atomIndex.z)];
        foreach (var atomEdge in atomEdges)
        {
            var edge = Instantiate(edgePrefab);
            var edgeCenter = Vector3Util.Scale(atomEdge.GetDistance(), 0.5f);
            
            // Edge pos, account for origin being center of object. 
            var edgePos = zeroOrigin + Vector3Util.Mult(atomEdge.From + edgeCenter, bounds);
            
            edge.transform.position = edgePos;
            var atomDist = atomEdge.GetDistance();
            var newEdgeBounds = atomDist;//Vector3Util.Mult(atomDist, bounds);
            var normScale = Vector3Util.Div(bounds, edge.GetComponent<Renderer>().bounds.size);
            newEdgeBounds.x = Math.Max(edgeExtent.x, newEdgeBounds.x);
            newEdgeBounds.y = Math.Max(edgeExtent.y, newEdgeBounds.y);
            newEdgeBounds.z = Math.Max(edgeExtent.z, newEdgeBounds.z);
            edge.transform.localScale = Vector3Util.Mult(normScale, newEdgeBounds);
            
            // Prefab from blender needs to be rotated first...
            edge.transform.Rotate(new Vector3(90,0,0));
            edgeObjects.Add(edge);
        }

        return edgeObjects;
    }

    private static Color ApplyVariation(Color c, float fluctuation)
    {
        var rand = new Random();
        var fluct = (float)rand.NextDouble() * fluctuation;
        Color color = c;
        color.r += fluct;
        color.g += fluct;
        color.b += fluct;
        return color;

    }

    private void UpdateColorFromAtom(GameObject obj, int atomId, bool applyFluctuation=true)
    {
        var color = GetTerminalColorFromAtom(atomId);
        if (applyFluctuation) color = ApplyVariation(color, 0.2f);
        obj.GetComponent<Renderer>().material.color = color;
    }

    private void UpdateColorFromTerminal(GameObject obj, int tileId)
    {
        obj.GetComponent<Renderer>().material.color = CompleteTerminalSet[tileId].Color;
    }

    private Color GetTerminalColorFromAtom(int atomId)
    {
        return _xwfc.AdjMatrix.GetTerminalFromAtomId(atomId).Color;
    }

    private Vector3 CalcAtomPosition(Vector3 coord)
    {
        return Vector3Util.Mult(coord, _unitSize);
    }

    public void Reset()
    {
        _xwfc.UpdateExtent(extent);
        _drawnGrid.Map(Drawing.DestroyAtom);
        _drawnGrid = InitDrawGrid();
        _activeStateFlag = 0;
    }
    
    public void UpdateExtent(Vector3 newExtent)
    {
        if (extent.Equals(newExtent)) return;
        extent = newExtent;
        Reset();
    }

    public Vector3 GetGridCenter()
    {
        return 0.5f * Vector3Util.Mult(_unitSize, extent);
    }

    public void UpdateDelay(float value)
    {
        delay = value;
    }

    public void SaveConfig()
    {
        /*
         * To replicate a config need:
         * tileset and adjacency constraints.
         */
        var config = new Dictionary<string, object>();
        foreach (var (k,v) in TileSet)
        {
            var json = v.ToJson();
        }
        /*
         * To replicate tileset need:
         * Tile id and its extent, color, mask, orientation and description.
         */

        /*
         * To replicate adjacency constraints need:
         * Tile ids.
         * Tile adjacency constraints per offset.
         */

    }
    
    private record Drawing
    {
        public int Id;
        public GameObject Atom;
        public HashSet<GameObject> Edges;

        public Drawing(int id, GameObject atom, [CanBeNull] HashSet<GameObject> edges=null)
        {
            Id = id;
            Atom = atom;
            Edges = edges;
        }

        public static void DestroyAtom(Drawing drawing)
        {
            if (drawing == null || drawing.Atom == null) return; 
            
            Destroy(drawing.Atom);
            
            if (drawing.Edges == null) return;
            
            foreach (var edge in drawing.Edges)
            {
                Destroy(edge);
            }
        }
    }
}
