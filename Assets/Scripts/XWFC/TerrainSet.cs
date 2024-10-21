using System.Collections.Generic;
using System.Linq;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;
using UnityEngine;
using UnityEngine.Rendering;

namespace XWFC
{
    public class TerrSet : TileSet
    {
        public bool PlateAtoms;
        public TerrSet(bool plateAtoms = true)
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
                    "leaves",
                    new Vector3Int(1,1,1),
                    new Color32(20, 100, 20, 255)
                ),
                new(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color32(0,0,0,0))
                
            };

            var tileSet = new TileSet();
            for (var i = 0; i < tiles.Length; i++)
            {
                tileSet[i] = tiles[i];
            }

            return tileSet;
        }
        
        public (string[] t, SampleGrid) RedDotPatternWFC()
        {
            var bricks = new string[] { "b", "w", "r", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["w"], new Vector3Int(0, 0, 0)),
                (t["w"], new Vector3Int(1, 0, 0)),
                (t["w"], new Vector3Int(2, 0, 0)),
                (t["w"], new Vector3Int(3, 0, 0)),
                (t["w"], new Vector3Int(4, 0, 0)),
                (t["w"], new Vector3Int(5, 0, 0)),
                
                (t["w"], new Vector3Int(0, 1, 0)),
                (t["w"], new Vector3Int(0, 2, 0)),
                (t["w"], new Vector3Int(0, 3, 0)),
                (t["w"], new Vector3Int(0, 4, 0)),
                (t["w"], new Vector3Int(0, 5, 0)),
                
                (t["w"], new Vector3Int(1, 5, 0)),
                (t["w"], new Vector3Int(2, 5, 0)),
                (t["w"], new Vector3Int(3, 5, 0)),
                (t["w"], new Vector3Int(4, 5, 0)),
                (t["w"], new Vector3Int(5, 5, 0)),
                
                (t["w"], new Vector3Int(5, 4, 0)),
                (t["w"], new Vector3Int(5, 3, 0)),
                (t["w"], new Vector3Int(5, 2, 0)),
                (t["w"], new Vector3Int(5, 1, 0)),
                
                (t["b"], new Vector3Int(1, 1, 0)),
                (t["b"], new Vector3Int(2, 1, 0)),
                (t["b"], new Vector3Int(3, 1, 0)),
                (t["b"], new Vector3Int(4, 1, 0)),
                
                (t["b"], new Vector3Int(4, 2, 0)),
                (t["b"], new Vector3Int(4, 3, 0)),
                (t["b"], new Vector3Int(4, 4, 0)),
                
                (t["b"], new Vector3Int(3, 4, 0)),
                (t["b"], new Vector3Int(2, 4, 0)),
                (t["b"], new Vector3Int(1, 4, 0)),
                
                (t["b"], new Vector3Int(1, 3, 0)),
                (t["b"], new Vector3Int(1, 2, 0)),
                
                (t["r"], new Vector3Int(2, 2, 0)),
                (t["r"], new Vector3Int(3, 2, 0)),
                (t["r"], new Vector3Int(2, 3, 0)),
                (t["r"], new Vector3Int(3, 3, 0)),
                
                
            };
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(0,0,1));
            

            return (bricks, grid );
        }
        
        public (string[] t, SampleGrid) RedDotPatternXWFC()
        {
            var bricks = new string[] { "b", "w", "rL", "void" };
            var tiles = GetSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = tiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["w"], new Vector3Int(0, 0, 0)),
                (t["w"], new Vector3Int(1, 0, 0)),
                (t["w"], new Vector3Int(2, 0, 0)),
                (t["w"], new Vector3Int(3, 0, 0)),
                (t["w"], new Vector3Int(4, 0, 0)),
                (t["w"], new Vector3Int(5, 0, 0)),
                
                (t["w"], new Vector3Int(0, 1, 0)),
                (t["w"], new Vector3Int(0, 2, 0)),
                (t["w"], new Vector3Int(0, 3, 0)),
                (t["w"], new Vector3Int(0, 4, 0)),
                (t["w"], new Vector3Int(0, 5, 0)),
                
                (t["w"], new Vector3Int(1, 5, 0)),
                (t["w"], new Vector3Int(2, 5, 0)),
                (t["w"], new Vector3Int(3, 5, 0)),
                (t["w"], new Vector3Int(4, 5, 0)),
                (t["w"], new Vector3Int(5, 5, 0)),
                
                (t["w"], new Vector3Int(5, 4, 0)),
                (t["w"], new Vector3Int(5, 3, 0)),
                (t["w"], new Vector3Int(5, 2, 0)),
                (t["w"], new Vector3Int(5, 1, 0)),
                
                (t["b"], new Vector3Int(1, 1, 0)),
                (t["b"], new Vector3Int(2, 1, 0)),
                (t["b"], new Vector3Int(3, 1, 0)),
                (t["b"], new Vector3Int(4, 1, 0)),
                
                (t["b"], new Vector3Int(4, 2, 0)),
                (t["b"], new Vector3Int(4, 3, 0)),
                (t["b"], new Vector3Int(4, 4, 0)),
                
                (t["b"], new Vector3Int(3, 4, 0)),
                (t["b"], new Vector3Int(2, 4, 0)),
                (t["b"], new Vector3Int(1, 4, 0)),
                
                (t["b"], new Vector3Int(1, 3, 0)),
                (t["b"], new Vector3Int(1, 2, 0)),
                
                (t["rL"], new Vector3Int(2, 2, 0)),
            };
            var grid = ToSampleGrid(stackedPattern, tiles, true, extraLayer: new Vector3Int(0,0,1));
            

            return (bricks, grid );
        }

        public static int BrickUnitSize(bool plateAtoms = true)
        {
            /*
             * A brick is three time as high as a plate.
             */
            return plateAtoms ? 3 : 1;
        }

        public (TileSet legoTiles, List<SampleGrid> samples) RedDotExampleWFC()
        {
            var roof = RedDotPatternWFC();
            var patterns = new[] { roof };
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) RedDotExampleXWFC()
        {
            var roof = RedDotPatternXWFC();
            var patterns = new[] { roof };
            return ExtractTilesAndSamples(patterns);
        }

        public static Component[] RedDotExampleComparison()
        {
            var set = new ExampleSet(false);

            var (t0, s0) = set.RedDotExampleWFC();
            var (t1, s1) = set.RedDotExampleXWFC();
            
            var unit = LegoSet.BrickUnitSize(set.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
            
            var cwfc = new Component(
                new Vector3Int(0,0,0), 
                new Vector3Int(30,30,2), 
                t0, s0.ToArray()
            );

            var cxwfc = new Component(
                new Vector3Int(40, 0, 0),
                new Vector3Int(30, 30, 2),
                t1, s1.ToArray());
            
            var components = new[] { cwfc, cxwfc }; //  

            return components;
        }
        public static Component[] RedDotXWFC()
        {
            var set = new ExampleSet(false);

            var (t, s) = set.RedDotExampleXWFC();
            
            var unit = LegoSet.BrickUnitSize(set.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
            
            var c = new Component(
                new Vector3Int(0,0,0), 
                new Vector3Int(30,30,2), 
                t, s.ToArray(),
                customSeed:26
            );
            
            var components = new[] { c }; //  

            return components;
        }
        
        public static Component[] RedDotWFC()
        {
            var set = new ExampleSet(false);

            var (t, s) = set.RedDotExampleWFC();
            
            var unit = LegoSet.BrickUnitSize(set.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
            
            var c = new Component(
                new Vector3Int(0,0,0), 
                new Vector3Int(30,30,1), 
                t, s.ToArray(),
                customSeed:26
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