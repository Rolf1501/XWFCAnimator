using System.Collections.Generic;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;
using UnityEngine;
using UnityEngine.Rendering;

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
                new(
                    "b112",
                    new Vector3Int(1,unit,2),
                    new Color32(35, 120, 65, 255)
                ),
                new(
                    "b312",
                    new Vector3Int(3,unit,2),
                    new Color32(0, 85, 191, 255)
                ),
                new(
                    "b412",
                    new Vector3Int(4,unit,2),
                    new Color32(201, 26, 9, 255)
                ),
                new(
                    "b212",
                    new Vector3Int(2,unit,2 ),
                    new Color32(0, 185, 241, 255)
                ),
                new(
                    "b111",
                    new Vector3Int(1,unit,1),
                    new Color32(242, 243, 242, 255)
                ),
                new(
                    "b115",
                    new Vector3Int(1,unit,5),
                    new Color32(242, 243, 242, 255)
                ),
                // Bricks 90deg
                new(
                    "b211",
                    new Vector3Int(2,unit,1),
                    new Color32(35, 120, 65, 255)
                ),
                new(
                    "b213",
                    new Vector3Int(2,unit,3),
                    new Color32(0, 85, 191, 255)
                ),
                new(
                    "b214",
                    new Vector3Int(2,unit,4),
                    new Color32(201, 26, 9, 255)
                ),
                new(
                    "b216",
                    new Vector3Int(2,unit,6),
                    new Color32(120, 100, 160, 255)
                ),
                
                // Plates
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
                //
                // new(
                //     "p414",
                //     new Vector3Int(4,1,4),
                //     new Color32(20, 200, 160, 255)
                // ),
                //
                // new(
                //     "p317",
                //     new Vector3Int(3,1,7),
                //     new Color32(190, 100, 10, 255)
                // ),
                
                // Compound bricks
                // new(
                //     "door",
                //     new Vector3Int(3, 5*unit, 2),
                //     new Color(0.4f, 0.2f, 0.1f)
                // ),
                new(
                    "doorEven",
                    new Vector3Int(4, 6*unit, 2),
                    new Color(0.4f, 0.2f, 0.1f)
                ),
                // new(
                //     "window",
                //     new Vector3Int(3,3*unit,1),
                //     new Color(0, 0, 0.8f)
                // ),
                // new(
                //     "windowEven",
                //     new Vector3Int(4,3*unit,1),
                //     new Color(0, 0, 0.8f)
                // ),
                new(
                    "windowSplit",
                    new Vector3Int(4,5*unit,2),
                    new Color(0, 0, 0.8f),
                    mask: new bool[,,]
                    {
                        {
                            {false,false},{true,true},{true,true},{false,false},
                            
                        },
                        {
                            {true,true},{true,true},{true,true},{true,true},
                        },
                        {
                            {false,false},{false,false},{false,false},{false,false},
                        },
                        {
                            {true,true},{true,true},{true,true},{true,true},
                        },
                        {
                            {false,false},{true,true},{true,true},{false,false},
                            
                        },
                    }
                    ),
                
                // Compound bricks 90deg
                // new(
                //     "door90",
                //     new Vector3Int(1, 5*unit, 3),
                //     new Color(0.4f, 0.2f, 0.1f)
                // ),
                // new(
                //     "window90",
                //     new Vector3Int(1,3*unit,3),
                //     new Color(0, 0, 0.8f)
                // ),
                
                // Void
                new(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color(0,0,0,0f),
                    isEmptyTile:true,computeAtomEdges:false
                ),
                
                // new(
                //     "voidBrick",
                //     new Vector3Int(unit,unit,unit),
                //     new Color(0,0,0,0f),
                //     isEmptyTile:true,computeAtomEdges:false
                // ),
                //
                // new(
                //     "voidLarge",
                //     new Vector3Int(2*unit,2*unit,2*unit),
                //     new Color(0,0,0,0f),
                //     isEmptyTile:true,computeAtomEdges:false
                // ),
            };

            var tileSet = new TileSet();
            for (var i = 0; i < tiles.Length; i++)
            {
                tileSet[i] = tiles[i];
            }

            return tileSet;
        }
        
        

        public static Component[] LegoHouse()
        {
            var lego = new LegoSet(false);
            // var (t, samples) = lego.WallPerimeter3DExample();
            // var (t, samples) = lego.DoorExample();
            // var (t, samples) = lego.WallExample();
            // var (t, samples) = lego.DoorOddExample();
            var (tDoor, sDoor) = lego.DoorEvenExample();
            var (tWindow, sWindow) = lego.SplitWindowExample();
            var (tBalcony, sBalcony) = lego.BalconyExample();
            var (tRoof, sRoof) = lego.RoofExample();
            var (tChimney, sChimney) = lego.ChimneyExample();
            // var (t, p) = LegoSet.StackedBricksExample();

            var unit = LegoSet.BrickUnitSize(lego.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);

            var oDoor = new Vector3Int(6, 0, 6);
            var eDoor = new Vector3Int(30, 10, 2) * unitV;

            var oWindow = new Vector3Int(0, eDoor.y, 0) + oDoor;
            var eWindow = new Vector3Int(20, 10, 2) * unitV;

            var oBalcony = new Vector3Int(0,eWindow.y, -6) + oWindow;
            var eBalcony = new Vector3Int(20, 8, 8) * unitV;

            var oRoof = new Vector3Int(-7, eBalcony.y, 6) + oBalcony;
            var eRoof = new Vector3Int(40, 15, 2) * unitV;

            var oChimney = new Vector3Int(25, eRoof.y, 0) + oRoof;
            var eChimney = new Vector3Int(4, 8, 2) * unitV;
            
            var cDoor = new Component(
                oDoor, 
                eDoor, 
                tDoor, sDoor.ToArray(),
                customSeed:26
                );

            var cWindow = new Component(
                oWindow, 
                eWindow, 
                tWindow, sWindow.ToArray(),
                customSeed:20
                );
            
            var cBalcony = new Component(
                oBalcony, 
                eBalcony,
                tBalcony, sBalcony.ToArray(),
                customSeed: 1340540562
                );
            
            var cRoof = new Component(
                oRoof, 
                eRoof,
                tRoof, sRoof.ToArray(),
                customSeed:20
                );
            
            var cChimney = new Component(
                oChimney, 
                eChimney, 
                tChimney, sChimney.ToArray(),
                customSeed:20
                );

            var components = new[] { cDoor, cWindow, cBalcony, cRoof, cChimney };

            return components;
        }

        public (TileSet t, List<SampleGrid>) SplitWindowExample()
        {
            var oddBricks = GetOddBrickPattern();
            var voids = GetVoidPattern();
            // var splitWindow = GetSplitWindowPattern();
            var oddCorners = GetCornerPatternOdd(false);

            var patterns = new[] { oddBricks, oddCorners };
            
            return ExtractTilesAndSamples(patterns); 
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
        
        public (TileSet legoTiles, List<SampleGrid> samples) WallPerimeter3DExample()
        {
            var cornerSample = GetCornerPattern();
            var brickPattern412Sample = Get412BrickPattern();
            var brickPattern214Sample = Get214BrickPattern();
            var voids = GetVoidPattern();

            var patterns = new[] { cornerSample, brickPattern412Sample, brickPattern214Sample, voids };
            
            return ExtractTilesAndSamples(patterns); 
        }

        public (TileSet legoTiles, List<SampleGrid> samples) WallExample()
        {
            var oddSample = GetOddConnectorPattern();
            var evenSample = GetRightEvenConnectorPattern();
            var brickSample = Get412BrickPattern();
            var doorSample = GetDoorPattern();

            var patterns = new[] { oddSample, evenSample, brickSample, doorSample };
            return ExtractTilesAndSamples(patterns);
        }

        // public (TileSet legoTiles, List<SampleGrid> samples) DoorOddExample()
        // {
        //     var cornerSample = GetCornerPattern();
        //     var brickPattern412Sample = Get412BrickPattern();
        //     var brickPattern214Sample = Get214BrickPattern();
        //     var doorSample = GetDoorPattern();
        //     var oddSample = GetOddConnectorPattern();
        //     var evenSample = GetRightEvenConnectorPattern();
        //     var voids = GetVoidPattern();
        //     
        //     var patterns = new[] { cornerSample, brickPattern412Sample, brickPattern214Sample, doorSample, oddSample, evenSample, voids };
        //     return ExtractTilesAndSamples(patterns); 
        // }

        public (TileSet legoTiles, List<SampleGrid> samples) ChimneyExample()
        {
            var chimney = GetChimneyPattern();
            var patterns = new[] { chimney };
            return ExtractTilesAndSamples(patterns);
        }

        public (TileSet legoTiles, List<SampleGrid> samples) BalconyExample()
        {
            var balcony = GetBalconyPattern();
            var patterns = new[] { balcony };
            return ExtractTilesAndSamples(patterns);
        }

        public (TileSet legoTiles, List<SampleGrid> samples) RoofExample()
        {
            var roof = GetRoofPattern();
            var patterns = new[] { roof };
            return ExtractTilesAndSamples(patterns);
        }
        public (TileSet legoTiles, List<SampleGrid> samples) DoorEvenExample()
        {
            var door = DoorOnlyPattern2();
            var corner = GetCornerPattern(false);
            var brick412 = Get412BrickPattern();
            var brick214 = Get214BrickPattern();
            var voids = GetVoidPattern();
            var patterns = new[] { door, corner, brick412, brick214, voids };
            
            return ExtractTilesAndSamples(patterns);
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
            
            var legoTiles = GetLegoSet().GetSubset(bricks);

            return (legoTiles, sampleGrids);
        }


        private (int unit, TileSet legoTiles, Dictionary<string, int> t) PatternData(string[] bricks)
        {
            var unit = BrickUnitSize(PlateAtoms);
            var legoTiles = GetLegoSet().GetSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            return (unit, legoTiles, t);
        }

        private (string[] bricks, SampleGrid sample) GetBalconyPattern()
        {
            var bricks = new string[] { "b211", "b111", "b112", "b115", "b216", "b412","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(0,0,4)),
                (t["b412"], new Vector3Int(0,unit,4)),
                (t["b412"], new Vector3Int(0,2*unit,4)),
                (t["b412"], new Vector3Int(0,3*unit,4)),
                (t["b412"], new Vector3Int(0,4*unit,4)),
                (t["b412"], new Vector3Int(0,5*unit,4)),
                (t["b412"], new Vector3Int(0,6*unit,4)),
                
                (t["b412"], new Vector3Int(4,5*unit,4)),
                (t["b412"], new Vector3Int(4,6*unit,4)),
                
                (t["b412"], new Vector3Int(8,0,4)),
                (t["b412"], new Vector3Int(8,unit,4)),
                (t["b412"], new Vector3Int(8,2*unit,4)),
                (t["b412"], new Vector3Int(8,3*unit,4)),
                (t["b412"], new Vector3Int(8,4*unit,4)),
                (t["b412"], new Vector3Int(8,5*unit,4)),
                (t["b412"], new Vector3Int(8,6*unit,4)),
                
                (t["b216"], new Vector3Int(4,0,0)),
                (t["b216"], new Vector3Int(6,0,0)),
                
                (t["b111"], new Vector3Int(4,unit,0)),
                (t["b112"], new Vector3Int(4,2*unit,0)),
                (t["b115"], new Vector3Int(4,3*unit,1)),
                (t["b211"], new Vector3Int(4,3*unit,0)),
                
                (t["b111"], new Vector3Int(7,unit,0)),
                (t["b112"], new Vector3Int(7,2*unit,0)),
                (t["b115"], new Vector3Int(7,3*unit,1)),
                (t["b211"], new Vector3Int(6,3*unit,0)),
                
                
            }, new Vector3Int(0,0,1));
            // }, new Vector3Int(1,0,1));

            var sample = ToSampleGrid(patterns, legoTiles, true);
            // var sample = ToSampleGrid(patterns, legoTiles, true, extraLayer:new Vector3Int(1,0,0));
            return (bricks, sample);
        }
        private (string[] bricks, SampleGrid sample) GetRoofPattern()
        {
            var bricks = new string[] { "b412", "b212","b312","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(1,0,0)),
                (t["b412"], new Vector3Int(9,0,0)),
                
                (t["b212"], new Vector3Int(0,unit,0)),
                (t["b212"], new Vector3Int(1,2*unit,0)),
                (t["b212"], new Vector3Int(2,3*unit,0)),
                (t["b212"], new Vector3Int(3,4*unit,0)),
                
                (t["b212"], new Vector3Int(4,5*unit,0)),
                (t["b212"], new Vector3Int(6,5*unit,0)),
                (t["b212"], new Vector3Int(8,5*unit,0)),
                
                (t["b212"], new Vector3Int(9,4*unit,0)),
                (t["b212"], new Vector3Int(10,3*unit,0)),
                (t["b212"], new Vector3Int(11,2*unit,0)),
                (t["b212"], new Vector3Int(12,unit,0)),
                
                (t["b312"], new Vector3Int(2,unit,0)),
                (t["b312"], new Vector3Int(3,2*unit,0)),
                (t["b312"], new Vector3Int(4,3*unit,0)),
                
                (t["b312"], new Vector3Int(7,3*unit,0)),
                (t["b312"], new Vector3Int(8,2*unit,0)),
                (t["b312"], new Vector3Int(9,unit,0)),
                
                (t["b412"], new Vector3Int(5,4*unit,0)),
                
                
            }, new Vector3Int(0,0,1));

            var sample = ToSampleGrid(patterns, legoTiles, true);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetChimneyPattern()
        {
            var bricks = new string[] { "b112", "b212","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b112"], new Vector3Int(0, 0, 0)),
                (t["b112"], new Vector3Int(0, 2*unit, 0)),
                
                (t["b112"], new Vector3Int(4, unit, 0)),
                (t["b112"], new Vector3Int(4, 3*unit, 0)),
                
                (t["b212"], new Vector3Int(0, unit, 0)),
                (t["b212"], new Vector3Int(0, 3*unit, 0)),
                
                (t["b212"], new Vector3Int(2, unit, 0)),
                (t["b212"], new Vector3Int(2, 3*unit, 0)),
                
                (t["b212"], new Vector3Int(1, 0, 0)),
                (t["b212"], new Vector3Int(1, 2*unit, 0)),
                
                (t["b212"], new Vector3Int(3, 0, 0)),
                (t["b212"], new Vector3Int(3, 2*unit, 0)),
            }, new Vector3Int());

            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetSplitWindowPattern()
        {
            var bricks = new string[] { "b112", "b212","b312", "windowSplit","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["windowSplit"], new Vector3Int(4,unit,0)),
                
                (t["b312"], new Vector3Int(0,unit,0)),
                (t["b312"], new Vector3Int(0,3*unit,0)),
                (t["b312"], new Vector3Int(0,5*unit,0)),
                (t["b312"], new Vector3Int(0,7*unit,0)),
                
                (t["b312"], new Vector3Int(2,0,0)),
                (t["b312"], new Vector3Int(2,6*unit,0)),
                (t["b312"], new Vector3Int(2,8*unit,0)),
                
                (t["b212"], new Vector3Int(2,2*unit,0)),
                (t["b212"], new Vector3Int(2,4*unit,0)),
                
                (t["b212"], new Vector3Int(3,unit,0)),
                (t["b212"], new Vector3Int(3,5*unit,0)),
                
                (t["b312"], new Vector3Int(3,3*unit,0)),
                (t["b312"], new Vector3Int(3,7*unit,0)),
                
                (t["b212"], new Vector3Int(5,0,0)),
                (t["b212"], new Vector3Int(5,6*unit,0)),
               
                (t["b312"], new Vector3Int(6,3*unit,0)),
                (t["b312"], new Vector3Int(6,7*unit,0)),
               
                (t["b312"], new Vector3Int(7,0,0)),
                (t["b312"], new Vector3Int(7,6*unit,0)),
                (t["b312"], new Vector3Int(7,8*unit,0)),
                
                (t["b212"], new Vector3Int(7,unit,0)),
                (t["b212"], new Vector3Int(7,5*unit,0)),
                
                (t["b212"], new Vector3Int(8,2*unit,0)),
                (t["b212"], new Vector3Int(8,4*unit,0)),
                
                (t["b312"], new Vector3Int(9,unit,0)),
                (t["b312"], new Vector3Int(9,3*unit,0)),
                (t["b312"], new Vector3Int(9,5*unit,0)),
                (t["b312"], new Vector3Int(9,7*unit,0)),
            }, new Vector3Int(0, 0, 0));
            var sample = ToSampleGrid(patterns, legoTiles,false);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetOddBrickPattern()
        {
            var bricks = new string[] { "b112", "b212","b312", "windowSplit","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b312"], new Vector3Int(0, unit, 0)),
                (t["b312"], new Vector3Int(0, 3*unit, 0)),
                (t["b312"], new Vector3Int(0, 5*unit, 0)),
                
                (t["b312"], new Vector3Int(1, 0, 0)),
                (t["b312"], new Vector3Int(1, 2*unit, 0)),
                (t["b312"], new Vector3Int(1, 4*unit, 0)),
                (t["b312"], new Vector3Int(1, 6*unit, 0)),
                
                (t["b212"], new Vector3Int(3, unit, 0)),
                (t["b212"], new Vector3Int(3, 3*unit, 0)),
                (t["b212"], new Vector3Int(3, 5*unit, 0)),
                
                (t["b312"], new Vector3Int(4, 0, 0)),
                (t["b312"], new Vector3Int(4, 2*unit, 0)),
                (t["b312"], new Vector3Int(4, 4*unit, 0)),
                (t["b312"], new Vector3Int(4, 6*unit, 0)),
                
                (t["b312"], new Vector3Int(5, unit, 0)),
                (t["b312"], new Vector3Int(5, 3*unit, 0)),
                (t["b312"], new Vector3Int(5, 5*unit, 0)),
                
                (t["b112"], new Vector3Int(7, 0, 0)),
                (t["b112"], new Vector3Int(7, 2*unit, 0)),
                (t["b112"], new Vector3Int(7, 4*unit, 0)),
                (t["b112"], new Vector3Int(7, 6*unit, 0)),

                (t["b112"], new Vector3Int(0, 0, 0)),
                (t["b112"], new Vector3Int(0, 2*unit, 0)),
                (t["b112"], new Vector3Int(0, 4*unit, 0)),
                (t["b112"], new Vector3Int(0, 6*unit, 0)),
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,9,0,9,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,9,0,9,0,1), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }
        
        
        private (string[] bricks, SampleGrid sample) GetOddConnectorPattern()
        {
            var bricks = new string[] { "b412", "b112", "b312", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var oddConnectorPattern = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(0, 2 * unit, 0)),
                (t["b412"], new Vector3Int(2, unit, 0)),
                (t["b412"], new Vector3Int(2, 3 * unit, 0)),
                (t["b412"], new Vector3Int(6, 3 * unit, 0)),

                (t["b312"], new Vector3Int(4, 0, 0)),
                (t["b312"], new Vector3Int(4, 2 * unit, 0)),

                (t["b112"], new Vector3Int(6, unit, 0)),
            }, new Vector3Int(0, 0, 1));
            
            LayerAdd(ref oddConnectorPattern, new Range3D(0,10,0,4*unit, 0,1), t["void"]); // front
            LayerAdd(ref oddConnectorPattern, new Range3D(0,10,0,4*unit, 3,4), t["void"]); // back

            var sample = ToSampleGrid(oddConnectorPattern, legoTiles, fillWithVoids: false);

            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetRightEvenConnectorPattern()
        {
            var bricks = new string[] { "b412", "b212", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var evenConnectorPattern = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(0, 0, 0)),
                (t["b412"], new Vector3Int(0, 2 * unit, 0)),
                (t["b412"], new Vector3Int(2, unit, 0)),
                
                (t["b212"], new Vector3Int(0, unit, 0)),
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref evenConnectorPattern, new Range3D(0,6,0,2*unit, 0,1), t["void"]); // front
            LayerAdd(ref evenConnectorPattern, new Range3D(0,6,0,2*unit, 3,4), t["void"]); // back

            var sample = ToSampleGrid(evenConnectorPattern, legoTiles, fillWithVoids: false);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) GetLeftEvenConnectorPattern()
        {
            var bricks = new string[] { "b412", "b212", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var evenConnectorPattern = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(2, 0, 0)),
                (t["b412"], new Vector3Int(2, 2 * unit, 0)),
                (t["b412"], new Vector3Int(0, unit, 0)),
                
                (t["b212"], new Vector3Int(4, unit, 0)),
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref evenConnectorPattern, new Range3D(0,6,0,2*unit, 0,1), t["void"]); // front
            LayerAdd(ref evenConnectorPattern, new Range3D(0,6,0,2*unit, 3,4), t["void"]); // back

            var sample = ToSampleGrid(evenConnectorPattern, legoTiles, fillWithVoids: false);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetDoorPattern()
        {
            var bricks = new string[] { "b412", "b112", "b312", "b212", "void", "door" };
            var (unit, legoTiles, t) = PatternData(bricks);
            
            /*
             * Surrounds a door with bricks. Door has slight inset.
             */
            var doorPattern = TranslatePattern(new Patterns()
            {
                (t["door"], new Vector3Int(3, 0, 0)),
                (t["door"], new Vector3Int(3, 0, 1)),
                
                (t["b312"], new Vector3Int(0, 0, 0)),
                (t["b312"], new Vector3Int(0, 2*unit, 0)), 
                (t["b312"], new Vector3Int(0, 4*unit, 0)),
                
                (t["b112"], new Vector3Int(2,unit,0)),
                (t["b112"], new Vector3Int(2,3*unit,0)),
                
                (t["b412"], new Vector3Int(2,5*unit,0)),
                
                (t["b412"], new Vector3Int(6,unit,0)),
                (t["b412"], new Vector3Int(6, 3*unit,0)),
                (t["b412"], new Vector3Int(6,5*unit,0)),
                
                (t["b212"], new Vector3Int(6,0,0)),
                (t["b212"], new Vector3Int(6,2*unit,0)),
                (t["b212"], new Vector3Int(6,4*unit,0)),
                
            }, new Vector3Int(0,0,1));
            LayerAdd(ref doorPattern, new Range3D(0,10,0,6*unit, 0,1), t["void"]); // Layer in front.
            LayerAdd(ref doorPattern, new Range3D(0,10,0,6*unit, 3,4), t["void"]); // Layer in back.

            var doorSample = ToSampleGrid(doorPattern, legoTiles, fillWithVoids: false);
            return (bricks, doorSample);
        }

        private (string[] bricks, SampleGrid sample) DoorOnlyPattern()
        {
            var bricks = new string[] { "b212", "void", "doorEven" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var doorPattern = TranslatePattern(new Patterns()
            {
                (t["doorEven"], new Vector3Int(2, 0, 0)),
                (t["doorEven"], new Vector3Int(2, 0, 1)),
                
                (t["b212"], new Vector3Int(0,0,0)),
                (t["b212"], new Vector3Int(0,unit,0)),
                (t["b212"], new Vector3Int(0,2*unit,0)),
                (t["b212"], new Vector3Int(0,3*unit,0)),
                (t["b212"], new Vector3Int(0,4*unit,0)),
                (t["b212"], new Vector3Int(0,5*unit,0)),
                (t["b212"], new Vector3Int(0,6*unit,0)),
                
                (t["b212"], new Vector3Int(6,0,0)),
                (t["b212"], new Vector3Int(6,unit,0)),
                (t["b212"], new Vector3Int(6,2*unit,0)),
                (t["b212"], new Vector3Int(6,3*unit,0)),
                (t["b212"], new Vector3Int(6,4*unit,0)),
                (t["b212"], new Vector3Int(6,5*unit,0)),
                (t["b212"], new Vector3Int(6,6*unit,0)),
                
                (t["b212"], new Vector3Int(2,6*unit,0)),
                (t["b212"], new Vector3Int(2,7*unit,0)),
                (t["b212"], new Vector3Int(4,6*unit,0)),
                (t["b212"], new Vector3Int(4,7*unit,0)),
            }, new Vector3Int(0,0,1));
            LayerAdd(ref doorPattern, new Range3D(0,10,0,6*unit, 0,1), t["void"]); // Layer in front.
            LayerAdd(ref doorPattern, new Range3D(0,10,0,7*unit, 3,4), t["void"]); // Layer in back.

            var doorSample = ToSampleGrid(doorPattern, legoTiles, fillWithVoids: false);
            return (bricks, doorSample);
        }
        
        private (string[] bricks, SampleGrid sample) DoorOnlyPattern2()
        {
            var bricks = new string[] { "b212", "b412", "void", "doorEven" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var doorPattern = TranslatePattern(new Patterns()
            {
                (t["doorEven"], new Vector3Int(6, 0, 0)),
                
                (t["b412"], new Vector3Int(0,0,0)),
                (t["b212"], new Vector3Int(0,unit,0)),
                (t["b412"], new Vector3Int(0,2*unit,0)),
                (t["b212"], new Vector3Int(0,3*unit,0)),
                (t["b412"], new Vector3Int(0,4*unit,0)),
                (t["b212"], new Vector3Int(0,5*unit,0)),
                (t["b412"], new Vector3Int(0,6*unit,0)),
                (t["b212"], new Vector3Int(0,7*unit,0)),
                
                (t["b212"], new Vector3Int(4,0,0)),
                (t["b412"], new Vector3Int(2,unit,0)),
                (t["b212"], new Vector3Int(4,2*unit,0)),
                (t["b412"], new Vector3Int(2,3*unit,0)),
                (t["b212"], new Vector3Int(4,4*unit,0)),
                (t["b412"], new Vector3Int(2,5*unit,0)),
                (t["b412"], new Vector3Int(4,6*unit,0)),
                (t["b412"], new Vector3Int(2,7*unit,0)),
                
                (t["b212"], new Vector3Int(10,0,0)),
                (t["b412"], new Vector3Int(10,unit,0)),
                (t["b212"], new Vector3Int(10,2*unit,0)),
                (t["b412"], new Vector3Int(10,3*unit,0)),
                (t["b212"], new Vector3Int(10,4*unit,0)),
                (t["b412"], new Vector3Int(10,5*unit,0)),
                (t["b412"], new Vector3Int(8,6*unit,0)),
                
                (t["b412"], new Vector3Int(6,7*unit,0)),
                (t["b212"], new Vector3Int(10,7*unit,0)),

            }, new Vector3Int(0,0,1));
            LayerAdd(ref doorPattern, new Range3D(0,16,0,6*unit, 0,1), t["void"]); // Layer in front.
            LayerAdd(ref doorPattern, new Range3D(0,16,0,7*unit, 3,4), t["void"]); // Layer in back.

            var doorSample = ToSampleGrid(doorPattern, legoTiles, fillWithVoids: false);
            return (bricks, doorSample);
        }
        private (string[] bricks, SampleGrid sample) GetEvenDoorPattern()
        {
            var bricks = new string[] { "b412", "b212", "void", "doorEven" };
            var (unit, legoTiles, t) = PatternData(bricks);
            
            /*
             * Surrounds a door with bricks. Door has slight inset.
             */
            var doorPattern = TranslatePattern(new Patterns()
            {
                (t["doorEven"], new Vector3Int(4, 0, 0)),
                (t["doorEven"], new Vector3Int(4, 0, 1)),
                
                (t["b412"], new Vector3Int(0,1*unit,0)),
                (t["b412"], new Vector3Int(0,3*unit,0)),
                (t["b412"], new Vector3Int(0,5*unit,0)),
                
                (t["b412"], new Vector3Int(2,6*unit,0)),
                (t["b412"], new Vector3Int(6,6*unit,0)),
                
                (t["b412"], new Vector3Int(8,1*unit,0)),
                (t["b412"], new Vector3Int(8,3*unit,0)),
                (t["b412"], new Vector3Int(8,5*unit,0)),
                
                (t["b212"], new Vector3Int(2,0,0)),
                (t["b212"], new Vector3Int(2,2*unit,0)),
                (t["b212"], new Vector3Int(2,4*unit,0)),
                
                (t["b212"], new Vector3Int(8,0,0)),
                (t["b212"], new Vector3Int(8,2*unit,0)),
                (t["b212"], new Vector3Int(8,4*unit,0)),
                
            }, new Vector3Int(0,0,1));
            LayerAdd(ref doorPattern, new Range3D(0,10,0,6*unit, 0,1), t["void"]); // Layer in front.
            LayerAdd(ref doorPattern, new Range3D(0,10,0,7*unit, 3,4), t["void"]); // Layer in back.

            var doorSample = ToSampleGrid(doorPattern, legoTiles, fillWithVoids: false);
            return (bricks, doorSample);
        }
        
        private (string[] bricks, SampleGrid cornerSample) GetCornerPattern(bool includeVoidLayer=true)
        {
            var bricks = new string[] { "b412", "b214", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
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
            }, includeVoidLayer ? new Vector3Int(1,0,1) : new Vector3Int());
            
            var cornerSample = ToSampleGrid(corners, legoTiles, extraLayer:includeVoidLayer ? new Vector3Int(1,0,1) : null);
            return (bricks, cornerSample);
        }
        
        private (string[] bricks, SampleGrid cornerSample) GetCornerPatternOdd(bool includeVoidLayer=true)
        {
            var bricks = new string[] { "b312", "b213", "b112", "b211", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var corners = TranslatePattern(new Patterns()
            {
                (t["b312"], new Vector3Int(0,0,0)),
                (t["b312"], new Vector3Int(3,0,0)),
                
                (t["b213"], new Vector3Int(6,0,0)),
                (t["b213"], new Vector3Int(6,0,3)),
                
                (t["b312"], new Vector3Int(2,0,6)),
                (t["b312"], new Vector3Int(5,0,6)),
                
                (t["b213"], new Vector3Int(0,0,2)),
                (t["b213"], new Vector3Int(0,0,5)),
                
                
                (t["b213"], new Vector3Int(0,unit,0)),
                (t["b213"], new Vector3Int(0,unit,3)),
                
                (t["b312"], new Vector3Int(0,unit,6)),
                (t["b312"], new Vector3Int(3,unit,6)),
                
                (t["b213"], new Vector3Int(6,unit,5)),
                (t["b213"], new Vector3Int(6,unit,2)),
                
                (t["b312"], new Vector3Int(2,unit,0)),
                (t["b312"], new Vector3Int(5,unit,0)),
                
                (t["b312"], new Vector3Int(0,2*unit,0)),
                (t["b312"], new Vector3Int(3,2*unit,0)),
                
                (t["b213"], new Vector3Int(6,2*unit,0)),
                (t["b213"], new Vector3Int(6,2*unit,3)),
                
                (t["b312"], new Vector3Int(2,2*unit,6)),
                (t["b312"], new Vector3Int(5,2*unit,6)),
                
                (t["b213"], new Vector3Int(0,2*unit,2)),
                (t["b213"], new Vector3Int(0,2*unit,5)),
            }, includeVoidLayer ? new Vector3Int(1,0,1) : new Vector3Int());
            
            var cornerSample = ToSampleGrid(corners, legoTiles, extraLayer:includeVoidLayer ? new Vector3Int(1,0,1) : null);
            return (bricks, cornerSample);
        }

        private (string[] bricks, SampleGrid voids) GetVoidPattern()
        {           
            var bricks = new string[] { "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var voids = ToSampleGrid(new Patterns(), legoTiles, new Vector3Int(2, 2, 2));
            return (bricks, voids);
        }

        private (string[] bricks, SampleGrid brickPattern214Grid) Get214BrickPattern()
        {
            var bricks = new string[] { "b214","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
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
            return (bricks, brickPattern214Grid);
        }

        private (string[] bricks, SampleGrid brickPattern412Grid) Get412BrickPattern()
        {
            var bricks = new string[] { "b412", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            
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

            return (bricks, brickPattern412Grid);
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