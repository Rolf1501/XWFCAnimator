using System.Collections.Generic;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;
using UnityEngine;

namespace XWFC
{
    public class LegoSet : TileSet
    {
        public bool PlateAtoms;
        public LegoSet(bool plateAtoms = true)
        {
            PlateAtoms = plateAtoms;
        }
        public TileSet GetLegoSet()
        {
            var unit = BrickUnitSize(PlateAtoms);
            // b211, b312, b412, p111, p211, door, window, void
            var tiles = new NonUniformTile[]
            {
                // Bricks
                new NonUniformTile(
                    "b112",
                    new Vector3Int(1,unit,2),
                    new Color32(35, 120, 65, 255)
                ),
                new NonUniformTile(
                    "b312",
                    new Vector3Int(3,unit,2),
                    new Color32(0, 85, 191, 255)
                ),
                new NonUniformTile(
                    "b412",
                    new Vector3Int(4,unit,2),
                    new Color32(201, 26, 9, 255)
                ),
                new NonUniformTile(
                    "b212",
                    new Vector3Int(2,unit,2 ),
                    new Color32(0, 185, 241, 255)
                ),
                new NonUniformTile(
                    "b111",
                    new Vector3Int(1,unit,1),
                    new Color32(242, 243, 242, 255)
                ),
                
                // Bricks 90deg
                new NonUniformTile(
                    "b211",
                    new Vector3Int(2,unit,1),
                    new Color32(35, 120, 65, 255)
                ),
                new NonUniformTile(
                    "b213",
                    new Vector3Int(2,unit,3),
                    new Color32(0, 85, 191, 255)
                ),
                new NonUniformTile(
                    "b214",
                    new Vector3Int(2,unit,4),
                    new Color32(201, 26, 9, 255)
                ),
                
                // Plates
                new NonUniformTile(
                    "p111",
                    new Vector3Int(1,1,1),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "p211",
                    new Vector3Int(2,1,1),
                    new Color32(242, 205, 55, 255)
                ),
                new NonUniformTile(
                    "p212",
                    new Vector3Int(2,1,2),
                    new Color32(165, 165, 203, 255)
                ),
                
                // Compound bricks
                new NonUniformTile(
                    "door",
                    new Vector3Int(3, 5*unit, 1),
                    new Color(0.4f, 0.2f, 0.1f)
                ),
                new NonUniformTile(
                    "window",
                    new Vector3Int(3,3*unit,1),
                    new Color(0, 0, 0.8f)
                ),
                
                // Compound bricks 90deg
                new NonUniformTile(
                    "door90",
                    new Vector3Int(1, 5*unit, 3),
                    new Color(0.4f, 0.2f, 0.1f)
                ),
                new NonUniformTile(
                    "window90",
                    new Vector3Int(1,3*unit,3),
                    new Color(0, 0, 0.8f)
                ),
                
                // Void
                new NonUniformTile(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color(0,0,0,0.5f),
                    isEmptyTile:true,computeAtomEdges:false
                ),
            };

            var tileSet = new TileSet();
            for (var i = 0; i < tiles.Length; i++)
            {
                tileSet[i] = tiles[i];
            }

            return tileSet;
        }

        public (TileSet t, List<SampleGrid>) StackedBricksExample()
        {
            var unit = BrickUnitSize(PlateAtoms);
            var bricks = new string[] { "b412" };
            var legoTiles = GetLegoSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["b412"], new Vector3Int(0, 0, 0)),
                (t["b412"], new Vector3Int(4, 0, 0)),
                (t["b412"], new Vector3Int(0, 0, 2)),
                (t["b412"], new Vector3Int(4, 0, 2)),
                (t["b412"], new Vector3Int(0, unit, 0)),
                (t["b412"], new Vector3Int(4, unit, 0)),
                (t["b412"], new Vector3Int(0, unit, 2)),
                (t["b412"], new Vector3Int(4, unit, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, false);
            return (legoTiles, new List<SampleGrid>() { grid });
            
        }
        
        public (TileSet t, List<SampleGrid>) StackedPlateExample()
        {
            var bricks = new string[] { "p212" };
            var legoTiles = GetLegoSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            var stackedPattern = new Patterns()
            {
                (t["p212"], new Vector3Int(0, 0, 0)),
                (t["p212"], new Vector3Int(2, 0, 0)),
                (t["p212"], new Vector3Int(0, 0, 2)),
                (t["p212"], new Vector3Int(2, 0, 2)),
                (t["p212"], new Vector3Int(0, 1, 0)),
                (t["p212"], new Vector3Int(2, 1, 0)),
                (t["p212"], new Vector3Int(0, 1, 2)),
                (t["p212"], new Vector3Int(2, 1, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, false);

            return (legoTiles, new List<SampleGrid>() { grid });
        }

        public static int BrickUnitSize(bool plateAtoms = true)
        {
            /*
             * A brick is three time as high as a plate.
             */
            return plateAtoms ? 3 : 1;
        }



        public (TileSet legoTiles, List<SampleGrid>) WallPerimeter3DExample()
        {
            var unit = BrickUnitSize(PlateAtoms);
            var bricks = new string[] { "b412", "b214", "b213", "b312", "b212", "void" };
            var legoTiles = GetLegoSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            var corners = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(0,0,0)),
                (t["b412"], new Vector3Int(2,0,4)),
                
                (t["b214"], new Vector3Int(0,0,2)),
                (t["b214"], new Vector3Int(4,0,0)),
                
                (t["b412"], new Vector3Int(2,unit,0)),
                (t["b412"], new Vector3Int(0,unit,4)),
                
                (t["b214"], new Vector3Int(0,unit,0)),
                (t["b214"], new Vector3Int(4,unit,2)),
                
                (t["b412"], new Vector3Int(0,2*unit,0)),
                (t["b412"], new Vector3Int(2,2*unit,4)),
                
                (t["b214"], new Vector3Int(0,2*unit,2)),
                (t["b214"], new Vector3Int(4,2*unit,0)),
            }, new Vector3Int(1,0,1));
            
            var cornerSample = ToSampleGrid(corners, legoTiles);
            // var (NORTH, EAST, SOUTH, WEST, UP, DOWN) = (Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
            //     Vector3.up, Vector3.down);

            // var corner0 = TranslatePattern(new Patterns()
            // {
            //     (t["b412"], new Vector3Int(0, 0, 0)),
            //     (t["b412"], new Vector3Int(2, 3, 0)),
            //     (t["b412"], new Vector3Int(0, 6, 0)),
            //     (t["b214"], new Vector3Int(0, 0, 2)),
            //     (t["b214"], new Vector3Int(0, 3, 0)),
            //     (t["b214"], new Vector3Int(0, 6, 2)),
            // }, new Vector3Int(1,0,1));
            //
            // var corner0Grid = ToSampleGrid(corner0, legoTiles, new Vector3Int(7, 9, 7));
            // //
            // //
            // // LayerAdd(ref corner0, new Range3D(0,8,0,9,0,1), t["void"]);
            // // LayerAdd(ref corner0, new Range3D(0,1,0,9,0,8), t["void"]);
            // // LayerAdd(ref corner0, new Range3D(3,4,0,9,3,8), t["void"]);
            // // LayerAdd(ref corner0, new Range3D(3,8,0,9,3,4), t["void"]);
            //
            // var corner1 = TranslatePattern(new Patterns()
            // {
            //     (t["b412"], new Vector3Int(0, 0, 4)),
            //     (t["b412"], new Vector3Int(2, 3, 4)),
            //     (t["b412"], new Vector3Int(0, 6, 4)),
            //     (t["b214"], new Vector3Int(0, 0, 0)),
            //     (t["b214"], new Vector3Int(0, 3, 2)),
            //     (t["b214"], new Vector3Int(0, 6, 0)),
            // }, new Vector3Int(1,0,0));
            //
            // var corner1Grid = ToSampleGrid(corner1, legoTiles, new Vector3Int(7, 9, 7));
            //
            //
            // // LayerAdd(ref corner1, new Range3D(0,8,0,9,6,7), t["void"]);
            // // LayerAdd(ref corner1, new Range3D(0,1,0,9,0,7), t["void"]);
            // // LayerAdd(ref corner1, new Range3D(3,4,0,9,0,4), t["void"]);
            // // LayerAdd(ref corner1, new Range3D(3,8,0,9,3,4), t["void"]);
            //
            // var corner2 = TranslatePattern(new Patterns()
            // {
            //     (t["b412"], new Vector3Int(2, 0, 0)),
            //     (t["b412"], new Vector3Int(0, 3, 0)),
            //     (t["b412"], new Vector3Int(2, 6, 0)),
            //     (t["b214"], new Vector3Int(4, 0, 2)),
            //     (t["b214"], new Vector3Int(4, 3, 0)),
            //     (t["b214"], new Vector3Int(4, 6, 2)),
            // }, new Vector3Int(0,0,1));
            //
            // var corner2Grid = ToSampleGrid(corner2, legoTiles, new Vector3Int(7, 9, 7));
            //
            // // LayerAdd(ref corner2, new Range3D(0,8,0,9,0,1), t["void"]);
            // // LayerAdd(ref corner2, new Range3D(7,8,0,9,0,8), t["void"]);
            // // LayerAdd(ref corner2, new Range3D(3, 4,0,9,3,8), t["void"]);
            // // LayerAdd(ref corner2, new Range3D(0,4,0,9,3,4), t["void"]);
            //
            //
            // var corner3 = new Patterns()
            // {
            //     (t["b412"], new Vector3Int(0, 0, 4)),
            //     (t["b412"], new Vector3Int(2, 3, 4)),
            //     (t["b412"], new Vector3Int(0, 6, 4)),
            //     (t["b214"], new Vector3Int(4, 0, 0)),
            //     (t["b214"], new Vector3Int(4, 3, 2)),
            //     (t["b214"], new Vector3Int(4, 6, 0)),
            // };
            // var corner3Grid = ToSampleGrid(corner3, legoTiles, new Vector3Int(7, 9, 7));
            
            // LayerAdd(ref corner3, new Range3D(0,7,0,9,6,7), t["void"]);
            // LayerAdd(ref corner3, new Range3D(6,7,0,9,0,7), t["void"]);
            // LayerAdd(ref corner3, new Range3D(3,4,0,9,0,4), t["void"]);
            // LayerAdd(ref corner3, new Range3D(0,4,0,9,3,4), t["void"]);


            var brickPattern412 = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(2, 0, 0)),
                (t["b412"], new Vector3Int(0, unit, 0)),
                (t["b412"], new Vector3Int(4, unit, 0)),
                (t["b412"], new Vector3Int(2, 2*unit, 0)),
            }, new Vector3Int(0,0,1));
            
            LayerAdd(ref brickPattern412, new Range3D(0,8,0,3*unit,0,1), t["void"]);
            LayerAdd(ref brickPattern412, new Range3D(0,8,0,3*unit,3,4), t["void"]);

            var brickPattern412Grid = ToSampleGrid(brickPattern412, legoTiles, false);
            
            var brickPattern214 = TranslatePattern(new Patterns()
            {
                (t["b214"], new Vector3Int(0, 0, 2)),
                (t["b214"], new Vector3Int(0, unit, 0)),
                (t["b214"], new Vector3Int(0, unit, 4)),
                (t["b214"], new Vector3Int(0, 2*unit, 2)),
            }, new Vector3Int(1,0,0));
            
            LayerAdd(ref brickPattern214, new Range3D(0, 1, 0,3*unit, 0, 8), t["void"]);
            LayerAdd(ref brickPattern214, new Range3D(3, 4, 0,3*unit, 0, 8), t["void"]);

            var brickPattern214Grid = ToSampleGrid(brickPattern214, legoTiles, false);

            var voids = ToSampleGrid(new Patterns(), legoTiles, new Vector3Int(2, 2, 2));
            // var voids = new Patterns();
            // LayerAdd(ref voids, new Range3D(0,2,0,2,0,2), t["void"]);
           
            // return (legoTiles, new List<Patterns>() { corners, brickPattern412, brickPattern214, voids });
            return (legoTiles, new List<SampleGrid> { cornerSample, brickPattern412Grid, brickPattern214Grid, voids });
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
        private static SampleGrid ToSampleGrid(Patterns patterns, TileSet tileSet, bool fillWithVoids=true)
        {
            var extent = new Vector3Int();
            foreach (var (id, c) in patterns)
            {
                var nutMaxCoord = c + tileSet[id].Extent;
                extent = Vector3Util.PairWiseMax(extent, nutMaxCoord);
            }

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