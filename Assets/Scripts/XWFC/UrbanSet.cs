using System.Collections.Generic;
using UnityEngine;
using XWFC;
using Component = XWFC.Component;
using Patterns = System.Collections.Generic.List<(int, UnityEngine.Vector3Int)>;

public class UrbanSet : TileSet
{
    public TileSet GetSet()
    {
        var tiles = new NonUniformTile[]
        {
                // Bricks
                new(
                    "road",
                    new Vector3Int(2,1,2),
                    new Color32(60,60,65, 255)
                ),
                new(
                    "root",
                    new Vector3Int(1,1,1),
                    new Color32(50, 150, 50, 255)
                ),
                new(
                    "bsNE",
                    new Vector3Int(2,3,2),
                    new Color32(185,74,62, 255),
                    mask: new bool[3,2,2] {
                        { { false, true }, { true, true } },
                        { { false, true }, { true, true } },
                        { { false, false},{ false, true } },
                        }
                ),
                new(
                    "bsSE",
                    new Vector3Int(2,3,2),
                    //new Color32(175,164,62, 255),
                    new Color32(175,64,62, 255),
                    mask: new bool[3,2,2] {
                        { { true, false }, {true, true } },
                        { { true, false }, {true, true } },
                        { { false, false},{true, false } },
                        }
                ),
                new(
                    "bsSW",
                    new Vector3Int(2,3,2),
                    new Color32(195,84,62, 255),
                    //new Color32(195,84,162, 255),
                    mask: new bool[3,2,2] {
                        { { true, true }, { true, false } },
                        { { true, true }, { true, false } },
                        { { true, false},{ false, false } },
                        }
                ),
                new(
                    "bsNW",
                    new Vector3Int(2,3,2),
                    //new Color32(65,54,62, 255),
                    new Color32(165,54,62, 255),
                    mask: new bool[3,2,2] {
                            { { true, true }, { false, true } },
                            { { true, true }, { false, true } }, 
                            { { false, true},{ false, false } }, 
                    }
                ),
                new(
                    "bmZ",
                    new Vector3Int(1,4,4),
                    new Color32(180,180,180, 255)
                ),
                new(
                    "bmX",
                    new Vector3Int(2,2,1),
                    new Color32(50,100,200, 150)
                ),
                new(
                    "smallWindowX",
                    new Vector3Int(2,1,1),
                    new Color32(50,100,200, 150)
                ),
                new(
                    "smallWindowZ",
                    new Vector3Int(1,1,2),
                    new Color32(50,100,200, 150)
                ),
                new(
                    "layerR",
                    new Vector3Int(5,3,2),
                    new Color32(100,100,150, 255),
                    mask: new bool[3,5,2]
                    {
                        { { true, true }, { true , true}, { true , true}, { true , true}, { false, false } },
                        { { false, false }, { true , true}, { true , true}, { true , true}, { true , true} },
                        { { false, false }, { true , true}, { true , true}, { true , true}, { true , true} },
                    }
                ),
                new(
                    "layerL",
                    new Vector3Int(5,3,2),
                    new Color32(150,100,100, 255),
                    mask: new bool[3,5,2]
                    {
                        { { false, false }, { true, true }, { true, true }, { true, true }, { true, true } },
                        { { true, true }, { true, true }, { true, true }, { true, true }, { false, false } },
                        { { true, true }, { true, true }, { true, true }, { true, true }, { false, false } },
                    }
                ),
                new(
                    "roof",
                    new Vector3Int(2,1,2),
                    new Color32(20,20,20, 255)
                ),
                new(
                    "top",
                    new Vector3Int(2,1,2),
                    new Color32(40,40,40, 255)
                ),
                new(
                    "antenna",
                    new Vector3Int(1,2,1),
                    new Color32(80,80,80, 180)
                ),
                new(
                    "connector",
                    new Vector3Int(2,1,2),
                    new Color32(80,80,80, 255)
                ),
                new(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color32(0,0,0,0)
                ),
        };

        var tileSet = new TileSet();
        for (var i = 0; i < tiles.Length; i++)
        {
            tileSet[i] = tiles[i];
        }

        return tileSet;
    }

    public (string[] t, SampleGrid) RootSelfPattern()
    {
        var nuts = new string[] { "root", "void", "bsNE", "bsSE", "bsSW", "bsNW" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns();
        LayerAdd(ref stackedPattern, new Range3D(0, 2, 0, 1, 0, 2), t["root"]);
        LayerAdd(ref stackedPattern, new Range3D(0, 2, 1, 2, 0, 2), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles, false);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) RoadRootPattern()
    {
        var nuts = new string[] { "road", "root", "void" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
            {
                (t["road"], new Vector3Int(0, 0, 4)),
                (t["road"], new Vector3Int(2, 0, 4)),
                (t["road"], new Vector3Int(4, 0, 4)),
                (t["road"], new Vector3Int(6, 0, 4)),
                (t["road"], new Vector3Int(8, 0, 4)),
                (t["road"], new Vector3Int(4, 0, 0)),
                (t["road"], new Vector3Int(4, 0, 2)),
                (t["road"], new Vector3Int(4, 0, 6)),
                (t["road"], new Vector3Int(4, 0, 8)),

                (t["root"], new Vector3Int(0, 0, 3)),
                (t["root"], new Vector3Int(1, 0, 3)),
                (t["root"], new Vector3Int(2, 0, 3)),
                (t["root"], new Vector3Int(3, 0, 3)),

                (t["root"], new Vector3Int(6, 0, 3)),
                (t["root"], new Vector3Int(7, 0, 3)),
                (t["root"], new Vector3Int(8, 0, 3)),
                (t["root"], new Vector3Int(9, 0, 3)),

                (t["root"], new Vector3Int(0, 0, 6)),
                (t["root"], new Vector3Int(1, 0, 6)),
                (t["root"], new Vector3Int(2, 0, 6)),
                (t["root"], new Vector3Int(3, 0, 6)),

                (t["root"], new Vector3Int(6, 0, 6)),
                (t["root"], new Vector3Int(7, 0, 6)),
                (t["root"], new Vector3Int(8, 0, 6)),
                (t["root"], new Vector3Int(9, 0, 6)),

                (t["root"], new Vector3Int(3, 0, 0)),
                (t["root"], new Vector3Int(3, 0, 1)),
                (t["root"], new Vector3Int(3, 0, 2)),

                (t["root"], new Vector3Int(3, 0, 7)),
                (t["root"], new Vector3Int(3, 0, 8)),
                (t["root"], new Vector3Int(3, 0, 9)),

                (t["root"], new Vector3Int(6, 0, 0)),
                (t["root"], new Vector3Int(6, 0, 1)),
                (t["root"], new Vector3Int(6, 0, 2)),

                (t["root"], new Vector3Int(6, 0, 7)),
                (t["root"], new Vector3Int(6, 0, 8)),
                (t["root"], new Vector3Int(6, 0, 9)),

            };

        LayerAdd(ref stackedPattern, new Range3D(0, 10, 1, 2, 0, 10), t["void"]);
        var grid = ToSampleGrid(stackedPattern, tiles, false);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingShortPattern()
    {
        var nuts = new string[] { "road", "root", "void", "bsNE", "bsSE", "bsSW", "bsNW", "smallWindowX", "smallWindowZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bsSW"], new Vector3Int(0, 0, 0)),
            (t["bsSW"], new Vector3Int(0, 3, 0)),
            (t["bsNW"], new Vector3Int(0, 0, 2)),
            (t["bsNW"], new Vector3Int(0, 3, 2)),
            (t["bsNE"], new Vector3Int(2, 0, 2)),
            (t["bsNE"], new Vector3Int(2, 3, 2)),
            (t["bsSE"], new Vector3Int(2, 3, 0)),
            (t["bsSE"], new Vector3Int(2, 0, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 3)),
            (t["smallWindowZ"], new Vector3Int(0, 2, 1)),
            (t["smallWindowZ"], new Vector3Int(3, 2, 1)),
        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(2, 1, 2));
        LayerAdd(ref stackedPattern, new Range3D(2, 6, 0, 1, 2, 6), t["root"]);

        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 0, 2), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 6, 8), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 2, 6), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(6, 8, 0, 1, 2, 6), t["road"], tiles);

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingShortCrossPattern()
    {
        var nuts = new string[] { "road", "root", "void", "bsNE", "bsSE", "bsSW", "bsNW", "smallWindowX", "smallWindowZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bsNE"], new Vector3Int(0, 0, 0)),
            (t["bsNE"], new Vector3Int(0, 3, 0)),
            (t["bsSE"], new Vector3Int(0, 0, 2)),
            (t["bsSE"], new Vector3Int(0, 3, 2)),
            (t["bsSW"], new Vector3Int(2, 0, 2)),
            (t["bsSW"], new Vector3Int(2, 3, 2)),
            (t["bsNW"], new Vector3Int(2, 3, 0)),
            (t["bsNW"], new Vector3Int(2, 0, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 3)),
            (t["smallWindowZ"], new Vector3Int(0, 2, 1)),
            (t["smallWindowZ"], new Vector3Int(3, 2, 1)),
        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(2, 1, 2));
        LayerAdd(ref stackedPattern, new Range3D(2, 6, 0, 1, 2, 6), t["root"]);

        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 0, 2), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 6, 8), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 2, 6), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(6, 8, 0, 1, 2, 6), t["road"], tiles);

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }
    public (string[] t, SampleGrid) BuildingShortRoofPattern()
    {
        var nuts = new string[] { "void", "bsNE", "bsSE", "bsSW", "bsNW", "roof", "smallWindowX", "smallWindowZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bsSW"], new Vector3Int(0, 0, 0)),
            (t["bsNW"], new Vector3Int(0, 0, 2)),
            (t["bsNE"], new Vector3Int(2, 0, 2)),
            (t["bsSE"], new Vector3Int(2, 0, 0)),
            (t["roof"], new Vector3Int(0, 3, 0)),
            (t["roof"], new Vector3Int(0, 3, 2)),
            (t["roof"], new Vector3Int(2, 3, 0)),
            (t["roof"], new Vector3Int(2, 3, 2)),
            (t["smallWindowX"], new Vector3Int(1, 2, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 3)),
            (t["smallWindowZ"], new Vector3Int(0, 2, 1)),
            (t["smallWindowZ"], new Vector3Int(3, 2, 1)),
        };
        LayerAdd(ref stackedPattern, new Range3D(0, 4, 4, 5, 0, 4), t["void"]);
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingShortCrossRoofPattern()
    {
        var nuts = new string[] { "void", "bsNE", "bsSE", "bsSW", "bsNW", "roof", "smallWindowX", "smallWindowZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bsNE"], new Vector3Int(0, 0, 0)),
            (t["bsSE"], new Vector3Int(0, 0, 2)),
            (t["bsSW"], new Vector3Int(2, 0, 2)),
            (t["bsNW"], new Vector3Int(2, 0, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 0)),
            (t["smallWindowX"], new Vector3Int(1, 2, 3)),
            (t["smallWindowZ"], new Vector3Int(0, 2, 1)),
            (t["smallWindowZ"], new Vector3Int(3, 2, 1)),
            (t["roof"], new Vector3Int(1, 3, 1)),
        };
        LayerAdd(ref stackedPattern, new Range3D(0, 4, 4, 5, 0, 4), t["void"]);
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) RoofTopAntennaPattern()
    {
        var nuts = new string[] { "void", "roof", "top", "antenna" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["roof"], new Vector3Int(0, 0, 0)),
            (t["roof"], new Vector3Int(0, 0, 2)),
            (t["roof"], new Vector3Int(2, 0, 0)),
            (t["roof"], new Vector3Int(2, 0, 2)),
            (t["top"], new Vector3Int(1, 1, 1)),
            (t["antenna"], new Vector3Int(1, 2, 1)),
            (t["antenna"], new Vector3Int(1, 4, 1)),
        };

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, new Vector3Int(0, 1, 0));

        return (nuts, grid);
    }
    public (string[] t, SampleGrid) RoofVoidPattern()
    {
        var nuts = new string[] { "void", "roof", "top" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["roof"], new Vector3Int(0, 0, 0)),
            (t["roof"], new Vector3Int(0, 0, 2)),
            (t["roof"], new Vector3Int(2, 0, 0)),
            (t["roof"], new Vector3Int(2, 0, 2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, new Vector3Int(1, 1, 1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingMediumPattern()
    {
        var nuts = new string[] { "road", "root", "void", "smallWindowX", "bmZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bmZ"], new Vector3Int(0, 0, 0)),
            (t["bmZ"], new Vector3Int(0, 4, 0)),
            (t["bmZ"], new Vector3Int(5, 0, 0)),
            (t["bmZ"], new Vector3Int(5, 4, 0)),
        };

        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 0, 8, 0, 1), t["smallWindowX"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 0, 8, 3, 4), t["smallWindowX"], tiles);

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(2, 1, 2));
        LayerAdd(ref stackedPattern, new Range3D(2, 8, 0, 1, 2, 6), t["root"]);

        LayerAddNut(ref stackedPattern, new Range3D(0, 10, 0, 1, 0, 2), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 10, 0, 1, 6, 8), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 2, 6), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(8, 10, 0, 1, 2, 6), t["road"], tiles);

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingMediumConnectionPattern()
    {
        var nuts = new string[] { "root", "void", "connector", "bmZ", "bsNE", "bsSE", "smallWindowZ" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bmZ"], new Vector3Int(0, 0, 0)),
            (t["bmZ"], new Vector3Int(3, 0, 0)),
            (t["connector"], new Vector3Int(1, 2, 1)),
            (t["bsNE"], new Vector3Int(6, 1, 0)),
            (t["bsSE"], new Vector3Int(6, 1, 2)),
            (t["smallWindowZ"], new Vector3Int(6, 3, 1)),
            (t["connector"], new Vector3Int(4, 2, 1)),
        };

        LayerAddNut(ref stackedPattern, new Range3D(6, 10, 0, 1, 0, 4), t["root"], tiles);
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }



    public (string[] t, SampleGrid) BuildingZigZagPatternR()
    {
        var nuts = new string[] { "layerR" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["layerR"], new Vector3Int(0, 0, 0)),
            (t["layerR"], new Vector3Int(2, 3, 0)),
            (t["layerR"], new Vector3Int(0, 0, 1)),
            (t["layerR"], new Vector3Int(2, 3, 2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1,0,1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingZigZagPatternL()
    {
        var nuts = new string[] { "layerL" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["layerL"], new Vector3Int(2, 0, 0)),
            (t["layerL"], new Vector3Int(0, 3, 0)),
            (t["layerL"], new Vector3Int(2, 0, 2)),
            (t["layerL"], new Vector3Int(0, 3, 2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingZigZagPatternRL()
    {
        var nuts = new string[] { "layerL", "layerR" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["layerR"], new Vector3Int(0, 0, 0)),
            (t["layerL"], new Vector3Int(0, 3, 0)),
            (t["layerR"], new Vector3Int(0, 6, 0)),
            (t["layerR"], new Vector3Int(0, 0, 2)),
            (t["layerL"], new Vector3Int(0, 3, 2)),
            (t["layerR"], new Vector3Int(0, 6, 2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }
    public (string[] t, SampleGrid) BuildingZigZagPatternRRoof()
    {
        var nuts = new string[] { "layerR", "roof" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["layerR"], new Vector3Int(0, 0, 0)),
            (t["layerR"], new Vector3Int(0, 0, 2)),
            (t["roof"], new Vector3Int(1, 3, 0)),
            (t["roof"], new Vector3Int(3, 3, 0)),
            (t["roof"], new Vector3Int(1, 3, 2)),
            (t["roof"], new Vector3Int(3, 3, 2)),
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingZigZagPatternRPillar()
    {
        var nuts = new string[] { "layerR", "pillar" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["layerR"], new Vector3Int(0, 3, 0)),
            (t["layerR"], new Vector3Int(0, 3, 2)),
            (t["pillar"], new Vector3Int(0, 0, 0)),
            (t["pillar"], new Vector3Int(3, 0, 0)),
            (t["pillar"], new Vector3Int(0, 0, 3)),
            (t["pillar"], new Vector3Int(3, 0, 3)),
            
        };

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1, 0, 1));

        return (nuts, grid);
    }
    public (string[] t, SampleGrid) BuildingPillarRootRoadPattern()
    {
        var nuts = new string[] { "root", "road", "pillar" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["pillar"], new Vector3Int(0, 1, 0)),
            (t["pillar"], new Vector3Int(3, 1, 0)),
            (t["pillar"], new Vector3Int(0, 1, 3)),
            (t["pillar"], new Vector3Int(3, 1, 3)),
        };

        LayerAdd(ref stackedPattern, new Range3D(0, 4, 0, 1, 0, 4), t["root"]);

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(2, 0, 2));

        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 0, 2), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 8, 0, 1, 6, 8), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 2, 6), t["road"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(6, 8, 0, 1, 2, 6), t["road"], tiles);

        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingPillarRootRoadCrossPattern()
    {
        var nuts = new string[] { "root", "void", "road", "pillar" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["pillar"], new Vector3Int(0, 1, 0)),
            (t["pillar"], new Vector3Int(3, 1, 0)),
            (t["pillar"], new Vector3Int(0, 1, 3)),
            (t["pillar"], new Vector3Int(3, 1, 3)),
        };
        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 0, 2), t["root"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 2, 0, 1, 4, 6), t["root"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(4, 6, 0, 1, 0, 2), t["root"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(4, 6, 0, 1, 4, 6), t["root"], tiles);

        LayerAddNut(ref stackedPattern, new Range3D(0, 6, 0, 1, 2, 4), t["road"], tiles);

        stackedPattern.Add((t["road"], new Vector3Int(2, 0, 0)));
        stackedPattern.Add((t["road"], new Vector3Int(2, 0, 4)));

        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 1, 2, 2, 4), t["void"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(2, 4, 1, 2, 1, 2), t["void"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(2, 4, 1, 2, 4, 5), t["void"], tiles);
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true);

        return (nuts, grid);
    }

    public (string[] t, SampleGrid) BuildingMediumRoofPattern()
    {
        var nuts = new string[] { "void", "smallWindowX", "bmZ", "roof" };
        var tiles = GetSet().GetSubset(nuts);

        var t = new Dictionary<string, int>();
        foreach (var tile in nuts)
        {
            t[tile] = tiles.GetTileIdFromValue(tile);
        }

        var stackedPattern = new Patterns()
        {
            (t["bmZ"], new Vector3Int(0, 0, 0)),
            (t["bmZ"], new Vector3Int(5, 0, 0)),
        };

        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 0, 4, 0, 1), t["smallWindowX"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 0, 4, 3, 4), t["smallWindowX"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(0, 6, 4, 5, 0, 4), t["roof"], tiles);
        LayerAddNut(ref stackedPattern, new Range3D(1, 5, 3, 4, 1, 3), t["void"], tiles);

        stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
        var grid = ToSampleGrid(stackedPattern, tiles, fillWithVoids: true, extraLayer: new Vector3Int(1, 1, 1));

        return (nuts, grid);
    }

    public static int BrickUnitSize(bool plateAtoms = true)
    {
        /*
         * A brick is three time as high as a plate.
         */
        return plateAtoms ? 3 : 1;
    }

    public (TileSet legoTiles, List<SampleGrid> samples) UrbanGroundExample()
    {
        var root = RootSelfPattern();
        var roadRoot = RoadRootPattern();
        var buildingShort = BuildingShortPattern();
        var buildingShortRoof = BuildingShortRoofPattern();
        var buildingShortCross = BuildingShortCrossPattern();
        var buildingShortCrossRoof = BuildingShortCrossRoofPattern();
        var roofTopAntenna = RoofTopAntennaPattern();
        var roofVoid = RoofVoidPattern();
        var buildingMedium = BuildingMediumPattern();
        var buildingMediumRoof = BuildingMediumRoofPattern();
        var connector = BuildingMediumConnectionPattern();


        var patterns = new[] {
            root,
            roadRoot,
            roofVoid,
            buildingShort,
            buildingShortRoof,
            buildingShortCross,
            buildingShortCrossRoof,
            roofTopAntenna,
            buildingMedium,
            buildingMediumRoof,
            connector,
        };
        return ExtractTilesAndSamples(patterns);
    }

    public (TileSet legoTiles, List<SampleGrid> samples) UrbanGroundExampleGood801516()
    {
        var root = RootSelfPattern();
        var roadRoot = RoadRootPattern();
        var buildingShort = BuildingShortPattern();
        var buildingRoof = BuildingShortRoofPattern();
        var roofTopAntenna = RoofTopAntennaPattern();
        var roofVoid = RoofVoidPattern();
        var buildingMedium = BuildingMediumPattern();
        var buildingMediumRoof = BuildingMediumRoofPattern();
        var buildingZigZagR = BuildingZigZagPatternR();
        var buildingZigZagL = BuildingZigZagPatternL();
        var buildingZigZagRL = BuildingZigZagPatternRL();
        var buildingZigZagPillars = BuildingZigZagPatternRPillar();
        var buildingZigZagRRoof = BuildingZigZagPatternRRoof();
        var buildingPillarRoot = BuildingPillarRootRoadPattern();
        var buildingPillarCross = BuildingPillarRootRoadCrossPattern();

        var patterns = new[] {
            root,
            roadRoot,
            roofVoid,
            buildingShort,
            buildingRoof,
            roofTopAntenna,
            buildingMedium,
            buildingMediumRoof,
            //buildingZigZagR,
            //buildingZigZagL,
            //buildingZigZagRL,
            //buildingZigZagPillars,
            //buildingZigZagRRoof,
            //buildingPillarRoot,
            //buildingPillarCross,
        };

        // seed: 736404273. 80w 15h 16d
        return ExtractTilesAndSamples(patterns);
    }

    public static Component[] UrbanGround()
    {
        var set = new UrbanSet();

        var (t, s) = set.UrbanGroundExample();

        var weights = new Dictionary<string, float>();
        foreach (var (tKey, value) in t)
        {
            weights[value.UniformAtomValue] = 1;
        }

        var c = new Component(
            new Vector3Int(0, 0, 0),
            new Vector3Int(20, 2, 20),
            t, s.ToArray(),
            tileWeights: weights,
            customSeed: 736377411
        );

        c.WithManualAtomSeeding(
            new List<(int tileId, Vector3Int atomCoord, Vector3Int gridCoord)>
            {
                (t.GetTileIdFromValue("road"),new Vector3Int(0,0,0), new Vector3Int(2,0,2)),
                //(t.GetTileIdFromValue("road"),new Vector3Int(0,0,0), new Vector3Int(16,0,14)),
            });

        var components = new[] { c };


        return components;
    }
    public static Component[] UrbanGroundGreat()
    {
        var set = new UrbanSet();

        var (t, s) = set.UrbanGroundExample();

        var weights = new Dictionary<string, float>();
        foreach (var (tKey, value) in t)
        {
            weights[value.UniformAtomValue] = 1;
        }

        var c = new Component(
            new Vector3Int(0, 0, 0),
            new Vector3Int(20, 2, 20),
            t, s.ToArray(),
            tileWeights: weights,
            customSeed: 736377411
        );

        c.WithManualAtomSeeding(
            new List<(int tileId, Vector3Int atomCoord, Vector3Int gridCoord)>
            {
                (t.GetTileIdFromValue("road"),new Vector3Int(0,0,0), new Vector3Int(2,0,2)),
                (t.GetTileIdFromValue("road"),new Vector3Int(0,0,0), new Vector3Int(16,0,14)),
            });

        var components = new[] { c };


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


    private static void LayerAddNut(ref Patterns patterns, Range3D layers, int id, TileSet tiles)
    {
        var extent = tiles[id].Extent;
        for (int x = layers.XRange.Start; x < layers.XRange.End; x += extent.x)
        {
            for (int y = layers.YRange.Start; y < layers.YRange.End; y += extent.y)
            {
                for (int z = layers.ZRange.Start; z < layers.ZRange.End; z += extent.z)
                {
                    patterns.Add((id, new Vector3Int(x, y, z)));
                }
            }
        }
    }
}