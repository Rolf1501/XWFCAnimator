using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWFC;
using Patterns = System.Collections.Generic.List<(int, UnityEngine.Vector3Int)>;

public class CogSet : TileSet
{
    private TileSet tiles;

    public CogSet()
    {
        tiles = new TileSet();
    }

    public TileSet GetSet()
    {
        if (tiles.Any()) return tiles;

        tiles = new InputReader().ReadNUT();
        tiles.Add(tiles.Count, new(
                    "void",
                    new Vector3Int(1, 1, 1),
                    new Color(0, 0, 0, 0f),
                    isEmptyTile: true, computeAtomEdges: false
                ));
        return tiles;
    }
    
    public (string[] t, SampleGrid) CPattern()
    {
        var bricks = new string[] { "C", "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["C"], new Vector3Int(1, 0, 1)),

            (t["W"], new Vector3Int(0,0,0)),
            (t["B"], new Vector3Int(1,0,0)),
            (t["B"], new Vector3Int(2,0,0)),
            (t["B"], new Vector3Int(3,0,0)),
            (t["B"], new Vector3Int(4,0,0)),
            (t["B"], new Vector3Int(5,0,0)),
            (t["B"], new Vector3Int(6,0,0)),
            
            (t["B"], new Vector3Int(0,0,1)),
            (t["B"], new Vector3Int(1,0,1)),
            (t["B"], new Vector3Int(6,0,1)),

            (t["B"], new Vector3Int(0,0,2)),
            (t["B"], new Vector3Int(3,0,2)),
            (t["B"], new Vector3Int(4,0,2)),
            (t["B"], new Vector3Int(5,0,2)),
            (t["B"], new Vector3Int(6,0,2)),
            
            (t["B"], new Vector3Int(0,0,3)),
            (t["B"], new Vector3Int(3,0,3)),
            (t["W"], new Vector3Int(4,0,3)),
            (t["W"], new Vector3Int(5,0,3)),
            (t["W"], new Vector3Int(6,0,3)),

            (t["B"], new Vector3Int(0,0,4)),
            (t["B"], new Vector3Int(3,0,4)),
            (t["B"], new Vector3Int(4,0,4)),
            (t["B"], new Vector3Int(5,0,4)),
            (t["B"], new Vector3Int(6,0,4)),

            (t["B"], new Vector3Int(0,0,5)),
            (t["B"], new Vector3Int(1,0,5)),
            (t["B"], new Vector3Int(6,0,5)),

            (t["W"], new Vector3Int(0,0,6)),
            (t["B"], new Vector3Int(1,0,6)),
            (t["B"], new Vector3Int(2,0,6)),
            (t["B"], new Vector3Int(3,0,6)),
            (t["B"], new Vector3Int(4,0,6)),
            (t["B"], new Vector3Int(5,0,6)),
            (t["B"], new Vector3Int(6,0,6)),

        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 7), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles);
       

        return (bricks, grid);
    }
    public (string[] t, SampleGrid) CPatternJoin()
    {
        var bricks = new string[] { "C", "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["C"], new Vector3Int(1, 0, 1)),

            (t["W"], new Vector3Int(0,0,0)),
            (t["W"], new Vector3Int(1,0,0)),
            (t["W"], new Vector3Int(2,0,0)),
            (t["W"], new Vector3Int(3,0,0)),
            (t["W"], new Vector3Int(4,0,0)),
            (t["W"], new Vector3Int(5,0,0)),
            (t["W"], new Vector3Int(6,0,0)),
            
            (t["B"], new Vector3Int(0,0,1)),
            (t["B"], new Vector3Int(1,0,1)),
            (t["B"], new Vector3Int(6,0,1)),

            (t["B"], new Vector3Int(0,0,2)),
            (t["B"], new Vector3Int(3,0,2)),
            (t["B"], new Vector3Int(4,0,2)),
            (t["B"], new Vector3Int(5,0,2)),
            (t["B"], new Vector3Int(6,0,2)),
            
            (t["B"], new Vector3Int(0,0,3)),
            (t["B"], new Vector3Int(3,0,3)),
            (t["W"], new Vector3Int(4,0,3)),
            (t["W"], new Vector3Int(5,0,3)),
            (t["B"], new Vector3Int(6,0,3)),

            (t["B"], new Vector3Int(0,0,4)),
            (t["B"], new Vector3Int(3,0,4)),
            (t["B"], new Vector3Int(4,0,4)),
            (t["B"], new Vector3Int(5,0,4)),
            (t["B"], new Vector3Int(6,0,4)),

            (t["B"], new Vector3Int(0,0,5)),
            (t["B"], new Vector3Int(1,0,5)),
            (t["B"], new Vector3Int(6,0,5)),

            (t["W"], new Vector3Int(0,0,6)),
            (t["W"], new Vector3Int(1,0,6)),
            (t["W"], new Vector3Int(2,0,6)),
            (t["W"], new Vector3Int(3,0,6)),
            (t["W"], new Vector3Int(4,0,6)),
            (t["W"], new Vector3Int(5,0,6)),
            (t["W"], new Vector3Int(6,0,6)),

        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 7), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles);
       

        return (bricks, grid);
    }
    
    public (string[] t, SampleGrid) OPattern()
    {
        var bricks = new string[] { "O", "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["O"], new Vector3Int(1, 0, 1)),

            (t["B"], new Vector3Int(1,0,1)),
            (t["B"], new Vector3Int(5,0,1)),
            (t["B"], new Vector3Int(1,0,5)),
            (t["B"], new Vector3Int(5,0,5)),
            
            (t["W"], new Vector3Int(3,0,2)),
            (t["W"], new Vector3Int(3,0,3)),
            (t["W"], new Vector3Int(3,0,4)),
            

        };
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 1), t["W"]);
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 6, 7), t["W"]);
        LayerAdd(ref stackedPattern, new Range3D(0, 1, 0, 1, 1, 6), t["B"]);
        LayerAdd(ref stackedPattern, new Range3D(6, 7, 0, 1, 1, 6), t["B"]);

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 7), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles);
       

        return (bricks, grid);
    }

    public (string[] t, SampleGrid) GPattern()
    {
        var bricks = new string[] { "G", "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["G"], new Vector3Int(1, 0, 1)),

            (t["W"], new Vector3Int(0,0,0)),
            (t["B"], new Vector3Int(1,0,0)),
            (t["B"], new Vector3Int(2,0,0)),
            (t["B"], new Vector3Int(3,0,0)),
            (t["B"], new Vector3Int(4,0,0)),
            (t["B"], new Vector3Int(5,0,0)),
            (t["W"], new Vector3Int(6,0,0)),

            (t["B"], new Vector3Int(0,0,1)),
            (t["B"], new Vector3Int(1,0,1)),
            (t["W"], new Vector3Int(6,0,1)),

            (t["B"], new Vector3Int(0,0,2)),
            (t["B"], new Vector3Int(3,0,2)),
            (t["B"], new Vector3Int(4,0,2)),
            (t["W"], new Vector3Int(6,0,2)),

            (t["B"], new Vector3Int(0,0,3)),
            (t["B"], new Vector3Int(3,0,3)),
            (t["W"], new Vector3Int(6,0,3)),

            (t["B"], new Vector3Int(0,0,4)),
            (t["B"], new Vector3Int(3,0,4)),
            (t["B"], new Vector3Int(4,0,4)),
            (t["B"], new Vector3Int(5,0,4)),
            (t["W"], new Vector3Int(6,0,4)),

            (t["B"], new Vector3Int(0,0,5)),
            (t["B"], new Vector3Int(1,0,5)),
            (t["W"], new Vector3Int(6,0,5)),

            (t["W"], new Vector3Int(0,0,6)),
            (t["B"], new Vector3Int(1,0,6)),
            (t["B"], new Vector3Int(2,0,6)),
            (t["B"], new Vector3Int(3,0,6)),
            (t["B"], new Vector3Int(4,0,6)),
            (t["B"], new Vector3Int(5,0,6)),
            (t["W"], new Vector3Int(6,0,6)),

        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 7), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles);


        return (bricks, grid);
    }
    
    public (string[] t, SampleGrid) GPatternJoin()
    {
        var bricks = new string[] { "G", "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["G"], new Vector3Int(1, 0, 1)),

            (t["W"], new Vector3Int(0,0,0)),
            (t["W"], new Vector3Int(1,0,0)),
            (t["W"], new Vector3Int(2,0,0)),
            (t["W"], new Vector3Int(3,0,0)),
            (t["W"], new Vector3Int(4,0,0)),
            (t["W"], new Vector3Int(5,0,0)),
            (t["W"], new Vector3Int(6,0,0)),

            (t["B"], new Vector3Int(0,0,1)),
            (t["B"], new Vector3Int(1,0,1)),
            (t["W"], new Vector3Int(6,0,1)),

            (t["B"], new Vector3Int(0,0,2)),
            (t["B"], new Vector3Int(3,0,2)),
            (t["B"], new Vector3Int(4,0,2)),
            (t["W"], new Vector3Int(6,0,2)),

            (t["B"], new Vector3Int(0,0,3)),
            (t["B"], new Vector3Int(3,0,3)),
            (t["W"], new Vector3Int(6,0,3)),

            (t["B"], new Vector3Int(0,0,4)),
            (t["B"], new Vector3Int(3,0,4)),
            (t["B"], new Vector3Int(4,0,4)),
            (t["B"], new Vector3Int(5,0,4)),
            (t["W"], new Vector3Int(6,0,4)),

            (t["B"], new Vector3Int(0,0,5)),
            (t["B"], new Vector3Int(1,0,5)),
            (t["W"], new Vector3Int(6,0,5)),

            (t["W"], new Vector3Int(0,0,6)),
            (t["W"], new Vector3Int(1,0,6)),
            (t["W"], new Vector3Int(2,0,6)),
            (t["W"], new Vector3Int(3,0,6)),
            (t["W"], new Vector3Int(4,0,6)),
            (t["W"], new Vector3Int(5,0,6)),
            (t["W"], new Vector3Int(6,0,6)),
        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 7, 0, 1, 0, 7), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles);


        return (bricks, grid);
    }

    public (string[] t, SampleGrid) BWPattern()
    {
        var bricks = new string[] { "B", "W", "void" };
        var tiles = GetSet().GetSubset(bricks);

        var t = new Dictionary<string, int>();
        foreach (var tile in bricks)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["B"], new Vector3Int(2,0,0)),
            (t["B"], new Vector3Int(2,0,1)),
            (t["B"], new Vector3Int(0,0,2)),
            (t["B"], new Vector3Int(1,0,2)),
            (t["B"], new Vector3Int(2,0,2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(2, 0, 2));
        LayerAdd(ref stackedPattern, new Range3D(1, 5, 0, 1, 1, 2), t["B"]);
        LayerAdd(ref stackedPattern, new Range3D(1, 2, 0, 1, 1, 5), t["B"]);
        LayerAdd(ref stackedPattern, new Range3D(1, 5, 0, 1, 0, 1), t["W"]);
        LayerAdd(ref stackedPattern, new Range3D(1, 5, 0, 1, 5, 6), t["W"]);
        LayerAdd(ref stackedPattern, new Range3D(0, 1, 0, 1, 0, 6), t["W"]);
        LayerAdd(ref stackedPattern, new Range3D(5, 6, 0, 1, 0, 6), t["W"]);

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(0, 1, 0));
        LayerAdd(ref stackedPattern, new Range3D(0, 6, 0, 1, 0, 6), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: false);


        return (bricks, grid);
    }

    public static int BrickUnitSize(bool plateAtoms = true)
    {
        /*
            * A brick is three time as high as a plate.
            */
        return plateAtoms ? 3 : 1;
    }
        
    public (TileSet legoTiles, List<SampleGrid> samples) CogExampleLettersDispersed()
    {
        var cPattern = CPattern();
        var oPattern = OPattern();
        var gPattern = GPattern();
        var bwPattern = BWPattern();
        
        var patterns = new[] { cPattern, oPattern, gPattern, bwPattern };
        return ExtractTilesAndSamples(patterns);
    }


    public (TileSet legoTiles, List<SampleGrid> samples) CogExampleLettersJoined()
    {
        var cPattern = CPatternJoin();
        var oPattern = OPattern();
        var gPattern = GPatternJoin();
        var bwPattern = BWPattern();

        var patterns = new[] { cPattern, oPattern, gPattern, bwPattern };
        return ExtractTilesAndSamples(patterns);
    }

    public static XWFC.Component[] COG()
    {
        var set = new CogSet();

        var (t, s) = set.CogExampleLettersDispersed();
        //var (t, s) = set.CogExampleLettersJoined();

        var weights = new Dictionary<string, float>();
        foreach (var (tKey, value) in t)
        {
            weights[value.UniformAtomValue] = 1;
        }

        weights["C"] = 500;

        var c = new XWFC.Component(
            new Vector3Int(0, 0, 0),
            new Vector3Int(60,2, 40),
            t, s.ToArray(),
            tileWeights: weights,
            customSeed: 1631764118
        /*
         * Other seeds:
         * 569159688 // For many O and some C and G.
         * 1631764118 Trail of O's with C and G. Extent: 60 2 40
         */
        );



        var components = new[] { c }; //  

        return components;
    }


    private (TileSet legoTiles, List<SampleGrid> sampleGrids) ExtractTilesAndSamples((string[] bricks, SampleGrid)[] samples)
    {
        var bricks = new HashSet<string>();
        var sampleGrids = new List<SampleGrid>();
        foreach (var (b, s) in samples)
        {
            foreach (var s1 in b)
            {
                bricks.Add(s1);
            }
            sampleGrids.Add(s);
        }

        var legoTiles = GetSet().GetSubset(bricks);

        return (legoTiles, sampleGrids);
    }


    

    private static SampleGrid ToSampleGrid(Patterns patterns, TileSet tileSet, Vector3Int extent, bool fillWithVoids = true)
    {
        var sampleGrid = new SampleGrid(extent, voidValue: "void");
        if (fillWithVoids) sampleGrid.Populate("void");
        foreach (var (id, c) in patterns)
        {
            sampleGrid.PlaceNut(tileSet[id], c);
        }

        return sampleGrid;
    }
    private static SampleGrid ToSampleGrid(Patterns patterns, TileSet tileSet, bool fillWithVoids = true, Vector3Int? extraLayer = null)
    {
        var extent = new Vector3Int();
        foreach (var (id, c) in patterns)
        {
            var nutMaxCoord = c + tileSet[id].Extent;
            extent = Vector3Util.PairWiseMax(extent, nutMaxCoord);
        }

        if (extraLayer != null)
            extent += (Vector3Int)extraLayer;
        return ToSampleGrid(patterns, tileSet, extent, fillWithVoids);
    }

    private static Patterns TranslatePattern(Patterns patterns, Vector3Int t)
    {
        var list = new Patterns();
        foreach (var (id, pos) in patterns)
        {
            list.Add((id, pos + t));
        }

        return list;
    }

    private static Patterns TranslatePatternPositiveAndFillTranslated(Patterns patterns, Vector3Int t, int id)
    {
        patterns = TranslatePattern(patterns, t);
        for (int x = 0; x < t.x; x++)
        {
            for (int y = 0; y < t.y; y++)
            {
                for (int z = 0; z < t.z; z++)
                {
                    patterns.Add((id, new Vector3Int(x, y, z)));
                }
            }
        }
        return patterns;
    }

    private static void LayerAdd(ref Patterns patterns, Range3D layers, int id)
    {
        for (int x = layers.XRange.Start; x < layers.XRange.End; x++)
        {
            for (int y = layers.YRange.Start; y < layers.YRange.End; y++)
            {
                for (int z = layers.ZRange.Start; z < layers.ZRange.End; z++)
                {
                    patterns.Add((id, new Vector3Int(x, y, z)));
                }
            }
        }
    }
}