using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;
using UnityEngine;
using UnityEngine.Rendering;

namespace XWFC
{
    public class TerrainSet : TileSet
    {
        public bool PlateAtoms;
        public TerrainSet(bool plateAtoms = true)
        {
            PlateAtoms = plateAtoms;
        }
        public TileSet GetSet()
        {
            var unit = BrickUnitSize(PlateAtoms);

            var tiles = new NonUniformTile[]
            {
                // Bricks
                new(
                    "soil",
                    new Vector3Int(1,1,1),
                    new Color32(40, 20, 0, 255)
                ),
                new(
                    "grass",
                    new Vector3Int(1,1,1),
                    new Color32(50, 150, 50, 255)
                ),
                new(
                    "root",
                    new Vector3Int(1,1,1),
                    new Color32(120, 60, 10, 255)
                ),
                new(
                    "trunk3",
                    new Vector3Int(1,3,1),
                    new Color32(150, 75, 10, 255)
                ),
                    
                new(
                    "trunk",
                    new Vector3Int(1,1,1),
                    new Color32(150, 75, 10, 255)
                ),
                new(
                    "leaf",
                    new Vector3Int(1,1,1),
                    new Color32(20, 100, 20, 200)
                ),
                new(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color32(0,0,0,0)
                    ),
                new(
                    "rockBase",
                    new Vector3Int(1,1,1),
                    new Color32(100,100,100, 255)),
                new(
                    "water",
                    new Vector3Int(2,1,2),
                    new Color32(70,70,200, 255))
                
            };

            var tileSet = new TileSet();
            for (var i = 0; i < tiles.Length; i++)
            {
                tileSet[i] = tiles[i];
            }

            return tileSet;
        }
        
        public (string[] t, SampleGrid) TreeTrunkPattern()
        {
            var bricks = new string[] { "trunk3", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["trunk3"], new Vector3Int(1, 0, 1)),
                (t["trunk3"], new Vector3Int(1, 3, 1)),
            };
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(1,0,1));
            
            return (bricks, grid );
        }
        
        
        public (string[] t, SampleGrid) TreeTrunkLeavesPattern()
        {
            var bricks = new string[] { "trunk3", "leaf", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["trunk3"], new Vector3Int(1, 0, 1)),
                (t["leaf"], new Vector3Int(1, 3, 1)),
            };
            LayerAdd(ref stackedPattern, new Range3D(0,3,1,4,0,1), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(0,3,1,4,2,3), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(0,1,1,4,1,2), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(2,3,1,4,1,2), t["leaf"]);
            stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 0, 1));
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(1,1,1));
            
            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) LeavesPattern()
        {
            var bricks = new string[] { "leaf", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns();
            LayerAdd(ref stackedPattern, new Range3D(1,3,0,1,1,3), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(0,4,1,3,1,3), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(1,3,3,4,1,3), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(1,3,1,3,0,1), t["leaf"]);
            LayerAdd(ref stackedPattern, new Range3D(1,3,1,3,3,4), t["leaf"]);
            stackedPattern = TranslatePattern(stackedPattern, new Vector3Int(1, 1, 1));
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(1,1,1));
            
            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) WaterPattern()
        {
            var bricks = new string[] { "water", "grass", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["water"], new Vector3Int(1, 0, 3)),
                (t["water"], new Vector3Int(3, 0, 2)),
                (t["water"], new Vector3Int(3, 0, 4)),
                (t["water"], new Vector3Int(5, 0, 1)),
                (t["water"], new Vector3Int(5, 0, 3)),
                (t["water"], new Vector3Int(7, 0, 2)),
            };

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    var vector = new Vector3Int(i, 0, j);
                    var free = true;
                    foreach (var (id, vec) in stackedPattern)
                    {

                        if (Vector3Util.Lt(vector, vec + tiles[id].Extent) && Vector3Util.Geq(vector, vec))
                        {
                            free = false;
                            break;
                        }
                        
                    }
                    if (free) stackedPattern.Add((t["grass"], new Vector3Int(i,0,j)));
                }
            }
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(0,1,0));
            
            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) WaterEvenPattern()
        {
            var bricks = new string[] { "water", "grass", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["water"], new Vector3Int(1, 0, 3)),
                (t["water"], new Vector3Int(3, 0, 3)),
                (t["water"], new Vector3Int(5, 0, 3)),
                (t["water"], new Vector3Int(3, 0, 1)),
                (t["water"], new Vector3Int(3, 0, 5)),
            };

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var vector = new Vector3Int(i, 0, j);
                    var free = true;
                    foreach (var (id, vec) in stackedPattern)
                    {

                        if (Vector3Util.Lt(vector, vec + tiles[id].Extent) && Vector3Util.Geq(vector, vec))
                        {
                            free = false;
                            break;
                        }
                        
                    }
                    if (free) stackedPattern.Add((t["grass"], new Vector3Int(i,0,j)));
                }
            }
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(0,1,0));
            
            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) WaterSquarePattern()
        {
            var bricks = new string[] { "water", "grass", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["water"], new Vector3Int(1, 0, 1)),
                (t["water"], new Vector3Int(3, 0, 1)),
                (t["water"], new Vector3Int(1, 0, 3)),
                (t["water"], new Vector3Int(3, 0, 3)),
            };

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    var vector = new Vector3Int(i, 0, j);
                    var free = true;
                    foreach (var (id, vec) in stackedPattern)
                    {

                        if (Vector3Util.Lt(vector, vec + tiles[id].Extent) && Vector3Util.Geq(vector, vec))
                        {
                            free = false;
                            break;
                        }
                        
                    }
                    if (free) stackedPattern.Add((t["grass"], new Vector3Int(i,0,j)));
                }
            }
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(0,1,0));
            
            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) RootPattern()
        {
            var bricks = new string[] { "grass", "root", "trunk3", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["root"], new Vector3Int(1,0,1)),
                (t["trunk3"], new Vector3Int(1,1,1)),
                
                (t["grass"], new Vector3Int(0,0,0)),
                (t["grass"], new Vector3Int(1,0,0)),
                (t["grass"], new Vector3Int(2,0,0)),
                (t["grass"], new Vector3Int(0,0,1)),
                (t["grass"], new Vector3Int(2,0,1)),
                (t["grass"], new Vector3Int(0,0,2)),
                (t["grass"], new Vector3Int(1,0,2)),
                (t["grass"], new Vector3Int(2,0,2)),
                (t["grass"], new Vector3Int(0,0,3)),
                (t["grass"], new Vector3Int(1,0,3)),
                (t["grass"], new Vector3Int(2,0,3)),
                
            };
            
            var grid = ToSampleGrid(stackedPattern, tiles, true);
            
            return (bricks, grid );
        }
        
        
        
        public static int BrickUnitSize(bool plateAtoms = true)
        {
            /*
             * A brick is three time as high as a plate.
             */
            return plateAtoms ? 3 : 1;
        }

        public (TileSet legoTiles, List<SampleGrid> samples) TreeExample()
        {
            var trunk = TreeTrunkPattern();
            var trunkLeaves = TreeTrunkLeavesPattern();
            var leaves = LeavesPattern();
            var root = RootPattern();
            var water = WaterPattern();
            var water2 = WaterEvenPattern();
            var water3 = WaterSquarePattern();
            var patterns = new[] { trunk, root, trunkLeaves, leaves, water, water2, water3};
            return ExtractTilesAndSamples(patterns);
        }
        
        public static Component[] Tree()
        {
            var set = new TerrainSet(false);

            var (t, s) = set.TreeExample();

            var weights = new Dictionary<string, float>();
            foreach (var (tKey, value) in t)
            {
                weights[value.UniformAtomValue] = 1;
            }

            weights["grass"] = 300;
            weights["root"] = 200;
            weights["trunk3"] = 200;
            weights["water"] = 400;
            weights["leaf"] = 0.01f;
            
            var unit = LegoSet.BrickUnitSize(set.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
            
            var c = new Component(
                new Vector3Int(0,0,0), 
                new Vector3Int(40,8,20), 
                t, s.ToArray(),
                tileWeights:weights,
                customSeed:754
            );
            
            
            
            var components = new[] { c }; //  

            return components;
        }


        private (TileSet legoTiles, List<SampleGrid> sampleGrids) ExtractTilesAndSamples((string[] bricks, SampleGrid)[] samples)
        {
            var bricks = new HashSet<string>();
            var sampleGrids = new List<SampleGrid>(); 
            foreach (var (b,s) in samples)
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


        private (int unit, TileSet legoTiles, Dictionary<string, int> t) PatternData(string[] bricks)
        {
            var unit = BrickUnitSize(PlateAtoms);
            var legoTiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            return (unit, legoTiles, t);
        }

        private static SampleGrid ToSampleGrid(Patterns patterns, TileSet tileSet, Vector3Int extent, bool fillWithVoids = true)
        {
            var sampleGrid = new SampleGrid(extent, voidValue:"void");
            if (fillWithVoids) sampleGrid.Populate("void");
            foreach (var (id, c) in patterns)
            {
                sampleGrid.PlaceNut(tileSet[id], c);
            }

            return sampleGrid;
        }
        private static SampleGrid ToSampleGrid(Patterns patterns, TileSet tileSet, bool fillWithVoids=true, Vector3Int? extraLayer=null)
        {
            var extent = new Vector3Int();
            foreach (var (id, c) in patterns)
            {
                var nutMaxCoord = c + tileSet[id].Extent;
                extent = Vector3Util.PairWiseMax(extent, nutMaxCoord);
            }

            if (extraLayer != null)
                extent += (Vector3Int) extraLayer;
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
                        patterns.Add((id, new Vector3Int(x,y,z)));
                    }
                }
            }
        }
    }
}