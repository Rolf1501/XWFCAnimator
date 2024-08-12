using System.Collections.Generic;
using UnityEngine;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;

namespace XWFC
{
    public class TetrisSet
    {
        private bool _plateUnit;
        public TetrisSet(bool plateUnit=false)
        {
            _plateUnit = plateUnit;
        }

        public int GetUnitSize()
        {
            // return _plateUnit ? 3 : 2;
            return 1;
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

        private (TileSet tiles, List<SampleGrid> sampleGrids) ExtractTilesAndSamples(string[] bricks, SampleGrid sample, TileSet set)
        {
            var sampleGrids = new List<SampleGrid> { sample };

            var tiles = set.GetSubset(bricks);

            return (tiles, sampleGrids);
        }

        public Dictionary<string, int> PatternData(TileSet set, string[] bricks)
        {
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = set.GetTileIdFromValue(tile);
            }

            return t;
        }
        
        public (TileSet tiles, List<SampleGrid> samples) Example()
        {
            // var (bricks, sample, set) = SmallSample();
            var (bricks, sample, set) = LargeSample();
            
            return ExtractTilesAndSamples(bricks, sample, set);
        }

        public (TileSet tiles, List<SampleGrid> samples) FillExample()
        {
            var (bricks, sample, set) = FillSample();
            return ExtractTilesAndSamples(bricks, sample, set);
        }

        public (string[] bricks, SampleGrid sample, TileSet) FillSample()
        {
            // var bricks = new string[] { "o" };
            var bricks = new string[] { "O", "o" };
            var fullSet = new TileSet();
            // var fullSet = GetTetrisTileSet(false);
            fullSet.Add(fullSet.Count, new NonUniformTile(uniformAtomValue:"o", new Vector3Int(1,2,1), new Color(1,1,1)));
            fullSet.Add(fullSet.Count, new NonUniformTile(uniformAtomValue:"O", new Vector3Int(2,2,2), new Color(0.3f,0.3f,0.3f)));
            var t = PatternData(fullSet, bricks);
            
            // t.Add("o", fullSet.Count-1);
            var set = fullSet.GetSubset(bricks);
            

            var sample = new SampleGrid(new Vector3(12,2,12));
            sample.Populate("o");
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(1,0,1), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(1,0,5), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(1,0,7), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(4,0,2), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(6,0,2), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(4,0,4), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(6,0,4), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(4,0,7), true);
            sample.PlaceNut(fullSet[t["O"]], new Vector3Int(6,0,7), true);
            return (bricks, sample, set);
        }

        public (string[] bricks, SampleGrid sample, TileSet) LargeSample()
        {
            var bricks = new string[] { "LL", "OL", "JL", "void" };
            var fullSet = GetLargeTetrisSet(true);
            var unit = GetUnitSize();
            var t = PatternData(fullSet, bricks);
            var set = fullSet.GetSubset(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["OL"], new Vector3Int(0,0,4)),
                (t["OL"], new Vector3Int(0,1,4)),
                (t["OL"], new Vector3Int(4,0,4)),
                (t["OL"], new Vector3Int(4,1,4)),
                (t["OL"], new Vector3Int(8,0,4)),
                (t["OL"], new Vector3Int(8,1,4)),
                (t["OL"], new Vector3Int(12,0,4)),
                (t["OL"], new Vector3Int(12,1,4)),
                (t["LL"], new Vector3Int(0,0,8)),
                (t["LL"], new Vector3Int(0,1,8)),
                (t["JL"], new Vector3Int(4,0,8)),
                (t["JL"], new Vector3Int(4,1,8)),
                (t["LL"], new Vector3Int(8,0,8)),
                (t["LL"], new Vector3Int(8,1,8)),
                (t["LL"], new Vector3Int(12,0,8)),
                (t["LL"], new Vector3Int(12,1,8)),
                (t["LL"], new Vector3Int(16,0,0)),
                (t["LL"], new Vector3Int(16,1,0)),
            },new Vector3Int(1,0,0));
            var sample = ToSampleGrid(patterns, set, true, extraLayer:new Vector3Int(0,0,1));
            return (bricks, sample, set);
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
        
        public (string[] bricks, SampleGrid sample, TileSet) SmallSample()
        {
            var bricks = new string[] { "S", "L", "O", "J", "void" };
            var unit = GetUnitSize();
            var t = PatternData(GetTetrisTileSet(true), bricks);
            var set = GetTetrisTileSet(true).GetSubset(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["O"], new Vector3Int(0, 0, 2)),
                (t["O"], new Vector3Int(0, 0, 4)),
                (t["L"], new Vector3Int(0, 0, 6)),
                (t["O"], new Vector3Int(2, 0, 2)),
                (t["O"], new Vector3Int(2, 0, 4)),
                (t["L"], new Vector3Int(2, 0, 6)),
                
                (t["S"], new Vector3Int(4, 0, 2)),
                (t["S"], new Vector3Int(4, 0, 4)),
                (t["S"], new Vector3Int(4, 0, 6)),
                (t["S"], new Vector3Int(4, 0, 8)),
                
                (t["S"], new Vector3Int(6, 0, 0)),
                (t["S"], new Vector3Int(6, 0, 2)),
                (t["S"], new Vector3Int(6, 0, 4)),
                (t["S"], new Vector3Int(6, 0, 6)),
                
                (t["S"], new Vector3Int(8, 0, 0)),
                
                (t["O"], new Vector3Int(8, 0, 4)),
                (t["J"], new Vector3Int(8, 0, 6)),
                
                (t["L"], new Vector3Int(10, 0, 3)),
                (t["O"], new Vector3Int(10, 0, 6)),
                
                (t["O"], new Vector3Int(11, 0, 4)),
                (t["O"], new Vector3Int(12, 0, 6)),
                (t["J"], new Vector3Int(12, 0, 3)),
                
                
            }, new Vector3Int(1,0,1));

            var sample = ToSampleGrid(patterns, set, true, extraLayer:new Vector3Int(1,0,1));
            
            
            return (bricks, sample, set);
             
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
        
        public TileSet GetTetrisTileSet(bool includeVoid = false, int? thickness=null)
        {
            var unit = thickness ?? GetUnitSize();
            var tileL = new NonUniformTile(
                "L",
            new Vector3Int(2, unit, 3),
                new Color(240 / 255.0f, 160 / 255.0f, 0),
                new bool[,,] { { { true, true, true }, { true, false, false } } },
                null,
                
                computeAtomEdges:true
            );
            var tileT = new NonUniformTile(
                "T",
                new Vector3Int(2, unit, 3),
                new Color(160 / 255.0f, 0, 240/255.0f),
                new bool[,,] { { { false, true, false }, { true, true, true }} },
                null,
                computeAtomEdges:true
            );
            
            var tileJ = new NonUniformTile(
                "J",
                new Vector3Int(2, unit, 3),
                new Color(0, 0, 240 / 255.0f),
                new bool[,,] { { { true, false, false }, { true, true, true } } },
                null,
                computeAtomEdges:true
            );
            
            var tileI = new NonUniformTile(
                "I",
                new Vector3Int(4, unit, 1),
                new Color(120 / 255.0f, 120 / 255.0f, 1 / 255.0f),
                new bool[,,] { { { true }, { true }, { true }, { true } } },
                null,
                computeAtomEdges:true
            );
            
            var tileZ = new NonUniformTile(
                "Z",
                new Vector3Int(2, unit, 3),
                new Color(240 / 255.0f, 0, 0),
                new bool[,,] { { { true, true, false }, { false, true, true }} },
                null,
                computeAtomEdges:true
            );
            var tileS = new NonUniformTile(
                "S",
                new Vector3Int(2, unit, 3),
                new Color(0, 240 / 255.0f, 0),
                new bool[,,] { { { false, true, true }, { true, true, false }} },
                null,
                computeAtomEdges:true
            );
            var tileO = new NonUniformTile(
                "O",
                new Vector3Int(2, unit, 2),
                new Color(0, 240 / 255.0f, 240 / 255.0f),

                // new Color(240 / 255.0f, 240 / 255.0f, 0),
                new bool[,,] { { { true, true }, { true, true }} },
                null,
                computeAtomEdges:true
            );

            var voidTile = new NonUniformTile(
                "void",
                new Vector3Int(1, 1, 1),
                new Color(0, 0, 0, 0),
                computeAtomEdges: false, isEmptyTile:true);

            var tetrisTiles = new NonUniformTile[] { tileL, tileT, tileJ, tileI, tileZ, tileS, tileO };
            var tileSet = new TileSet();
            for (var i = 0; i < tetrisTiles.Length; i++)
            {
                tileSet[i] = tetrisTiles[i];
            }

            if (includeVoid) tileSet[tetrisTiles.Length] = voidTile;
            return tileSet;
        }

        private static bool[,,] GetBools(Vector3Int e, bool value, List<Range3D> invertRegions)
        {
            var bools = new bool[e.y, e.x, e.z];
            for (int x = 0; x < e.x; x++)
            {
                for (int y = 0; y < e.y; y++)
                {
                    for (int z = 0; z < e.z; z++)
                    {
                        bools[y, x, z] = value;
                        foreach (var invertRegion in invertRegions)
                        {
                            if (invertRegion.InRange(new Vector3Int(x, y, z)))
                            {
                                bools[y, x, z] = !value;
                            }
                        }
                    }
                }
            }

            return bools;
        }

        public TileSet GetLargeTetrisSet(bool includeVoid, int? thickness=null)
        {
            var unit = thickness ?? GetUnitSize();
            var largeL = GetBools(new Vector3Int(4, unit, 6), true, new List<Range3D>()
            {
                new (2,4,0,1,2,6)
            });
            var tileL = new NonUniformTile(
                "LL",
                new Vector3Int(),
                new Color(240 / 255.0f, 160 / 255.0f, 0),
                largeL,
                null,
                computeAtomEdges:true
            );
            
            var largeT = GetBools(new Vector3Int(4, unit, 6), true, new List<Range3D>()
            {
                new (2,4,0,1,0,2),
                new (2,4,0,1,4,6)
            });
            var tileT = new NonUniformTile(
                "TL",
                new Vector3Int(),
                new Color(160 / 255.0f, 0, 240/255.0f),
                largeT,
                null,
                computeAtomEdges:true
            );
            
            var largeJ = GetBools(new Vector3Int(4, unit, 6), true, new List<Range3D>()
            {
                new (0,2,0,1,2,6)
            });
            var tileJ = new NonUniformTile(
                "JL",
                new Vector3Int(),
                new Color(0, 0, 240 / 255.0f),
                largeJ,
                null,
                computeAtomEdges:true
            );
            
            var largeI = GetBools(new Vector3Int(8, unit, 2), true, new List<Range3D>());
            var tileI = new NonUniformTile(
                "IL",
                new Vector3Int(),
                new Color(120 / 255.0f, 120 / 255.0f, 1 / 255.0f),
                largeI,
                null,
                computeAtomEdges:true
            );
            
            var largeZ = GetBools(new Vector3Int(4, unit, 6), true, new List<Range3D>()
            {
                new (0,2,0,1,4,6),
                new (2,4,0,1,0,2)
            });
            var tileZ = new NonUniformTile(
                "ZL",
                new Vector3Int(),
                new Color(240 / 255.0f, 0, 0),
                largeZ,
                null,
                computeAtomEdges:true
            );

            var largeS = GetBools(new Vector3Int(4, unit, 6), true, new List<Range3D>()
            {
                new(0, 2, 0, unit, 0,2),
                new(2, 4, 0, unit, 4, 6)
            });

            var tileS = new NonUniformTile(
                "SL",
                new Vector3Int(),
                new Color(0, 240 / 255.0f, 0),
                largeS,
                null,
                computeAtomEdges:true
            );

            var largeO = GetBools(new Vector3Int(4, unit, 4), true, new List<Range3D>());
            var tileO = new NonUniformTile(
                "OL",
                new Vector3Int(),
                new Color(0, 240 / 255.0f, 240 / 255.0f),

                // new Color(240 / 255.0f, 240 / 255.0f, 0),
                largeO,
                null,
                computeAtomEdges:true
            );
            
            
            var voidTile = new NonUniformTile(
                "void",
                new Vector3Int(1, 1, 1),
                new Color(0, 0, 0, 0),
                computeAtomEdges: false, isEmptyTile:true);
            
            
            var tetrisTiles = new NonUniformTile[] { tileL, tileT, tileJ, tileI, tileZ, tileS, tileO };
            var tileSet = new TileSet();
            for (var i = 0; i < tetrisTiles.Length; i++)
            {
                tileSet[i] = tetrisTiles[i];
            }

            if (includeVoid) tileSet[tetrisTiles.Length] = voidTile;
            return tileSet;
        }
    }
}