using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using XWFC;
using Canvas = UnityEngine.Canvas;
using Component = XWFC.Component;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class XWFCAnimator : MonoBehaviour
{
    [SerializeField] private GameObject unitTilePrefab;
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private Canvas tileLabelPrefab;
    public Vector3Int extent;
    public float stepSize;
    public TileSet TileSet;
    public TileSet CompleteTileSet = new();
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

    private ComponentManager _componentManager;
    private Component _currentComponent;
    
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

        _adjacency = new HashSetAdjacency();
        
        // InitXWFC();

        var houseComponents = GetHouseComponents();

        _componentManager = new ComponentManager(houseComponents);
        
        LoadNextComponent();
        
        TileSet = _xwfc.AdjMatrix.TileSet;
        CompleteTileSet = _xwfc.AdjMatrix.TileSet;
        
        // Grid for keeping track of drawn atoms.
        _drawnGrid = InitDrawGrid();
        
        PrintAdjacencyData();

        _unitSize = unitTilePrefab.GetComponent<Renderer>().bounds.size;
        
        // Set for keeping track of drawn terminals.
        _drawnTiles = new HashSet<GameObject>();
        
        DrawTiles();
        // _xwfc.CollapseAutomatic();
        
        // if (!FindConfigFileNames().Any()) SaveConfig();
        // LoadConfig();
    }

    public void Assemble()
    {
        if (_componentManager.HasNext()) return;
        SaveComponent();
        Reset();
        /*
         * Assemble components in large grid?
         */
        /*
         * Assemble components in large grid?
         */
        /*
         * Find smallest and largest coord.
         */
        _activeStateFlag = 0;
        
        var (min, max) = _componentManager.BoundingBox();
        
        // foreach (var drawing in _drawnGrid.GetGrid())
        // {
        //     Drawing.DestroyAtom(drawing);
        // }
        
        var defaultValue = _xwfc.GridManager.Grid.DefaultFillValue;
        _drawnGrid = new Grid<Drawing>(max, new Drawing(defaultValue, null));
            
        for (var id = 0; id< _componentManager.Components.Length; id++)
        {
            var component = _componentManager.Components[id];
            var origin = component.Origin;
            var grid = component.Grid;
            var e = grid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        DrawAtom(new Vector3(x,y,z), grid.Get(x,y,z), origin);
                    }
                }
            }
        }
    }

    public bool HasNextComponent()
    {
        return _componentManager.HasNext();
    }

    public void LoadNextComponent()
    {
        if (!HasNextComponent()) return;
        
        SaveComponent();
        
        /*
         * Before selecting the next component, translate all unsolved components depending on the results of the current component.
         */
        _componentManager.TranslateUnsolved();
        
        var id = _componentManager.Next();
        _componentManager.SeedComponent(id);
        
        _currentComponent = _componentManager.Components[id];
        
        // Intersection with next component.
        
        InitXWFComponent(ref _currentComponent);
        TileSet = _currentComponent.Tiles;
        CompleteTileSet = TileSet;
        _activeStateFlag = 0;
        Reset();
    }

    private TileSet ToTileSet(Tile[] tiles)
    {
        var tileSet = new TileSet();
        var key = 0;
        foreach (var tile in tiles)
        {
            tileSet[key] = tile;
            key++;
        }
        return tileSet;
    }

    private Component[] GetHouseComponents()
    {
        var (houseTiles, weights) = GetHouseTiles();
        var tileSet = ToTileSet(houseTiles);

        // var activeTileSet = tetrisTiles;
        var patterns = GetHousePatterns();
        
        var (grids, tileIds) = InputHandler.PatternsToGrids(patterns, tileSet, "");
        // Three components stacked in the y-direction.
        var baseExtent = new Vector3Int(30, 1, 20);
        var floorExtent = new Vector3Int(20, 1, 20);
        var roofExtent = new Vector3Int(20, 1, 20);

        var baseOrigin = new Vector3Int(0,0,0);
        var floorOrigin = baseOrigin;
        floorOrigin.z += baseExtent.z;
        var roofOrigin = floorOrigin;
        roofOrigin.z += floorExtent.z;

        var baseTileSet = ToTileSet(BaseTiles());
        var (baseGrid, ids) = InputHandler.PatternsToGrids(BasePatterns(), baseTileSet, "");
        
        
        var baseComponent = new Component(baseOrigin, baseExtent, baseTileSet, baseGrid.ToArray(), weights);
        // var floor = new Component(floorOrigin, floorExtent, tileSet, grids.ToArray(), weights);
        // var roof = new Component(roofOrigin, roofExtent, tileSet, grids.ToArray(), weights);
        var components = new Component[] { baseComponent };//, floor, roof };

        return components;
    }

    private Tile[] BaseTiles()
    {
        var grassTile = new Tile(
            "g",
            new Vector3(1,1,1), 
            color: new Color(0,0.8f,0,1) 
        );
        
        var soilTile = new Tile(
            "s",
            new Vector3(1,1,1), 
            color: new Color(0.1f,0.1f,0.1f,1),
            computeAtomEdges: false
        );
        
        var brickTile = new Tile(
            "b0",
            new Vector3(2,1,1), 
            color: new Color(0.8f,0,0,1)
        );
        
        var emptyTile = new Tile(
            ".",
            new Vector3(1, 1, 1),
            new Color(0, 0.2f, 0.5f, 0.2f),
            description: "empty",
            computeAtomEdges:false,
            isEmptyTile:true
        );
        
        var halfBrickTile = new Tile(
            "b1",
            new Vector3(1,1,1), 
            color: new Color(0.4f,0,0.4f,1)
        );

        return new Tile[] { brickTile, grassTile, soilTile, emptyTile, halfBrickTile };
    }

    private List<List<(int, Vector3)>> BasePatterns()
    {
        var grassBrickPattern = new List<(int, Vector3)>()
        {
            // b,b
            // g,g
            (0, new Vector3(0, 0, 1)),
            (1, new Vector3(0, 0, 0)),
            (1, new Vector3(1, 0, 0)),
        };

        var brickPattern = new List<(int, Vector3)>()
        {
            // b1,b,b,b1
            //  b,b,b,b
            (0, new Vector3(2,0,0)),
            (0, new Vector3(0,0,0)),
            (0, new Vector3(1,0,1)),
            (4, new Vector3(0,0,1)),
            (4, new Vector3(3,0,1)),
        };

        var brickPattern1 = new List<(int, Vector3)>()
        {
            // b1      ,b1
            // b0,b0,b0,b0
            // b1      ,b1
            (4, new Vector3(0, 0, 0)),
            (4, new Vector3(0, 0, 2)),
            (0, new Vector3(0, 0, 1)),
            (0, new Vector3(2, 0, 1)),
            (4, new Vector3(3, 0, 0)),
            (4, new Vector3(3, 0, 2)),
        };

        var grassSoilPattern = new List<(int, Vector3)>()
        {
            // g,g,s
            // s,s,s
            (1, new Vector3(0,0,1)),
            (1, new Vector3(1,0,1)),
            (2, new Vector3(0,0,0)),
            (2, new Vector3(1,0,0)),
            (2, new Vector3(2,0,1)),
            (2, new Vector3(2,0,0)),
        };

        var emptyGrassPattern = new List<(int, Vector3)>()
        {
            // e
            // g
            (3, new Vector3(0, 0, 1)),
            (1, new Vector3(0, 0, 0))
        };

        var emptyBrickPattern = new List<(int, Vector3)>()
        {
            //   e,e
            // e,b,b,e
            (0, new Vector3(1, 0, 0)),
            (3, new Vector3(0, 0, 0)),
            (3, new Vector3(3, 0, 0)),
            (3, new Vector3(1, 0, 1)),
            (3, new Vector3(1, 0, 2)),
        };

        var emptyEmptyPattern = new List<(int, Vector3)>()
        {
            // e
            // e,e
            (3, new Vector3(0, 0, 0)),
            (3, new Vector3(0, 0, 1)),
            (3, new Vector3(1, 0, 0)),
        };

        return new List<List<(int, Vector3)>>()
            { brickPattern, emptyBrickPattern, emptyEmptyPattern, emptyGrassPattern, grassBrickPattern, grassSoilPattern, brickPattern1 };
    }
    
    private void PrintAdjacencyData()
    {
        foreach (var o in _xwfc.Offsets)
        {
            var x = _xwfc.AdjMatrix.AtomAdjacencyMatrix[o];
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
        
        foreach (var (k,v) in _xwfc.AdjMatrix.AtomMapping.Dict)
        {
            Debug.Log($"{k}: {v}");
        }
    }

    private (Tile[] houseTiles, float[] weights) GetHouseTiles()
    {
        var doorTile = new Tile(
            "d", 
            new Vector3(3, 1, 5), 
            new Color(0, 0, 0.8f)
        );
        
        var brickTile = new Tile(
            "b0",
            new Vector3(2,1,1), 
            color: new Color(0.8f,0,0,1)
        );
        
        var halfBrickTile = new Tile(
            "b1",
            new Vector3(1,1,1), 
            color: new Color(0.4f,0,0.4f,1)
        );
        
        var grassTile = new Tile(
            "g",
            new Vector3(1,1,1), 
            color: new Color(0,0.8f,0,1) 
        );
        
        var soilTile = new Tile(
            "s",
            new Vector3(1,1,1), 
            color: new Color(0.1f,0.1f,0.1f,1),
            computeAtomEdges: false
        );

        var windowTile = new Tile(
            "w",
            new Vector3(3, 1, 3),
            color: new Color(0, 0, 0.8f)
        );

        var emptyTile = new Tile(
            ".",
            new Vector3(1, 1, 1),
            new Color(0, 0.2f, 0.5f, 0.2f),
            description: "empty",
            computeAtomEdges:false,
            isEmptyTile:true
        );
        
        var houseTiles = new Tile[] { doorTile, brickTile, halfBrickTile, grassTile, soilTile, windowTile, emptyTile };
        var weights = new float[] { 1, 1, 1, 1, 1, 1, 1};
        return (houseTiles, weights);
    }
    private List<List<(int, Vector3)>> GetHousePatterns()
    {
        var doorBrickPattern = new List<(int, Vector3)>()
        {
            (0, new Vector3(2,0,0)),
            
            (1, new Vector3(0,0,1)),
            (1, new Vector3(0,0,3)),
            (1, new Vector3(0,0,5)),
            
            (1, new Vector3(2,0,5)),
            (1, new Vector3(4,0,5)),
            
            (1, new Vector3(5,0,4)),
            (1, new Vector3(5,0,2)),
            (1, new Vector3(5,0,0)),
            
            (2, new Vector3(1,0,0)),
            (2, new Vector3(1,0,2)),
            (2, new Vector3(1,0,4)),
            
            (2, new Vector3(5,0,1)),
            (2, new Vector3(5,0,3)),
        };

        var windowBrickPattern = new List<(int, Vector3)>()
        {
            (5, new Vector3(2, 0, 1)),

            (2, new Vector3(1, 0, 1)),
            (1, new Vector3(0, 0, 2)),
            (2, new Vector3(1, 0, 3)),
            
            (2, new Vector3(5, 0, 1)),
            (1, new Vector3(5, 0, 2)),
            (2, new Vector3(5, 0, 3)),

            (1, new Vector3(2, 0, 0)),
            (2, new Vector3(4, 0, 0)),
            
            (2, new Vector3(2, 0, 4)),
            (1, new Vector3(3, 0, 4)),
        };

        var doorGrassPattern = new List<(int, Vector3)>()
        {
            (0, new Vector3(0,0,1)),
            (3, new Vector3(0,0,0)),
            (3, new Vector3(1,0,0)),
            (3, new Vector3(2,0,0)),
        };

        var grassBrickPattern = new List<(int, Vector3)>()
        {
            (1, new Vector3(0, 0, 1)),
            (3, new Vector3(0, 0, 0)),
            (3, new Vector3(1, 0, 0)),
            
            (2, new Vector3(2,0,1)),
            (3, new Vector3(2, 0, 0)),
        };

        var brickPattern = new List<(int, Vector3)>()
        {
            (1, new Vector3(1,0,0)),
            (1, new Vector3(0,0,1)),
            (1, new Vector3(1,0,2)),
            (2, new Vector3(0,0,0)),
            (2, new Vector3(3,0,0)),
        };

        var grassSoilPattern = new List<(int, Vector3)>()
        {
            (4, new Vector3(0,0,0)),
            (4, new Vector3(1,0,0)),
            (4, new Vector3(1,0,1)),
            (3, new Vector3(0,0,1)),
        };

        var emptyGrassPattern = new List<(int, Vector3)>()
        {
            (6, new Vector3(0, 0, 1)),
            (4, new Vector3(0, 0, 0))
        };

        var emptyBrickPattern = new List<(int, Vector3)>()
        {
            (1, new Vector3(1, 0, 0)),
            (6, new Vector3(0, 0, 0)),
            (6, new Vector3(3, 0, 0)),
            (6, new Vector3(1, 0, 1)),
            (6, new Vector3(1, 0, 2)),
        };
        
        var emptyHalfBrickPattern = new List<(int, Vector3)>()
        {
            (2, new Vector3(1, 0, 0)),
            (6, new Vector3(0, 0, 0)),
            (6, new Vector3(2, 0, 0)),
            (6, new Vector3(1, 0, 1)),
        };

        var emptyEmptyPattern = new List<(int, Vector3)>()
        {
            (6, new Vector3(0,0,0)),
            (6, new Vector3(0,0,1)),
            (6, new Vector3(1,0,0)),
            (6, new Vector3(1,0,1))
        };

        var patterns = new List<List<(int, Vector3)>>()
        {
            doorBrickPattern,
            doorGrassPattern,
            grassBrickPattern,
            brickPattern,
            grassSoilPattern,
            windowBrickPattern,
            emptyEmptyPattern,
            emptyBrickPattern,
            emptyHalfBrickPattern,
            emptyGrassPattern,
        };
        return patterns;
    }
    private Tile[] GetTetrisTiles()
    {
        var tileL = new Tile(
            new Vector3(2, 1, 3),
            new Color(240 / 255.0f, 160 / 255.0f, 0),
            new bool[,,] { { { true, true, true }, { true, false, false } } },
            null,
            computeAtomEdges:true
        );
        var tileT = new Tile(
            new Vector3(2, 1, 3),
            new Color(160 / 255.0f, 0, 240/255.0f),
            new bool[,,] { { { false, true, false }, { true, true, true }} },
            null,
            computeAtomEdges:true
        );
        
        var tileJ = new Tile(
            new Vector3(2, 1, 3),
            new Color(0, 0, 240 / 255.0f),
            new bool[,,] { { { true, false, false }, { true, true, true } } },
            null,
            computeAtomEdges:true
        );
        
        var tileI = new Tile(
            new Vector3(4, 1, 1),
            new Color(120 / 255.0f, 120 / 255.0f, 1 / 255.0f),
            new bool[,,] { { { true }, { true }, { true }, { true } } },
            null,
            computeAtomEdges:true
        );
        
        var tileS = new Tile(
            new Vector3(2, 1, 3),
            new Color(0, 240 / 255.0f, 0),
            new bool[,,] { { { true, true, false }, { false, true, true }} },
            null,
            computeAtomEdges:true
        );
        var tileZ = new Tile(
            new Vector3(2, 1, 3),
            new Color(240 / 255.0f, 0, 0),
            new bool[,,] { { { false, true, true }, { true, true, false }} },
            null,
            computeAtomEdges:true
        );
        var tileO = new Tile(
            new Vector3(2, 1, 2),
            new Color(0, 240 / 255.0f, 240 / 255.0f),

            // new Color(240 / 255.0f, 240 / 255.0f, 0),
            new bool[,,] { { { true, true }, { true, true }} },
            null,
            computeAtomEdges:true
        );

        var tetrisTiles = new Tile[] { tileL, tileT, tileJ, tileI, tileS, tileZ, tileO };
        return tetrisTiles;
    }

    private void InitXWFCInput(TileSet tiles, Vector3Int gridExtent, InputGrid[] inputGrids, float[] weights)
    {
        _xwfc = new ExpressiveWFC(tiles, gridExtent, inputGrids, AdjacencyMatrix.ToWeightDictionary(weights, tiles));
        UpdateExtent(gridExtent);
    }

    

    private void InitXWFComponent(ref Component component)
    {
        _xwfc = new ExpressiveWFC(component.AdjacencyMatrix, ref component.Grid);
        UpdateExtent(component.Grid.GetExtent());
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
    
    public void SaveComponent()
    {
        if (_xwfc == null) return;
        _currentComponent.Grid = _xwfc.GridManager.Grid.Deepcopy();
        _xwfc.RemoveEmpty(ref _currentComponent.Grid);
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

    public void UpdateTileSet(TileSet newTileSet)
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
        var origin = new Vector3Int(-100,-100,-5);
        var gap = new Vector3Int(5, 0, 0);
        foreach (var (key, value) in CompleteTileSet)
        {
            var maxIndex = new Vector3Int();
            bool labeled = false;
            foreach (var (index, _) in value.AtomIndexToIdMapping)
            {
                if (maxIndex.x < index.x) maxIndex.x = (int)index.x;
                var drawnAtom = Instantiate(unitTilePrefab);
                drawnAtom.transform.position = CalcAtomPosition(index, origin);
                
                UpdateColorFromTerminal(drawnAtom, key);
                _drawnTiles.Add(drawnAtom);

                if (labeled) continue;
                LabelTiles(drawnAtom, key.ToString());
                labeled = true;
                drawnTilePositions[key] = drawnAtom.transform.position;
            }

            origin += maxIndex;
            origin += gap;
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
        var e = _xwfc.GridExtent;
        if (_drawnGrid != null) e = _drawnGrid.GetExtent(); 
        return new Grid<Drawing>(e, new Drawing(_xwfc.GridManager.Grid.DefaultFillValue, null));
    }

    public Vector3 GetUnitSize()
    {
        return _unitSize;
    }

    public Vector3Int[] GetOffsets()
    {
        return _xwfc.Offsets;
    }

    public Dictionary<int, Tile> GetTiles()
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
        
            // while (MayIterate() && MayCollapse())
            while (MayIterate() && MayCollapse())
            {
                CollapseOnce();
                _iterationsDone++;
            }
            
            Draw(new Vector3Int(0,0,0));
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

    public void CollapseOnce()
    {
        if (MayCollapse()) _xwfc.CollapseOnce();
    }

    public void CollapseAndDrawOnce()
    {
        /*
         * Performs a single collapse and draws the resulting atoms.
         * Returns the set of affected cells.
         */
        CollapseOnce();
        Draw(new Vector3Int(0,0,0));
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

    private void Draw(Vector3Int origin)
    {
        var grid = _xwfc.GridManager.Grid;
        var gridExtent = grid.GetExtent();
        for (int y = 0; y < gridExtent.y; y++)
        {
            for (int x = 0; x < gridExtent.x; x++)
            {
                for (int z = 0; z < gridExtent.z; z++)
                {
                    var drawing = _drawnGrid.Get(x, y, z);
                    var gridValue = grid.Get(x, y, z);
                    // Drawing.DestroyAtom(drawing);
                    // if (gridValue != grid.DefaultFillValue) DrawAtom(new Vector3(x,y,z),gridValue);

                    var blockedCellId =
                        ExpressiveWFC.BlockedCellId(grid.DefaultFillValue, _xwfc.AdjMatrix.TileSet.Keys);
                    var coord = new Vector3(x, y, z);
                    if (gridValue == grid.DefaultFillValue && drawing.Atom != null)
                    {
                        Drawing.DestroyAtom(drawing);
                        _drawnGrid.Set(coord, new Drawing(grid.DefaultFillValue, null));
                    }
                    else if (gridValue == blockedCellId && drawing.Atom != null)
                    {
                        Drawing.DestroyAtom(drawing);
                        _drawnGrid.Set(coord, new Drawing(blockedCellId, null));
                    }
                    else if (drawing.Id != gridValue)
                    {
                        if (drawing.Atom != null)
                        {
                            Drawing.DestroyAtom(drawing);
                        }
                        DrawAtom(coord, gridValue, origin);
                    }
                    
                    if (_drawnGrid.Get(coord).Id != grid.Get(coord))
                    {
                        Debug.Log("Somehow not matched...");
                    }
                }
            }
        }
    }

    private void DrawAtom(Vector3 coord, int atomId, Vector3Int origin)
    {
        if (!_xwfc.AdjMatrix.AtomMapping.ContainsValue(atomId)) return;
        
        var atom = Instantiate(unitTilePrefab);
                
        atom.transform.position = CalcAtomPosition(coord, origin);
        var edges = DrawEdges(atomId, atom);
        var drawing = new Drawing(atomId, atom, edges);
        UpdateColorFromAtom(atom, atomId);
        _drawnGrid.Set(coord + origin, drawing);
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
        obj.GetComponent<Renderer>().material.color = CompleteTileSet[tileId].Color;
    }

    private Color GetTerminalColorFromAtom(int atomId)
    {
        return _xwfc.AdjMatrix.GetTerminalFromAtomId(atomId).Color;
    }

    private Vector3 CalcAtomPosition(Vector3 coord, Vector3Int origin)
    {
        return Vector3Util.Mult(origin + coord, _unitSize);
    }

    public void Reset()
    {
        _xwfc?.UpdateExtent(extent);
        ResetDrawnGrid();
        _activeStateFlag = 0;
    }

    public void ResetDrawnGrid()
    {
        _drawnGrid?.Map(Drawing.DestroyAtom);
        _drawnGrid = InitDrawGrid();
    }
    
    public void UpdateExtent(Vector3Int newExtent)
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

    public string SaveConfig()
    {
        /*
         * To replicate a config need:
         * Json representation of AdjacencyMatrix.
         * This contains the tileset, the tile id mapping and the adjacency constraints.
         * From those, the parameters for recreating config can be restored.
         */
        var adjs = _xwfc.AdjMatrix.TileAdjacencyConstraints;
        var tiles = TileSet;
        var config = new AdjacencyMatrixJsonFormatter(adjs, tiles).ToJson();
        var path = CreateSaveConfigPath();
        FileUtil.WriteToFile(config, path);
        return path;
    }

    private string GetConfigFolderPath()
    {
        return FileUtil.RootPathTo("Configs");
    }

    public void LoadConfig(string fileName="")
    {
        var extension = ".json";
        
        var path = "";
        if (fileName.Length == 0)
        {
            var files = FindConfigPaths();
            path = files.Last();
        }
        else
        {
            if (!fileName.EndsWith(extension)) fileName += extension;
            path = Path.Join(GetConfigFolderPath(), fileName);
        }
        var contents = FileUtil.ReadFromFile(path);
        var adjMat = AdjacencyMatrixJsonFormatter.FromJson(contents);
        
        UpdateTileSet(adjMat.TileSet);
        UpdateAdjacencyConstraints(adjMat.TileAdjacencyConstraints);
    }

    private string CreateSaveConfigPath()
    {
        var prefix = "config-";
        var timeStamp = FileUtil.GetTimeStamp();
        var format = ".json";
        var fileName = $"{prefix}{timeStamp}{format}";
        var filePath = Path.Join(GetConfigFolderPath(), fileName);

        return filePath;
    }

    public IEnumerable<string> FindConfigPaths()
    {
        var files = FileUtil.FindFiles(GetConfigFolderPath(), "*.json").ToArray();
        return files;
    }

    public IEnumerable<string> FindConfigFileNames(bool includeExtension = false)
    {
        var paths = FindConfigPaths();
        foreach (var path in paths)
        {
            yield return FileUtil.GetFileNameFromPath(path, includeExtension);
        }
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
