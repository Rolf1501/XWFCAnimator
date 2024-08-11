using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;

public class XWFCAnimator : MonoBehaviour
{
    [SerializeField] private GameObject unitTilePrefab;
    [SerializeField] private GameObject thinTilePrefab;
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private Canvas tileLabelPrefab;
    [SerializeField] private int RandomSeed;
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
    public XwfcModel activeModel = XwfcModel.Overlapping;
    
    private XwfcStm _xwfc;

    private Grid<Drawing> _drawnGrid;
    private HashSetAdjacency _adjacency;

    private HashSet<GameObject> _drawnTiles;

    private ComponentManager _componentManager;
    private Component _currentComponent;

    private PatternMatrix _patternMatrix;

    private Vector3Int _kernelSize = new Vector3Int(2, 1, 2);

    private XWFC.Timer _timer = new();
    
    [Flags]
    private enum StateFlag
    {
        Collapsing = 1 << 0
    }

    [Flags]
    public enum XwfcModel
    {
        Overlapping = 1 << 0,
        SimpleTiled = 1 << 1
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
        _timer.Start();
        TileSet = new TileSet();

        _adjacency = new HashSetAdjacency();

        // InitXWFC();
        // _activeModel = XwfcModel.SimpleTiled;


        if (activeModel == XwfcModel.SimpleTiled)
        {
            // var legoTiles = LegoSet.GetLegoSubset(new []{"p211", "p212", "void"});
            var tetrisTile = new TetrisSet().GetTetrisTileSet();
            // var tetrisTile = TetrisSet.GetTetrisTileSet().GetSubset(new []{"J","Z","L","T", "S"});
            // var tetrisTile = TetrisSet.GetLargeTetrisSet().GetSubset(new []{"L","S","T","O","I"});
            // var tetrisTileLarge = TetrisSet.GetLargeTetrisSet().GetSubset(new []{"SL", "ZL", "TL", "OL"});
            // tetrisTile.Join(tetrisTileLarge);
            // PrintAdjacencyData(adjMat);
            // var components = new Component[] { 
            //     new (new Vector3Int(0, 0, 0), new Vector3Int(20, 1, 20), adjMat.TileSet, adjMat.TileAdjacencyConstraints),
            //     new (new Vector3Int(20, 0, 0), new Vector3Int(40, 1, 40), adjMat.TileSet, adjMat.TileAdjacencyConstraints),
            //     new (new Vector3Int(0, 0, 40), new Vector3Int(60, 1, 60), adjMat.TileSet, adjMat.TileAdjacencyConstraints),
            // };


            var bricks = new[]
            {
                // "b112", 
                "b312", 
                "b412", 
                // "b212", 
                "b115", 
                "b211", 
                "b213", 
                // "b216", 
                "p211", 
                "p212", 
                "p414", 
                "p317",
                "void",
                // "voidBrick",
            };
            var plateAtoms = true;
            if (plateAtoms) unitTilePrefab = thinTilePrefab;
            var legoTiles = new LegoSet(plateAtoms).GetLegoSet().GetSubset(bricks);
            
                // new(
                //     "b214",
                //     new Vector3Int(2,unit,4),
                //     new Color32(201, 26, 9, 255)
                // ),
                // new(
                //     "b216",
                //     new Vector3Int(2,unit,6),
                //     new Color32(120, 100, 160, 255)
                // ),
                //
                // // Plates
                // new(
                //     "p111",
                //     new Vector3Int(1,1,1),
                //     new Color(1,0,0)
                // ),
                // new(
                //     "p211",
                //     new Vector3Int(2,1,1),
                //     new Color32(242, 205, 55, 255)
                // ),
                // new(
                //     "p212",
                //     new Vector3Int(2,1,2),
                //     new Color32(165, 165, 203, 255)
                // ),
            var activeSet = legoTiles;
            var unit = LegoSet.BrickUnitSize(plateAtoms);
            SaveConfig(new AdjacencyMatrix(new HashSetAdjacency(), activeSet, null));
            
            var adjMat = ReadConfig();
            LoadConfig();

            var e1 = new Vector3Int(20, unit, 20);
            var c2 = new Vector3Int(e1.x, e1.y-2, 5);
            var c3 = new Vector3Int(10, e1.y, e1.z);
            var components = new Component[] { 
                new (new Vector3Int(0, 0, 0), e1, adjMat.TileSet, adjMat.TileAdjacencyConstraints),
                new (c2, e1, adjMat.TileSet, adjMat.TileAdjacencyConstraints),
                new (c3, e1, adjMat.TileSet, adjMat.TileAdjacencyConstraints),
            };
            _componentManager = new ComponentManager(components);
        }
        else
        {
            var set = new TetrisSet(true);
            var (tiles, sample )= set.Example();
            var (tF, sF )= set.FillExample();
            var (tF1, sF1 )= set.FillExample();
            var (tF2, sF2 )= set.FillExample();
            foreach (var (k,v) in tF1)
            {
                v.Color = new Color(0.8f, 0, 0.1f);
            }
            foreach (var (k,v) in tF2)
            {
                v.Color = new Color(0.1f, 0.6f, 0.1f);
            }

            var e0 = new Vector3Int(40, 2, 40);
            var o0 = new Vector3Int(0, 0, 0);
            var o1 = new Vector3Int(0, 0, e0.z);
            var e1 = new Vector3Int(20, 2, 10);
            var o2 = new Vector3Int(0,0,e1.z) + o1;
            var e2 = new Vector3Int(35, 2, 20);
            var o3 = new Vector3Int(e1.x, 0, 0) + o1;
            var e3 = new Vector3Int(20,2,20);
            var components = new Component[] { 
                new (
                o0, 
                e0, 
                tiles,sample.ToArray(),
                customSeed:20,
                offsetMode:OffsetMode.Max
                ),
                new (
                    o1,
                    e1,
                    tF,sF.ToArray(), offsetMode:OffsetMode.Median),
                new (
                o2,
                e2,
                tF1,sF1.ToArray(), offsetMode:OffsetMode.Max),
                new (
                    o3,
                    e3,
                    tF2,sF2.ToArray(), offsetMode:OffsetMode.Median)
            };
            // var components = LegoSet.LegoHouse();
            // var houseComponents = HouseComponents();
            _componentManager = new ComponentManager(components);
        }

        Debug.Log("\tInitialization DONE");
        
        LoadNextComponent();
        
        TileSet = _xwfc.AdjMatrix.TileSet;
        CompleteTileSet = _xwfc.AdjMatrix.TileSet;
        _timer.Stop();

        // Grid for keeping track of drawn atoms.
        _drawnGrid = InitDrawGrid();

        // PrintAdjacencyData();

        _unitSize = unitTilePrefab.GetComponent<Renderer>().bounds.size;

        // Set for keeping track of drawn terminals.
        _drawnTiles = new HashSet<GameObject>();

        DrawTiles();
    }

    private float CollapseAll()
    {
        _timer.Start();
        _xwfc.CollapseAutomatic();
        return _timer.Stop();
    }


    public void Assemble()
    {
        if (_componentManager.HasNext()) return;
        SaveComponent();
        Reset();

        /*
         * Find smallest and largest coord.
         */
        _activeStateFlag = 0;
        
        var (_, max) = _componentManager.BoundingBox();
        
        var defaultValue = _xwfc.GetGrid().DefaultFillValue;
        _drawnGrid = new Grid<Drawing>(max, new Drawing(defaultValue, null));
            
        foreach (var component in _componentManager.Components)
        {
            var origin = component.Origin;
            var grid = component.Grid;
            var e = grid.GetExtent();
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        DrawAtom(new Vector3Int(x,y,z), grid.Get(x,y,z), origin, component.AdjacencyMatrix);
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
        Debug.Log("Loading Component...");
        var timer = new Timer();
        timer.Start();
        if (!HasNextComponent()) return;
        
        SaveComponent();
        
        /*
         * Before selecting the next component, translate all unsolved components depending on the results of the current component.
         */
        _componentManager.TranslateCurrentSolved();
        _componentManager.TranslateUnsolved();
        
        var id = _componentManager.Next();
        _componentManager.SeedComponent(id);
        
        _currentComponent = _componentManager.Components[id];
        
        // Intersection with next component.
        
        InitXWFComponent(ref _currentComponent);
        var t = timer.Stop();
        Debug.Log($"Loaded Component! Time: {t}");

        TileSet = _currentComponent.Tiles;
        CompleteTileSet = TileSet;
        _activeStateFlag = 0;
        ResetDrawnGrid();
    }

    private TileSet ToTileSet(NonUniformTile[] tiles)
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

    private Component[] HouseComponents()
    {
        var (_, weights) = HouseTiles();

        // var activeTileSet = tetrisTiles;

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
        var baseGrid = InputHandler.PatternsToGrids(BasePatterns(), baseTileSet, "");

        var floorTileSet = ToTileSet(FloorTiles());
        var floorGrid = InputHandler.PatternsToGrids(FloorPatterns(), floorTileSet, "");
        
        var baseComponent = new Component(baseOrigin, baseExtent, baseTileSet, baseGrid.ToArray(), weights);
        var floorComponent = new Component(floorOrigin, floorExtent, floorTileSet, floorGrid.ToArray(), weights);
        // var roof = new Component(roofOrigin, roofExtent, tileSet, grids.ToArray(), weights);
        var components = new Component[] { baseComponent, floorComponent };//, floor, roof };

        return components;
    }

    private NonUniformTile[] BaseTiles()
    {
        var grassTile = new NonUniformTile(
            "g",
            new Vector3Int(1,1,1), 
            color: new Color(0,0.2f,0,1) 
        );
        
        var soilTile = new NonUniformTile(
            "s",
            new Vector3Int(1,1,1), 
            color: new Color(0.1f,0.1f,0.1f,1),
            computeAtomEdges: false
        );
        
        var brickTile = new NonUniformTile(
            "b0",
            new Vector3Int(2,1,1), 
            color: new Color(0.8f,1,0,1)
        );
        
        var emptyTile = new NonUniformTile(
            ".",
            new Vector3Int(1, 1, 1),
            new Color(0, 0.2f, 0.5f, 0.2f),
            description: "empty",
            computeAtomEdges:false,
            isEmptyTile:true
        );
        
        var halfBrickTile = new NonUniformTile(
            "b1",
            new Vector3Int(1,1,1), 
            color: new Color(0.4f,0.6f,0.4f,1)
        );
        var doorTile = new NonUniformTile(
            "d", 
            new Vector3Int(3, 1, 5), 
            new Color(0.4f, 0.2f, 0.1f)
        );

        return new NonUniformTile[] { brickTile, grassTile, soilTile, emptyTile, halfBrickTile, doorTile };
    }

    private List<Patterns> BasePatterns()
    {
        var grassBrickPattern = new Patterns()
        {
            // b,b,b,b,b1,b,b
            // g,g,g,g,g ,g,g
            (0, new Vector3Int(0, 0, 1)),
            (0, new Vector3Int(2, 0, 1)),
            (4, new Vector3Int(4, 0, 1)),
            (0, new Vector3Int(5, 0, 1)),
            (1, new Vector3Int(0, 0, 0)),
            (1, new Vector3Int(1, 0, 0)),
            (1, new Vector3Int(2, 0, 0)),
            (1, new Vector3Int(3, 0, 0)),
            (1, new Vector3Int(4, 0, 0)),
            (1, new Vector3Int(5, 0, 0)),
            (1, new Vector3Int(6, 0, 0)),
        };

        var brickPattern = new Patterns()
        {
            //  b,b,b,b
            // b1,b,b,b1
            //  b,b,b,b
            (0, new Vector3Int(2, 0, 0)),
            (0, new Vector3Int(0, 0, 0)),
            (0, new Vector3Int(2, 0, 2)),
            (0, new Vector3Int(0, 0, 2)),
            (0, new Vector3Int(1, 0, 1)),
            (4, new Vector3Int(0,0,1)),
            (4, new Vector3Int(3,0,1)),
        };
        
        var brickDoorPattern = new Patterns
        {
            /*
             * b ,b ,b ,b ,b ,b
             *   ,b1,d ,d ,d ,b , b
             * b ,b ,d ,d ,d ,b1,
             *   ,b1,d ,d ,d ,b , b
             * b ,b ,d ,d ,d ,b1,
             *   ,b1,d ,d ,d ,b , b
             */
            (0, new Vector3Int(0,0,1)),
            (0, new Vector3Int(0,0,3)),
            (0, new Vector3Int(0,0,5)),
            (0, new Vector3Int(2,0,5)),
            (0, new Vector3Int(4,0,5)),
            (0, new Vector3Int(5,0,4)),
            (0, new Vector3Int(5,0,2)),
            (0, new Vector3Int(5,0,0)),
            
            (4, new Vector3Int(1,0,0)),
            (4, new Vector3Int(1,0,2)),
            (4, new Vector3Int(1,0,4)),
            (4, new Vector3Int(5,0,1)),
            (4, new Vector3Int(5,0,3)),
            
            (5, new Vector3Int(2,0,0))
        };

        var doorGrassPattern = new Patterns
        {
            /*
             *    d,d,d
             *    d,d,d
             *    d,d,d
             *    d,d,d
             * b1,d,d,d,b,b
             * g ,g,g,g,g,g
             */
            (5, new Vector3Int(1, 0, 1)),

            (4, new Vector3Int(0, 0, 1)),
            (0, new Vector3Int(4, 0, 1)),

            (1, new Vector3Int(0, 0, 0)),
            (1, new Vector3Int(1, 0, 0)),
            (1, new Vector3Int(2, 0, 0)),
            (1, new Vector3Int(3, 0, 0)),
            (1, new Vector3Int(4, 0, 0)),
            (1, new Vector3Int(5, 0, 0)),
        };

        var grassSoilPattern = new Patterns()
        {
            // g,g,s,s
            // s,s,s,s
            (1, new Vector3Int(0,0,1)),
            (1, new Vector3Int(1,0,1)),
            (2, new Vector3Int(0,0,0)),
            (2, new Vector3Int(1,0,0)),
            (2, new Vector3Int(2,0,1)),
            (2, new Vector3Int(2,0,0)),
            (2, new Vector3Int(3,0,0)),
            (2, new Vector3Int(3,0,1)),
        };

        var emptyGrassPattern = new Patterns()
        {
            // e,e
            // g,g
            (3, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(1, 0, 1)),
            (1, new Vector3Int(0, 0, 0)),
            (1, new Vector3Int(1, 0, 0))
        };
        
        var emptyBrickPattern = new Patterns()
        {
            //   e,e,e,e
            // e,e,b,b,e,e
            // e,b,b,b,b,e
            (0, new Vector3Int(1, 0, 0)),
            (0, new Vector3Int(3, 0, 0)),
            (0, new Vector3Int(2, 0, 1)),
            (3, new Vector3Int(0, 0, 0)),
            (3, new Vector3Int(5, 0, 0)),
            (3, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(1, 0, 1)),
            (3, new Vector3Int(4, 0, 1)),
            (3, new Vector3Int(5, 0, 1)),
            (3, new Vector3Int(1, 0, 2)),
            (3, new Vector3Int(2, 0, 2)),
            (3, new Vector3Int(3, 0, 2)),
            (3, new Vector3Int(4, 0, 2)),
        };

        var emptyEmptyPattern = new Patterns()
        {
            // e,e
            // e,e
            (3, new Vector3Int(0, 0, 0)),
            (3, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(1, 0, 0)),
            (3, new Vector3Int(1, 0, 1)),
        };

        return new List<Patterns>()
            { brickPattern, emptyBrickPattern, emptyEmptyPattern, emptyGrassPattern, grassBrickPattern, grassSoilPattern, doorGrassPattern, brickDoorPattern };
    }

    private NonUniformTile[] FloorTiles()
    {
        var brickTile = new NonUniformTile(
            "b0",
            new Vector3Int(2,1,1), 
            color: new Color(1,0.3f,0.3f,1)
        );
        
        var emptyTile = new NonUniformTile(
            ".",
            new Vector3Int(1, 1, 1),
            new Color(0.5f, 0.2f, 0.5f, 0.2f),
            description: "empty",
            computeAtomEdges:false,
            isEmptyTile:true
        );
        
        var halfBrickTile = new NonUniformTile(
            "b1",
            new Vector3Int(1,1,1), 
            color: new Color(0.7f,0.6f,0.3f,1)
        );
        
        var windowTile = new NonUniformTile(
            "w",
            new Vector3Int(3, 1, 3),
            color: new Color(0, 0, 0.8f)
        );

        return new NonUniformTile[] { brickTile, halfBrickTile, windowTile, emptyTile };
    }

    private List<Patterns> FloorPatterns()
    {
        var brickPattern = new Patterns()
        {
            //  b,b,b,b
            // b1,b,b,b1
            //  b,b,b,b
            (0, new Vector3Int(2, 0, 0)),
            (0, new Vector3Int(0, 0, 0)),
            (0, new Vector3Int(2, 0, 2)),
            (0, new Vector3Int(0, 0, 2)),
            (0, new Vector3Int(1, 0, 1)),
            (1, new Vector3Int(0,0,1)),
            (1, new Vector3Int(3,0,1)),
        };

        var emptyBrickPattern = new Patterns()
        {
            // e,e,e,e,e,e
            // e,b,b,b,b,e
            (0, new Vector3Int(1, 0, 0)),
            (0, new Vector3Int(3, 0, 0)),
            (3, new Vector3Int(0, 0, 0)),
            (3, new Vector3Int(5, 0, 0)),
            (3, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(1, 0, 1)),
            (3, new Vector3Int(2, 0, 1)),
            (3, new Vector3Int(3, 0, 1)),
            (3, new Vector3Int(4, 0, 1)),
            (3, new Vector3Int(5, 0, 1)),
        };

        var emptyEmptyPattern = new Patterns()
        {
            // e,e
            // e,e
            (3, new Vector3Int(0, 0, 0)),
            (3, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(1, 0, 0)),
            (3, new Vector3Int(1, 0, 1)),
        };
        
        var windowBrickPattern = new Patterns()
        {
            /* b,b ,b,b,b,b
             *   b1,w,w,w,b ,b
             * b,b ,w,w,w,b1
             *   b1,w,w,w,b ,b
             * b,b ,b,b,b,b    
             */
            (2, new Vector3Int(2, 0, 1)),

            (1, new Vector3Int(1, 0, 1)),
            (0, new Vector3Int(0, 0, 2)),
            (1, new Vector3Int(1, 0, 3)),
            
            (0, new Vector3Int(5, 0, 1)),
            (1, new Vector3Int(5, 0, 2)),
            (0, new Vector3Int(5, 0, 3)),

            (0, new Vector3Int(0, 0, 0)),
            (0, new Vector3Int(2, 0, 0)),
            (0, new Vector3Int(4, 0, 0)),
            
            (0, new Vector3Int(0, 0, 4)),
            (0, new Vector3Int(2, 0, 4)),
            (0, new Vector3Int(4, 0, 4)),
        };

        return new List<Patterns>() { brickPattern, emptyBrickPattern, windowBrickPattern, emptyEmptyPattern };
    }

    private void PrintAdjacencyData(AdjacencyMatrix adjacencyMatrix)
    {
        foreach (var o in adjacencyMatrix.AtomAdjacencyMatrix.Keys)
        {
            var x = adjacencyMatrix.AtomAdjacencyMatrix[o];
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
        
        foreach (var (k,v) in adjacencyMatrix.AtomMapping.Dict)
        {
            Debug.Log($"{k}: {v}");
        }
    }
    
    public void PrintAdjacencyData()
    {
        PrintAdjacencyData(_xwfc.AdjMatrix);
    }

    private (NonUniformTile[] houseTiles, float[] weights) HouseTiles()
    {
        var doorTile = new NonUniformTile(
            "d", 
            new Vector3Int(3, 1, 5), 
            new Color(0, 0, 0.8f)
        );
        
        var brickTile = new NonUniformTile(
            "b0",
            new Vector3Int(2,1,1), 
            color: new Color(0.8f,0,0,1)
        );
        
        var halfBrickTile = new NonUniformTile(
            "b1",
            new Vector3Int(1,1,1), 
            color: new Color(0.4f,0,0.4f,1)
        );
        
        var grassTile = new NonUniformTile(
            "g",
            new Vector3Int(1,1,1), 
            color: new Color(0,0.8f,0,1) 
        );
        
        var soilTile = new NonUniformTile(
            "s",
            new Vector3Int(1,1,1), 
            color: new Color(0.1f,0.1f,0.1f,1),
            computeAtomEdges: false
        );

        var windowTile = new NonUniformTile(
            "w",
            new Vector3Int(3, 1, 3),
            color: new Color(0, 0, 0.8f)
        );

        var emptyTile = new NonUniformTile(
            ".",
            new Vector3Int(1, 1, 1),
            new Color(0, 0.2f, 0.5f, 0.2f),
            description: "empty",
            computeAtomEdges:false,
            isEmptyTile:true
        );
        
        var houseTiles = new NonUniformTile[] { doorTile, brickTile, halfBrickTile, grassTile, soilTile, windowTile, emptyTile };
        var weights = new float[] { 1, 1, 1, 1, 1, 1, 1};
        return (houseTiles, weights);
    }
    private List<Patterns> HousePatterns()
    {
        var doorBrickPattern = new Patterns()
        {
            (0, new Vector3Int(2,0,0)),
            
            (1, new Vector3Int(0,0,1)),
            (1, new Vector3Int(0,0,3)),
            (1, new Vector3Int(0,0,5)),
            
            (1, new Vector3Int(2,0,5)),
            (1, new Vector3Int(4,0,5)),
            
            (1, new Vector3Int(5,0,4)),
            (1, new Vector3Int(5,0,2)),
            (1, new Vector3Int(5,0,0)),
            
            (2, new Vector3Int(1,0,0)),
            (2, new Vector3Int(1,0,2)),
            (2, new Vector3Int(1,0,4)),
            
            (2, new Vector3Int(5,0,1)),
            (2, new Vector3Int(5,0,3)),
        };

        var windowBrickPattern = new Patterns()
        {
            (5, new Vector3Int(2, 0, 1)),

            (2, new Vector3Int(1, 0, 1)),
            (1, new Vector3Int(0, 0, 2)),
            (2, new Vector3Int(1, 0, 3)),
            
            (2, new Vector3Int(5, 0, 1)),
            (1, new Vector3Int(5, 0, 2)),
            (2, new Vector3Int(5, 0, 3)),

            (1, new Vector3Int(2, 0, 0)),
            (2, new Vector3Int(4, 0, 0)),
            
            (2, new Vector3Int(2, 0, 4)),
            (1, new Vector3Int(3, 0, 4)),
        };

        var doorGrassPattern = new Patterns()
        {
            (0, new Vector3Int(0,0,1)),
            (3, new Vector3Int(0,0,0)),
            (3, new Vector3Int(1,0,0)),
            (3, new Vector3Int(2,0,0)),
        };

        var grassBrickPattern = new Patterns()
        {
            (1, new Vector3Int(0, 0, 1)),
            (3, new Vector3Int(0, 0, 0)),
            (3, new Vector3Int(1, 0, 0)),
            
            (2, new Vector3Int(2,0,1)),
            (3, new Vector3Int(2, 0, 0)),
        };

        var brickPattern = new Patterns()
        {
            (1, new Vector3Int(1,0,0)),
            (1, new Vector3Int(0,0,1)),
            (1, new Vector3Int(1,0,2)),
            (2, new Vector3Int(0,0,0)),
            (2, new Vector3Int(3,0,0)),
        };

        var grassSoilPattern = new Patterns()
        {
            (4, new Vector3Int(0,0,0)),
            (4, new Vector3Int(1,0,0)),
            (4, new Vector3Int(1,0,1)),
            (3, new Vector3Int(0,0,1)),
        };

        var emptyGrassPattern = new Patterns()
        {
            (6, new Vector3Int(0, 0, 1)),
            (4, new Vector3Int(0, 0, 0))
        };

        var emptyBrickPattern = new Patterns()
        {
            (1, new Vector3Int(1, 0, 0)),
            (6, new Vector3Int(0, 0, 0)),
            (6, new Vector3Int(3, 0, 0)),
            (6, new Vector3Int(1, 0, 1)),
            (6, new Vector3Int(1, 0, 2)),
        };
        
        var emptyHalfBrickPattern = new Patterns()
        {
            (2, new Vector3Int(1, 0, 0)),
            (6, new Vector3Int(0, 0, 0)),
            (6, new Vector3Int(2, 0, 0)),
            (6, new Vector3Int(1, 0, 1)),
        };

        var emptyEmptyPattern = new Patterns()
        {
            (6, new Vector3Int(0,0,0)),
            (6, new Vector3Int(0,0,1)),
            (6, new Vector3Int(1,0,0)),
            (6, new Vector3Int(1,0,1))
        };

        var patterns = new List<Patterns>()
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
    
    
    

    private void InitXWFCInput(TileSet tiles, Vector3Int gridExtent, SampleGrid[] inputGrids, float[] weights)
    {
        _xwfc = new XWFC.XwfcStm(tiles, gridExtent, inputGrids, AdjacencyMatrix.ToWeightDictionary(weights, tiles));
        UpdateExtent(gridExtent);
    }
    
    private void InitXWFComponent(ref Component component)
    {
        Debug.Log("Initializing component...");
        if (activeModel == XwfcModel.Overlapping)
        {
            _kernelSize = new Vector3Int(2, 2, 2);
            _xwfc = new XwfcOverlappingModel(component.AdjacencyMatrix.AtomizedSamples, component.AdjacencyMatrix,
                ref component.Grid, _kernelSize, RandomSeed);
        }
        else
        {
            _xwfc = new XwfcStm(component.AdjacencyMatrix, ref component.Grid, RandomSeed);
        }

        if (component.CustomSeed >= 0)
        {
            RandomSeed = component.CustomSeed;
        }
        _xwfc.UpdateRandom(RandomSeed);
        extent = component.Grid.GetExtent();

        Debug.Log("Loaded component!");
    }

    private void InitXWFC()
    {
        try
        {
            _xwfc = new XWFC.XwfcStm(TileSet, _adjacency, extent);

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
        _currentComponent.Grid = _xwfc.GetGrid().Deepcopy();
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
        foreach (var tile in _drawnTiles)
        {
            Destroy(tile);
        }
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
                // LabelTiles(drawnAtom, key.ToString());
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
        var e = extent;
        // if (_drawnGrid != null) e = _drawnGrid.GetExtent(); 
        return new Grid<Drawing>(e, new Drawing(_xwfc.GetGrid().DefaultFillValue, null));
    }

    public Vector3 GetUnitSize()
    {
        return _unitSize;
    }

    public Vector3Int[] GetOffsets()
    {
        return OffsetFactory.GetOffsets(3);
    }

    public Dictionary<int, NonUniformTile> GetTiles()
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

            if (stepSize < 0)
            {
                _timer.Start();
            }
            while (MayIterate() && MayCollapse())
            {
                CollapseOnce();
                _iterationsDone++;
            }

            if (stepSize < 0)
            {
                var time = _timer.Stop();
                Debug.Log($"Time taken: {time}");
            }
            
            Draw(new Vector3Int(0,0,0));
        }
        
        if (!MayCollapse())
        {
            string s = DrawnGridIdsToString();
            Debug.Log("DRAWN GRID:\n" + s);
            Debug.Log("XWFC GRID:\n" + _xwfc.GetGrid().GridToString());
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
        _xwfc.RandomSeed = RandomSeed;
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
        var grid = _xwfc.GetGrid();
        var gridExtent = grid.GetExtent();
        var blockedCellId = XwfcStm.BlockedCellId(grid.DefaultFillValue, _xwfc.AdjMatrix.TileSet.Keys);
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

                    var coord = new Vector3Int(x, y, z);
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
                        DrawAtom(coord, gridValue, origin, _xwfc.AdjMatrix);
                    }
                    
                    if (_drawnGrid.Get(coord).Id != grid.Get(coord))
                    {
                        Debug.Log("Somehow not matched...");
                    }
                }
            }
        }
    }

    private void DrawAtom(Vector3Int coord, int atomId, Vector3Int origin, AdjacencyMatrix adjacencyMatrix)
    {
        if (!adjacencyMatrix.AtomMapping.ContainsValue(atomId)) return;
        
        var atom = Instantiate(unitTilePrefab);
                
        atom.transform.position = CalcAtomPosition(coord, origin);
        var edges = DrawEdges(atomId, atom, adjacencyMatrix);
        var drawing = new Drawing(atomId, atom, edges);
        UpdateColorFromAtom(atom, atomId, adjacencyMatrix);
        _drawnGrid.Set(coord + origin, drawing);
    }

    private HashSet<GameObject> DrawEdges(int atomId, GameObject atom, AdjacencyMatrix adjacencyMatrix)
    {
        var bounds = atom.GetComponent<Renderer>().bounds.size;
        var edgeExtent = new Vector3(0.05f,0.05f,0.05f);
        var origin = atom.transform.position;
        var halfBounds = Vector3Util.Scale(bounds, 0.5f);
        var zeroOrigin = origin - halfBounds;
        
        var (terminalId, atomIndex, _) = adjacencyMatrix.AtomMapping.Get(atomId);
        
        var edges = adjacencyMatrix.TileSet[terminalId].AtomEdges;
        var edgeObjects = new HashSet<GameObject>();
        var edgeColor = new Color(0, 0, 0);
        if (edges == null) return edgeObjects;

        var atomEdges = edges[atomIndex];
        foreach (var atomEdge in atomEdges)
        {
            var edge = Instantiate(edgePrefab);
            edge.GetComponent<Renderer>().material.color = edgeColor;
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

    private void UpdateColorFromAtom(GameObject obj, int atomId, AdjacencyMatrix adjacencyMatrix, bool applyFluctuation=true)
    {
        var color = GetTerminalColorFromAtom(atomId, adjacencyMatrix);
        if (applyFluctuation) color = ApplyVariation(color, 0.2f);
        obj.GetComponent<Renderer>().material.color = color;
    }

    private void UpdateColorFromTerminal(GameObject obj, int tileId)
    {
        obj.GetComponent<Renderer>().material.color = CompleteTileSet[tileId].Color;
    }

    private Color GetTerminalColorFromAtom(int atomId, AdjacencyMatrix adjacencyMatrix)
    {
        return adjacencyMatrix.GetTerminalFromAtomId(atomId).Color;
    }

    private Vector3 CalcAtomPosition(Vector3 coord, Vector3Int origin)
    {
        return Vector3Util.Mult(origin + coord, _unitSize);
    }

    public void Reset()
    {
        ResetDrawnGrid();
        _xwfc.UpdateExtent(extent);
        _xwfc.UpdateRandom(RandomSeed);
        _activeStateFlag = 0;
    }

    public void ResetDrawnGrid()
    {
        _drawnGrid?.Map(Drawing.DestroyAtom);
        _drawnGrid = InitDrawGrid();
    }
    
    public void UpdateExtent(Vector3Int newExtent)
    {
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
    
    public string SaveConfig(AdjacencyMatrix adjacencyMatrix)
    {
        /*
         * To replicate a config need:
         * Json representation of AdjacencyMatrix.
         * This contains the tileset, the tile id mapping and the adjacency constraints.
         * From those, the parameters for recreating config can be restored.
         */
        var adjs = adjacencyMatrix.TileAdjacencyConstraints;
        var tiles = adjacencyMatrix.TileSet;
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

        var adjMat = ReadConfig(fileName);
        UpdateTileSet(adjMat.TileSet);
        UpdateAdjacencyConstraints(adjMat.TileAdjacencyConstraints);
    }

    private AdjacencyMatrix ReadConfig(string fileName="")
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

        return adjMat;
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
