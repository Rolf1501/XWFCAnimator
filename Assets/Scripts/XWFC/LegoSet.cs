using System.Collections.Generic;
using System.Linq;
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
            var windowMask = Util.Populate3D(8, 4, 2, true);
            windowMask[0, 0, 0] = false;
            windowMask[0, 0, 1] = false;
            windowMask[0, 7, 0] = false;
            windowMask[0, 7, 1] = false;
            windowMask[2, 0, 0] = false;
            windowMask[2, 0, 1] = false;
            windowMask[2, 7, 0] = false;
            windowMask[2, 7, 1] = false;
            
            var windowMaskSmall = Util.Populate3D(5, 3, 2, true);
            windowMaskSmall[0, 0, 0] = false;
            windowMaskSmall[0, 0, 1] = false;
            windowMaskSmall[0, 4, 0] = false;
            windowMaskSmall[0, 4, 1] = false;
            windowMaskSmall[2, 0, 0] = false;
            windowMaskSmall[2, 0, 1] = false;
            windowMaskSmall[2, 4, 0] = false;
            windowMaskSmall[2, 4, 1] = false;
            
            var windowMaskLarge = Util.Populate3D(8,5,2, true);
            windowMaskLarge[0, 0, 0] = false;
            windowMaskLarge[0, 7, 0] = false;
            windowMaskLarge[3, 0, 0] = false;
            windowMaskLarge[4, 0, 0] = false;
            windowMaskLarge[4, 1, 0] = false;
            windowMaskLarge[3, 7, 0] = false;
            windowMaskLarge[4, 6, 0] = false;
            windowMaskLarge[4, 7, 0] = false;
            windowMaskLarge[0, 0, 1] = false;
            windowMaskLarge[0, 7, 1] = false;
            windowMaskLarge[3, 0, 1] = false;
            windowMaskLarge[4, 0, 1] = false;
            windowMaskLarge[4, 1, 1] = false;
            windowMaskLarge[3, 7, 1] = false;
            windowMaskLarge[4, 6, 1] = false;
            windowMaskLarge[4, 7, 1] = false;

            var wheelMask = Util.Populate3D(3, 3 * unit, 2, true);
            wheelMask[0, 0, 0] = false;
            wheelMask[0, 0, 1] = false;
            wheelMask[0, 2, 0] = false;
            wheelMask[0, 2, 1] = false;
            wheelMask[3*unit-1, 0, 0] = false;
            wheelMask[3*unit-1, 0, 1] = false;
            wheelMask[3*unit-1, 2, 0] = false;
            wheelMask[3*unit-1, 2, 1] = false;

            var poleLeft = Util.Populate3D(2, 3 * unit, 1, true);
            poleLeft[0, 1, 0] = false;
            poleLeft[unit, 1, 0] = false;
            
            var poleRight = Util.Populate3D(2, 3 * unit, 1, true);
            poleRight[0, 0, 0] = false;
            poleRight[unit, 0, 0] = false;

            
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
                    "b611",
                    new Vector3Int(6,1,1),
                    new Color32(242, 205, 55, 255)
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
                    "b311",
                    new Vector3Int(3,unit,1),
                    new Color32(242, 243, 242, 255)
                ),
                new(
                    "b115",
                    new Vector3Int(1,unit,5),
                    new Color32(242, 243, 242, 255)
                ),
                new(
                    "b115b",
                    new Vector3Int(1,2*unit,4),
                    new Color32(242, 243, 242, 255)
                ),
                // Bricks 90deg
                new(
                    "b211",
                    new Vector3Int(2,unit,1),
                    new Color32(35, 120, 65, 255)
                ),
                
                new(
                    "b231",
                    new Vector3Int(2,3*unit,1),
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
                // new(
                //     "windowLarge",
                //     new Vector3Int(),
                //     new Color(0.2f,0.1f,1f,0.7f),
                //     mask: windowMaskLarge
                //     ),
                new(
                    "window",
                    new Vector3Int(),
                    new Color(0.2f,0.1f,1f,0.7f),
                    mask: windowMask
                ),
                
                new(
                    "window112",
                    new Vector3Int(1,unit,2),
                    new Color(0.2f,0.1f,1f,0.7f)),
                new(
                    "window211",
                    new Vector3Int(2,unit,1),
                    new Color(0.2f,0.1f,1f,0.7f)),
                
                new(
                    "windowSmall",
                    new Vector3Int(),
                    new Color(0.2f,0.1f,1f,0.7f),
                    mask: windowMaskSmall
                ),
                
                // new(
                //     "poleLeft",
                //     new Vector3Int(2,3*unit,1),
                //     new Color(1,0,0),
                //     mask: poleLeft),
                //
                //
                // new(
                //     "poleRight",
                //     new Vector3Int(2,3*unit,1),
                //     new Color(1,0,1),
                //     mask: poleRight),
                
                // Plates
                
                new(
                    "p111",
                    new Vector3Int(1,1,1),
                    new Color(1,0,0)
                ),
                new(
                    "p112",
                    new Vector3Int(1,1,2),
                    new Color(1,1,0)
                ),
                new(
                    "p212",
                    new Vector3Int(2,1,2),
                    new Color(0.6f,0.6f,0)
                ),
                new(
                    "p412",
                    new Vector3Int(4,1,2),
                    new Color(1,1,0)
                    ),
                
                new(
                    "p612",
                    new Vector3Int(6,1,2),
                    new Color(0.99f,0.7f,0)
                ),
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
                
                new(
                    "wheel",
                    new Vector3Int(3, 3*unit, 2),
                    new Color(0.2f, 0.2f, 0.2f),
                    mask:wheelMask
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
                // new(
                //     "windowSplit",
                //     new Vector3Int(4,5*unit,2),
                //     new Color(0, 0, 0.8f),
                //     mask: new bool[,,]
                //     {
                //         {
                //             {false,false},{true,true},{true,true},{false,false},
                //             
                //         },
                //         {
                //             {true,true},{true,true},{true,true},{true,true},
                //         },
                //         {
                //             {false,false},{false,false},{false,false},{false,false},
                //         },
                //         {
                //             {true,true},{true,true},{true,true},{true,true},
                //         },
                //         {
                //             {false,false},{true,true},{true,true},{false,false},
                //             
                //         },
                //     }
                //     ),
                
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

        public static Component[] SimpleHouseExample()
        {
            var set = new LegoSet(false);

            var (t, s) = set.SimpleHouse();

            var weights = new Dictionary<string, float>();
            foreach (var (tKey, value) in t)
            {
                weights[value.UniformAtomValue] = 1;
            }

            weights["door"] = 500;
        
            var unit = LegoSet.BrickUnitSize(set.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
        
            var c = new Component(
                new Vector3Int(0,0,0), 
                new Vector3Int(14,8,14), 
                t, s.ToArray(),
                tileWeights:weights,
                customSeed:1166925486
                // 569
            );
        
        
        
            var components = new[] { c }; //  

            return components;
        }

        public static Component[] LegoHouse2D()
        {
            var lego = new LegoSet(false);

            var (tDoor, sDoor) = lego.DoorEvenExample();
            var (tWindow, sWindow) = lego.WindowExample2D();
            var (tBalcony, sBalcony) = lego.BalconyExample2D();
            var (tRoof, sRoof) = lego.RoofExample();
            var (tChimney, sChimney) = lego.ChimneyExample2D();
            
            var unit = LegoSet.BrickUnitSize(lego.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);
            
            var oDoor = new Vector3Int(6, 0, 4);
            var eDoor = new Vector3Int(40, 10, 2) * unitV;
            
            var oWindow = new Vector3Int(0, eDoor.y, 0) + oDoor;
            var eWindow = new Vector3Int(40, 10, 2) * unitV;
            
            var eBalcony = new Vector3Int(24, 8, 6) * unitV;
            var oBalcony = new Vector3Int((int)(0.5*(eWindow.x - eBalcony.x)),eWindow.y, -4) + oWindow;
            
            var oRoof = new Vector3Int(-12, eBalcony.y, 4) + oBalcony;
            var eRoof = new Vector3Int(50, 22, 2) * unitV;
            
            var oChimney = new Vector3Int(35, eRoof.y, 0) + oRoof;
            var eChimney = new Vector3Int(6, 12, 2) * unitV;
            
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
                customSeed:2394
            );
            
            var cBalcony = new Component(
                oBalcony, 
                eBalcony,
                tBalcony, sBalcony.ToArray(),
                customSeed: 474596326
            );
            
            var cRoof = new Component(
                oRoof, 
                eRoof,
                tRoof, sRoof.ToArray(),
                customSeed:558,
                offsetMode:OffsetMode.Max
            );
            
            var cChimney = new Component(
                oChimney, 
                eChimney, 
                tChimney, sChimney.ToArray(),
                customSeed:20,
                offsetMode:OffsetMode.Max
            );
            
            // var components = new[] { cBalcony }; //  
            var components = new[] { cDoor, cWindow,cBalcony, cRoof,cChimney }; //  

            return components;
        }

        public static Component[] LegoHouse()
        {
            var lego = new LegoSet(false);
            // var (t, samples) = lego.WallPerimeter3DExample();
            // var (t, samples) = lego.DoorExample();
            // var (t, samples) = lego.WallExample();
            // var (t, samples) = lego.DoorOddExample();
            var (tDoor, sDoor) = lego.DoorEvenExample();
            var (tWindow, sWindow) = lego.WindowExample();
            var (tBalcony, sBalcony) = lego.BalconyExample();
            var (tRoof, sRoof) = lego.RoofExample();
            var (tChimney, sChimney) = lego.ChimneyExample();
            // var (t, p) = LegoSet.StackedBricksExample();

            var unit = LegoSet.BrickUnitSize(lego.PlateAtoms);
            var unitV = new Vector3Int(1, unit, 1);

            var oDoor = new Vector3Int(6, 0, 6);
            var eDoor = new Vector3Int(40, 10, 14) * unitV;
            
            var oWindow = new Vector3Int(0, eDoor.y, 0) + oDoor;
            var eWindow = new Vector3Int(40, 10, 14) * unitV;
            
            var oBalcony = new Vector3Int(8,eWindow.y, -4) + oWindow;
            var eBalcony = new Vector3Int(24, 8, 18) * unitV;
            
            var oRoof = new Vector3Int(-14, eBalcony.y, 4) + oBalcony;
            var eRoof = new Vector3Int(50, 22, 14) * unitV;
            
            var oChimney = new Vector3Int(35, eRoof.y, 3) + oRoof;
            var eChimney = new Vector3Int(6, 12, 6) * unitV;
            
            var cDoor = new Component(
                oDoor, 
                eDoor, 
                tDoor, sDoor.ToArray(),
                customSeed:1520751262
                );
            
            var cWindow = new Component(
                oWindow, 
                eWindow, 
                tWindow, sWindow.ToArray(),
                customSeed:38
                );
            
            var cBalcony = new Component(
                oBalcony, 
                eBalcony,
                tBalcony, sBalcony.ToArray(),
                customSeed: 38
                );
            
            var cRoof = new Component(
                oRoof, 
                eRoof,
                tRoof, sRoof.ToArray(),
                customSeed:20058,
                offsetMode:OffsetMode.Max
                );
            
            var cChimney = new Component(
                oChimney, 
                eChimney, 
                tChimney, sChimney.ToArray(),
                customSeed:20,
                offsetMode:OffsetMode.Max
                );
            
            // var oDoor = new Vector3Int(6, 0, 6);
            // var eDoor = new Vector3Int(10, 10, 2) * unitV;
            // // var eDoor = new Vector3Int(30, 10, 2) * unitV;
            //
            // var oWindow = new Vector3Int(0, eDoor.y, 0) + oDoor;
            // var eWindow = new Vector3Int(10, 10, 2) * unitV;
            // // var eWindow = new Vector3Int(20, 10, 2) * unitV;
            //
            // var oBalcony = new Vector3Int(0,eWindow.y, -6) + oWindow;
            // var eBalcony = new Vector3Int(4, 8, 8) * unitV;
            // // var eBalcony = new Vector3Int(20, 8, 8) * unitV;
            //
            // var oRoof = new Vector3Int(-7, eBalcony.y, 6) + oBalcony;
            // var eRoof = new Vector3Int(10, 4, 2) * unitV;
            // // var eRoof = new Vector3Int(40, 15, 2) * unitV;
            //
            // var oChimney = new Vector3Int(25, eRoof.y, 0) + oRoof;
            // var eChimney = new Vector3Int(4, 8, 2) * unitV;
            // // var eChimney = new Vector3Int(4, 8, 2) * unitV;
            //
            // var cDoor = new Component(
            //     oDoor, 
            //     eDoor, 
            //     tDoor, sDoor.ToArray(),
            //     customSeed:26
            // );
            //
            // var cWindow = new Component(
            //     oWindow, 
            //     eWindow, 
            //     tWindow, sWindow.ToArray(),
            //     customSeed:20
            // );
            //
            // var cBalcony = new Component(
            //     oBalcony, 
            //     eBalcony,
            //     tBalcony, sBalcony.ToArray(),
            //     customSeed: 1340540562
            // );
            //
            // var cRoof = new Component(
            //     oRoof, 
            //     eRoof,
            //     tRoof, sRoof.ToArray(),
            //     customSeed:20
            // );
            //
            // var cChimney = new Component(
            //     oChimney, 
            //     eChimney, 
            //     tChimney, sChimney.ToArray(),
            //     customSeed:20
            // );

            // var components = new[] { cBalcony }; //   
            
            var components = new[] { cDoor, cWindow,cBalcony, cRoof,cChimney }; //   

            return components;
        }

        public static Component[] LegoTrain()
        {
            var plateAtoms = true;
            var (t, x) = new LegoSet(plateAtoms).Wheel();
            var (tc, xc) = new LegoSet(plateAtoms).TrainChassis();
            var (tb, xb) = new LegoSet(plateAtoms).TrainBase();
            var (tco, xco) = new LegoSet(plateAtoms).Cockpit();
            var (ts, xs) = new LegoSet(plateAtoms).TrainSteam();
            var (ts2, xs2) = new LegoSet(plateAtoms).TrainSteam2();
            var (tg, xg) = new LegoSet(plateAtoms).TrainGuard();
            var unit = LegoSet.BrickUnitSize(plateAtoms);
            var components = new[]
            {
                // Chassis
                new Component(new Vector3Int(0,unit+1,2), new Vector3Int(16, 3*unit, 6), tc, xc.ToArray()),
                
                // Wheels
                new Component(new Vector3Int(2,0,0), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                new Component(new Vector3Int(7,0,0), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                new Component(new Vector3Int(12,0,0), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                new Component(new Vector3Int(2,0,8), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                new Component(new Vector3Int(7,0,8), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                new Component(new Vector3Int(12,0,8), new Vector3Int(3, 3*unit, 2), t, x.ToArray()),
                
                // Base
                new Component(new Vector3Int(0,3*unit+3+1,1), new Vector3Int(16, 3*unit, 8), tb, xb.ToArray()),
                
                // Cockpit
                new Component(new Vector3Int(0,6*unit+3+1,1), new Vector3Int(10, 5*unit, 8), tco, xco.ToArray(), customSeed:288),
                
                // Train steam
                new Component(new Vector3Int(10,6*unit+3+1,1), new Vector3Int(6, unit, 8), ts, xs.ToArray(), customSeed:288),
                new Component(new Vector3Int(11,6*unit+3+1+2,3), new Vector3Int(4, 5*unit, 4), ts2, xs2.ToArray(), customSeed:204),
                
                // Guard
                new Component(new Vector3Int(16,1,1), new Vector3Int(3, 2*unit+1, 8), tg, xg.ToArray(), customSeed:127),
            };

            return components;
        }
        public (TileSet t, List<SampleGrid>) WindowExample()
        {
            var oddBricks = GetOddBrickPattern();
            var window = GetWindowPattern();
            var smallWindow = GetSmallWindowPattern();
            var curl = GetOddCurlPattern();
            var oddCorners = GetCornerPatternOdd(false);

            var patterns = new[] { oddBricks, window, smallWindow,oddCorners, curl };
            
            return ExtractTilesAndSamples(patterns); 
        }
        
        public (TileSet t, List<SampleGrid>) WindowExample2D()
        {
            var oddBricks = GetOddBrickPattern2D();
            var window = GetWindowPattern();
            var smallWindow = GetSmallWindowPattern();
            var curl = GetOddCurlPattern();
            // var oddCorners = GetCornerPatternOdd(false);

            var patterns = new[] { oddBricks, window, smallWindow, curl };
            
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
            var b211 = Get211Pattern();
            var b112 = Get112Pattern();
            var voids = GetVoidPattern();
            var patterns = new[] { chimney, b211, b112, voids };
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) ChimneyExample2D()
        {
            var b211 = Get211Pattern2D();
            // var b112 = Get112Pattern();
            // var voids = GetVoidPattern();
            var patterns = new[] { b211 };
            return ExtractTilesAndSamples(patterns);
        }

        public (TileSet legoTiles, List<SampleGrid> samples) BalconyExample()
        {
            var balcony = GetBalconyPattern();
            var patterns = new[] { balcony };//, brick412, brick214, corner, runningStacked };
            return ExtractTilesAndSamples(patterns);
        }

        public (TileSet legoTiles, List<SampleGrid> samples) BalconyExample2D()
        {
            var balcony = GetBalconyPattern2D();
            var patterns = new[] { balcony };//, brick412, brick214, corner, runningStacked };
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
            // var voids = GetVoidPattern();
            var patterns = new[] { door, corner, brick412, brick214 };
            
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) SimpleHouse()
        {
            var door = DoorOnlyPattern2();
            var corner = GetCornerPattern(false);
            var brick412 = Get412BrickPattern();
            var brick214 = Get214BrickPattern();
            var oddCorner = GetCornerPatternOdd(false);
            var oddSingle = GetCornerPatternOddSingle();
            var evenSingle = GetCornerPatternEvenSingle();
            var oddBrick = GetOddBrickPattern();
            
            var smallWindow = GetSmallWindowPattern();
            var curl = GetOddCurlPattern();

            var patterns = new[] { door, corner, brick412, brick214, oddCorner, oddSingle, evenSingle, oddBrick, smallWindow, curl };
            
            
            return ExtractTilesAndSamples(patterns);
        }

        public (TileSet legoTiles, List<SampleGrid> samples) TrainChassis()
        {
            var trainBase = TrainChassisPattern();
            var patterns = new[] { trainBase };
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) TrainBase()
        {
            // var trainBase = TrainBasePattern();
            // var trainBase2 = TrainBasePattern2();
            // var trainBase3 = TrainBasePattern3();
            // var patterns = new[] { trainBase, trainBase2, trainBase3 };
            var trainBase = TrainBasePatternSimple();
            var patterns = new[] { trainBase };
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) Wheel()
        {
            var wheel = WheelPattern();
            var patterns = new[] { wheel };
            
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) Cockpit()
        {
            var sample = CockpitPattern();
            var patterns = new[] { sample };
            
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) TrainSteam()
        {
            var sample = TrainSteamPattern();
            var patterns = new[] { sample };
            
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) TrainSteam2()
        {
            var sample = TrainSteamPattern2();
            var patterns = new[] { sample };
            
            return ExtractTilesAndSamples(patterns);
        }
        
        public (TileSet legoTiles, List<SampleGrid> samples) TrainGuard()
        {
            var sample = TrainGuardPattern();
            var patterns = new[] { sample };
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
        
        
        public (string[] bricks, SampleGrid sample) WheelPattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "wheel", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["wheel"], new Vector3Int(0, 0, 0)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true);
            return (bricks, grid);
        }
        
        public (string[] bricks, SampleGrid sample) CockpitPattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "window211", "window112", "b111", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["b111"], new Vector3Int(0, 0, 0)),
                (t["b111"], new Vector3Int(0, unit, 0)),
                
                (t["window211"], new Vector3Int(1, 0, 0)),
                (t["window211"], new Vector3Int(1, unit, 0)),
                
                (t["window211"], new Vector3Int(3, 0, 0)),
                (t["window211"], new Vector3Int(3, unit, 0)),
                
                (t["b111"], new Vector3Int(5, 0, 0)),
                (t["b111"], new Vector3Int(5, unit, 0)),
                
                (t["b111"], new Vector3Int(0, 0, 5)),
                (t["b111"], new Vector3Int(0, unit, 5)),
                
                (t["window211"], new Vector3Int(1, 0, 5)),
                (t["window211"], new Vector3Int(1, unit, 5)),
                
                (t["window211"], new Vector3Int(3, 0, 5)),
                (t["window211"], new Vector3Int(3, unit, 5)),
                
                (t["b111"], new Vector3Int(5, 0, 5)),
                (t["b111"], new Vector3Int(5, unit, 5)),
                
                (t["window112"], new Vector3Int(5, 0, 1)),
                (t["window112"], new Vector3Int(5, unit, 1)),
                
                (t["window112"], new Vector3Int(5, 0, 3)),
                (t["window112"], new Vector3Int(5, unit, 3)),
                
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true);
            return (bricks, grid);
        }

        public (string[] bricks, SampleGrid sample) TrainSteamPattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "p612", "p412", "p212", "b212", "b412", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["p612"], new Vector3Int(0, 0, 0)),
                (t["p612"], new Vector3Int(0, 0, 2)),
                
                (t["p412"], new Vector3Int(0, 1, 0)),
                (t["p412"], new Vector3Int(0, 1, 2)),
                
                (t["p212"], new Vector3Int(0, 2, 0)),
                (t["p212"], new Vector3Int(0, 2, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true, extraLayer:new Vector3Int(0,1,0));
            return (bricks, grid);
        }
        
        public (string[] bricks, SampleGrid sample) TrainSteamPattern2()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b212", "b412", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["b212"], new Vector3Int(1, 0, 1)),
                (t["b212"], new Vector3Int(1, unit, 1)),
                (t["b412"], new Vector3Int(0, 2*unit, 0)),
                (t["b412"], new Vector3Int(0, 2*unit, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true);
            return (bricks, grid);
        }
        
        public (string[] bricks, SampleGrid sample) TrainGuardPattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b112", "b212", "p212", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["b112"], new Vector3Int(0, unit+1, 0)),
                (t["b212"], new Vector3Int(0, 1, 0)),
                (t["p212"], new Vector3Int(1, 0, 0)),
                
                (t["b112"], new Vector3Int(0, unit+1, 2)),
                (t["b212"], new Vector3Int(0, 1, 2)),
                (t["p212"], new Vector3Int(1, 0, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true);
            return (bricks, grid);
        }
        public (string[] bricks, SampleGrid sample) TrainChassisPattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "p412", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var stackedPattern = new Patterns()
            {
                (t["p412"], new Vector3Int(0, 0, 0)),
                (t["p412"], new Vector3Int(4, 0, 0)),
                (t["p412"], new Vector3Int(0, 0, 2)),
                (t["p412"], new Vector3Int(4, 0, 2)),
            };
            var grid = ToSampleGrid(stackedPattern, legoTiles, true, extraLayer:new Vector3Int(0,2,0));
            return (bricks, grid);
        }
        
        public (string[] bricks, SampleGrid sample) TrainBasePattern()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b212", "p112", "p212","void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var pattern = new Patterns()
            {
                (t["p112"], new Vector3Int(0, 0, 0)),
                (t["p212"], new Vector3Int(1, 0, 0)),
                (t["p212"], new Vector3Int(3, 0, 0)),
                (t["p112"], new Vector3Int(5, 0, 0)),
                
                (t["p112"], new Vector3Int(0, unit+1, 0)),
                (t["p212"], new Vector3Int(1, unit+1, 0)),
                (t["p212"], new Vector3Int(3, unit+1, 0)),
                (t["p112"], new Vector3Int(5, unit+1, 0)),
                
                (t["b212"], new Vector3Int(0, 1, 0)),
                (t["b212"], new Vector3Int(2, 1, 0)),
                (t["b212"], new Vector3Int(4, 1, 0)),
                
                (t["p112"], new Vector3Int(0, 0, 2)),
                (t["p212"], new Vector3Int(1, 0, 2)),
                (t["p212"], new Vector3Int(3, 0, 2)),
                (t["p112"], new Vector3Int(5, 0, 2)),
                
                (t["p112"], new Vector3Int(0, unit+1, 2)),
                (t["p212"], new Vector3Int(1, unit+1, 2)),
                (t["p212"], new Vector3Int(3, unit+1, 2)),
                (t["p112"], new Vector3Int(5, unit+1, 2)),
                
                
                
                (t["b212"], new Vector3Int(0, 1, 2)),
                (t["b212"], new Vector3Int(2, 1, 2)),
                (t["b212"], new Vector3Int(4, 1, 2)),
            };
            var grid = ToSampleGrid(pattern, legoTiles, true, extraLayer:new Vector3Int(0,1,0));
            return (bricks, grid);
        }
        
        
        
        public (string[] bricks, SampleGrid sample) TrainBasePatternSimple()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b111", "p111", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var pattern = new Patterns()
            {
                // (t["b111"], new Vector3Int(0, 1, 0)),
                // (t["b111"], new Vector3Int(1, 1, 0)),
                // (t["b111"], new Vector3Int(2, 1, 0)),
                // (t["b111"], new Vector3Int(3, 1, 0)),
                //
                // (t["p111"], new Vector3Int(0, 0, 0)),
                // (t["p111"], new Vector3Int(3, 0, 0)),
                // (t["p111"], new Vector3Int(0, unit + 1, 0)),
                // (t["p111"], new Vector3Int(3, unit + 1, 0)),
                //
                // (t["b111"], new Vector3Int(0, 1, 1)),
                // (t["b111"], new Vector3Int(1, 1, 1)),
                // (t["b111"], new Vector3Int(2, 1, 1)),
                // (t["b111"], new Vector3Int(3, 1, 1)),
                //
                // (t["p111"], new Vector3Int(0, 0, 1)),
                // (t["p111"], new Vector3Int(3, 0, 1)),
                // (t["p111"], new Vector3Int(0, unit + 1, 1)),
                // (t["p111"], new Vector3Int(3, unit + 1, 1))
                
                (t["p111"], new Vector3Int(0, 0, 0)),
                (t["p111"], new Vector3Int(1, 0, 0)),
                (t["p111"], new Vector3Int(0, 0, 1)),
                (t["p111"], new Vector3Int(1, 0, 1)),
                (t["p111"], new Vector3Int(0, 1, 0)),
                (t["p111"], new Vector3Int(1, 1, 0)),
                (t["p111"], new Vector3Int(0, 1, 1)),
                (t["p111"], new Vector3Int(1, 1, 1)),
            };
                
            var grid = ToSampleGrid(pattern, legoTiles, true);
            return (bricks, grid);
        }
        
        
        
        public (string[] bricks, SampleGrid sample) TrainBasePattern2()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b212", "p112", "p212","void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var pattern = TranslatePattern(new Patterns()
            {
                (t["b212"], new Vector3Int(7,1, 0)),
                (t["b212"], new Vector3Int(9,1, 0)),
                (t["b212"], new Vector3Int(11,1, 0)),
                
                (t["p212"], new Vector3Int(9,0, 0)),
                (t["p112"], new Vector3Int(8,0, 0)),
                (t["p112"], new Vector3Int(11,0, 0)),
                
                (t["p212"], new Vector3Int(7,unit+1, 0)),
                (t["p212"], new Vector3Int(9,unit+1, 0)),
                (t["p212"], new Vector3Int(11,unit+1, 0)),
                
                (t["b212"], new Vector3Int(7,1, 2)),
                (t["b212"], new Vector3Int(9,1, 2)),
                (t["b212"], new Vector3Int(11,1, 2)),
                
                (t["p212"], new Vector3Int(9,0, 2)),
                (t["p112"], new Vector3Int(8,0, 2)),
                (t["p112"], new Vector3Int(11,0, 2)),
                
                (t["p212"], new Vector3Int(7,unit+1, 2)),
                (t["p212"], new Vector3Int(9,unit+1, 2)),
                (t["p212"], new Vector3Int(11,unit+1, 2)),
            }, new Vector3Int(-7,0,0));
            var grid = ToSampleGrid(pattern, legoTiles, true, extraLayer:new Vector3Int(0,1,0));
            return (bricks, grid);
        }
        
        public (string[] bricks, SampleGrid sample) TrainBasePattern3()
        {
            PlateAtoms = true;
            var bricks = new string[] { "b212", "p112", "p212","void" };
            var (unit, legoTiles, t) = PatternData(bricks);

            var pattern = TranslatePattern(new Patterns()
            {
                (t["b212"], new Vector3Int(14,1, 0)),
                (t["b212"], new Vector3Int(16,1, 0)),
                (t["b212"], new Vector3Int(18,1, 0)),
                (t["b212"], new Vector3Int(20,1, 0)),
                
                
                (t["p112"], new Vector3Int(14,0, 0)),
                (t["p112"], new Vector3Int(16,0, 0)),
                (t["p112"], new Vector3Int(19,0, 0)),
                (t["p112"], new Vector3Int(21,0, 0)),
                
                
                
                (t["b212"], new Vector3Int(14,1, 2)),
                (t["b212"], new Vector3Int(16,1, 2)),
                (t["b212"], new Vector3Int(18,1, 2)),
                (t["b212"], new Vector3Int(20,1, 2)),
                
                (t["p112"], new Vector3Int(14,0, 2)),
                (t["p112"], new Vector3Int(16,0, 2)),
                (t["p112"], new Vector3Int(19,0, 2)),
                (t["p112"], new Vector3Int(21,0, 2)),
            }, new Vector3Int(-14,0,0));
            var grid = ToSampleGrid(pattern, legoTiles, true, extraLayer:new Vector3Int(0,1,0));
            return (bricks, grid);
        }
        
        private (string[] bricks, SampleGrid sample) GetBalconyPattern()
        {
            var bricks = new string[] { "b211", "b111", "b112", "b115", "b216", "b412","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                // (t["b412"], new Vector3Int(0,0,4)),
                // (t["b412"], new Vector3Int(0,unit,4)),
                // (t["b412"], new Vector3Int(0,2*unit,4)),
                // (t["b412"], new Vector3Int(0,3*unit,4)),
                // (t["b412"], new Vector3Int(0,4*unit,4)),
                // (t["b412"], new Vector3Int(0,5*unit,4)),
                // (t["b412"], new Vector3Int(0,6*unit,4)),
                //
                // (t["b412"], new Vector3Int(12,0,4)),
                // (t["b412"], new Vector3Int(12,unit,4)),
                // (t["b412"], new Vector3Int(12,2*unit,4)),
                // (t["b412"], new Vector3Int(12,3*unit,4)),
                // (t["b412"], new Vector3Int(12,4*unit,4)),
                // (t["b412"], new Vector3Int(12,5*unit,4)),
                // (t["b412"], new Vector3Int(12,6*unit,4)),
                //
                // (t["b212"], new Vector3Int(4,5*unit,4)),
                // (t["b212"], new Vector3Int(4,6*unit,4)),
                // (t["b212"], new Vector3Int(6,5*unit,4)),
                // (t["b212"], new Vector3Int(6,6*unit,4)),
                // (t["b212"], new Vector3Int(8,5*unit,4)),
                // (t["b212"], new Vector3Int(8,6*unit,4)),
                // (t["b212"], new Vector3Int(10,5*unit,4)),
                // (t["b212"], new Vector3Int(10,6*unit,4)),
                //
                // (t["b412"], new Vector3Int(0,0, 6)),
                // (t["b412"], new Vector3Int(0,unit, 6)),
                // (t["b412"], new Vector3Int(0,2*unit, 6)),
                // (t["b412"], new Vector3Int(0,3*unit, 6)),
                // (t["b412"], new Vector3Int(0,4*unit, 6)),
                // (t["b412"], new Vector3Int(0,5*unit, 6)),
                // (t["b412"], new Vector3Int(0,6*unit, 6)),
                //
                // (t["b412"], new Vector3Int(12,0, 6)),
                // (t["b412"], new Vector3Int(12,unit, 6)),
                // (t["b412"], new Vector3Int(12,2*unit, 6)),
                // (t["b412"], new Vector3Int(12,3*unit, 6)),
                // (t["b412"], new Vector3Int(12,4*unit, 6)),
                // (t["b412"], new Vector3Int(12,5*unit, 6)),
                // (t["b412"], new Vector3Int(12,6*unit, 6)),
                //
                // (t["b212"], new Vector3Int(4,5*unit, 6)),
                // (t["b212"], new Vector3Int(4,6*unit, 6)),
                // (t["b212"], new Vector3Int(8,5*unit, 6)),
                // (t["b212"], new Vector3Int(8,6*unit, 6)),
                // (t["b212"], new Vector3Int(6,5*unit, 6)),
                // (t["b212"], new Vector3Int(6,6*unit, 6)),
                // (t["b212"], new Vector3Int(10,5*unit, 6)),
                // (t["b212"], new Vector3Int(10,6*unit, 6)),
                //
                // (t["b216"], new Vector3Int(4,0,0)),
                // (t["b216"], new Vector3Int(6,0,0)),
                // (t["b216"], new Vector3Int(8,0,0)),
                // (t["b216"], new Vector3Int(10,0,0)),
                //
                // (t["poleLeft"], new Vector3Int(4,unit,0)),
                // (t["poleRight"], new Vector3Int(10,unit,0)),
                // (t["b231"], new Vector3Int(6,unit,0)),
                // (t["b231"], new Vector3Int(8,unit,0)),
                // (t["b115"], new Vector3Int(4,2*unit,1)),
                // (t["b115b"], new Vector3Int(11,2*unit,1)),
                
                
                
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
                
                (t["b412"], new Vector3Int(0,0,6)),
                (t["b412"], new Vector3Int(0,unit,6)),
                (t["b412"], new Vector3Int(0,2*unit,6)),
                (t["b412"], new Vector3Int(0,3*unit,6)),
                (t["b412"], new Vector3Int(0,4*unit,6)),
                (t["b412"], new Vector3Int(0,5*unit,6)),
                (t["b412"], new Vector3Int(0,6*unit,6)),
                
                (t["b412"], new Vector3Int(4,5*unit,6)),
                (t["b412"], new Vector3Int(4,6*unit,6)),
                
                (t["b412"], new Vector3Int(8,0,6)),
                (t["b412"], new Vector3Int(8,unit,6)),
                (t["b412"], new Vector3Int(8,2*unit,6)),
                (t["b412"], new Vector3Int(8,3*unit,6)),
                (t["b412"], new Vector3Int(8,4*unit,6)),
                (t["b412"], new Vector3Int(8,5*unit,6)),
                (t["b412"], new Vector3Int(8,6*unit,6)),
                
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
                
            }, new Vector3Int(0,0,0));
            // }, new Vector3Int(1,0,1));

            var sample = ToSampleGrid(patterns, legoTiles, true);
            // var sample = ToSampleGrid(patterns, legoTiles, true, extraLayer:new Vector3Int(0,0,1));
            return (bricks, sample);
        }
        
         
        private (string[] bricks, SampleGrid sample) GetBalconyPattern2D()
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
                
            }, new Vector3Int(0,0,0));
            // }, new Vector3Int(1,0,1));

            var sample = ToSampleGrid(patterns, legoTiles, true);
            // var sample = ToSampleGrid(patterns, legoTiles, true, extraLayer:new Vector3Int(0,0,1));
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid cornerSample) GetStackedCornerPattern(bool includeVoidLayer=true)
        {
            var bricks = new string[] { "b412", "b214", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var corners = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(0,0,0)),
                (t["b412"], new Vector3Int(2,0,4)),
                
                (t["b214"], new Vector3Int(0,0,2)),
                (t["b214"], new Vector3Int(4,0,0)),
                
                (t["b412"], new Vector3Int(0,unit,0)),
                (t["b412"], new Vector3Int(2,unit,4)),
                
                (t["b214"], new Vector3Int(0,unit,2)),
                (t["b214"], new Vector3Int(4,unit,0)),
                
                (t["b412"], new Vector3Int(0,2*unit,0)),
                (t["b412"], new Vector3Int(2,2*unit,4)),
                
                (t["b214"], new Vector3Int(0,2*unit,2)),
                (t["b214"], new Vector3Int(4,2*unit,0)),
               
            }, includeVoidLayer ? new Vector3Int(1,0,1) : new Vector3Int());
            
            var cornerSample = ToSampleGrid(corners, legoTiles, extraLayer:includeVoidLayer ? new Vector3Int(1,0,1) : null);
            return (bricks, cornerSample);
        }
        private (string[] bricks, SampleGrid sample) GetRoofPattern()
        {
            var bricks = new string[] { "b412", "b212","b312","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
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
                
                (t["b212"], new Vector3Int(0,unit, 2)),
                (t["b212"], new Vector3Int(1,2*unit, 2)),
                (t["b212"], new Vector3Int(2,3*unit, 2)),
                (t["b212"], new Vector3Int(3,4*unit, 2)),
                
                (t["b212"], new Vector3Int(4,5*unit, 2)),
                (t["b212"], new Vector3Int(6,5*unit, 2)),
                (t["b212"], new Vector3Int(8,5*unit, 2)),
               
                (t["b212"], new Vector3Int(9,4*unit, 2)),
                (t["b212"], new Vector3Int(10,3*unit, 2)),
                (t["b212"], new Vector3Int(11,2*unit, 2)),
                (t["b212"], new Vector3Int(12,unit, 2)),
                
                (t["b312"], new Vector3Int(2,unit, 2)),
                (t["b312"], new Vector3Int(3,2*unit, 2)),
                (t["b312"], new Vector3Int(4,3*unit, 2)),
                
                (t["b312"], new Vector3Int(7,3*unit, 2)),
                (t["b312"], new Vector3Int(8,2*unit, 2)),
                (t["b312"], new Vector3Int(9,unit, 2)),
                
                (t["b412"], new Vector3Int(5,4*unit, 2)),
                
            }, new Vector3Int(0,-unit,0));

            var sample = ToSampleGrid(patterns, legoTiles, true);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetChimneyPattern()
        {
            var bricks = new string[] { "b112", "b211","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b211"], new Vector3Int(0, 0, 0)),
                (t["b112"], new Vector3Int(2, 0, 0)),
                (t["b211"], new Vector3Int(1, 0, 2)),
                (t["b112"], new Vector3Int(0, 0, 1)),
                
                (t["b211"], new Vector3Int(1, unit, 0)),
                (t["b112"], new Vector3Int(0, unit, 0)),
                (t["b211"], new Vector3Int(0, unit, 2)),
                (t["b112"], new Vector3Int(2, unit, 1)),
                
                (t["b211"], new Vector3Int(0, 2*unit, 0)),
                (t["b112"], new Vector3Int(2, 2*unit, 0)),
                (t["b211"], new Vector3Int(1, 2*unit, 2)),
                (t["b112"], new Vector3Int(0, 2*unit, 1)),
                // (t["b112"], new Vector3Int(0, 0, 0)),
                // (t["b112"], new Vector3Int(0, 2*unit, 0)),
                //
                // (t["b112"], new Vector3Int(4, unit, 0)),
                // (t["b112"], new Vector3Int(4, 3*unit, 0)),
                //
                // (t["b212"], new Vector3Int(0, unit, 0)),
                // (t["b212"], new Vector3Int(0, 3*unit, 0)),
                //
                // (t["b212"], new Vector3Int(2, unit, 0)),
                // (t["b212"], new Vector3Int(2, 3*unit, 0)),
                //
                // (t["b212"], new Vector3Int(1, 0, 0)),
                // (t["b212"], new Vector3Int(1, 2*unit, 0)),
                //
                // (t["b212"], new Vector3Int(3, 0, 0)),
                // (t["b212"], new Vector3Int(3, 2*unit, 0)),
            }, new Vector3Int());

            var sample = ToSampleGrid(patterns, legoTiles, true);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) Get211Pattern()
        {
            var bricks = new string[] { "b211", "b111","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                // (t["b111"], new Vector3Int(0, 0, 0)),
                (t["b211"], new Vector3Int(1, 0, 0)),
                (t["b211"], new Vector3Int(0, unit, 0)),
                (t["b211"], new Vector3Int(2, unit, 0)),
                // (t["b111"], new Vector3Int(0, 2*unit, 0)),
                (t["b211"], new Vector3Int(1, 2*unit, 0)),
            }, new Vector3Int(0,0,1));

            LayerAdd(ref patterns, new Range3D(0,5,0,3,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,5,0,3,2,3), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) Get211Pattern2D()
        {
            var bricks = new string[] { "b211", "b111","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b111"], new Vector3Int(0, 0, 0)),
                (t["b111"], new Vector3Int(3, 0, 0)),
                (t["b211"], new Vector3Int(1, 0, 0)),
                (t["b211"], new Vector3Int(0, unit, 0)),
                (t["b211"], new Vector3Int(2, unit, 0)),
                (t["b111"], new Vector3Int(0, 2*unit, 0)),
                (t["b111"], new Vector3Int(3, 2*unit, 0)),
                (t["b211"], new Vector3Int(1, 2*unit, 0)),
            }, new Vector3Int(0,0,1));

            LayerAdd(ref patterns, new Range3D(0,5,0,3,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,5,0,3,2,3), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) Get112Pattern()
        {
            var bricks = new string[] { "b112", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b112"], new Vector3Int(0, 0, 1)),
                (t["b112"], new Vector3Int(0, unit, 0)),
                (t["b112"], new Vector3Int(0, unit,2)),
                (t["b112"], new Vector3Int(0, 2*unit, 1)),
            }, new Vector3Int(1,0,0));

            LayerAdd(ref patterns, new Range3D(0,1,0,3,0,5), t["void"]);
            LayerAdd(ref patterns, new Range3D(2,3,0,3,0,5), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }

        // private (string[] bricks, SampleGrid sample) GetSplitWindowPattern()
        // {
        //     var bricks = new string[] { "b112", "b212","b312", "windowSplit","void" };
        //     var (unit, legoTiles, t) = PatternData(bricks);
        //     var patterns = TranslatePattern(new Patterns()
        //     {
        //         (t["windowSplit"], new Vector3Int(4,unit,0)),
        //         
        //         (t["b312"], new Vector3Int(0,unit,0)),
        //         (t["b312"], new Vector3Int(0,3*unit,0)),
        //         (t["b312"], new Vector3Int(0,5*unit,0)),
        //         (t["b312"], new Vector3Int(0,7*unit,0)),
        //         
        //         (t["b312"], new Vector3Int(2,0,0)),
        //         (t["b312"], new Vector3Int(2,6*unit,0)),
        //         (t["b312"], new Vector3Int(2,8*unit,0)),
        //         
        //         (t["b212"], new Vector3Int(2,2*unit,0)),
        //         (t["b212"], new Vector3Int(2,4*unit,0)),
        //         
        //         (t["b212"], new Vector3Int(3,unit,0)),
        //         (t["b212"], new Vector3Int(3,5*unit,0)),
        //         
        //         (t["b312"], new Vector3Int(3,3*unit,0)),
        //         (t["b312"], new Vector3Int(3,7*unit,0)),
        //         
        //         (t["b212"], new Vector3Int(5,0,0)),
        //         (t["b212"], new Vector3Int(5,6*unit,0)),
        //        
        //         (t["b312"], new Vector3Int(6,3*unit,0)),
        //         (t["b312"], new Vector3Int(6,7*unit,0)),
        //        
        //         (t["b312"], new Vector3Int(7,0,0)),
        //         (t["b312"], new Vector3Int(7,6*unit,0)),
        //         (t["b312"], new Vector3Int(7,8*unit,0)),
        //         
        //         (t["b212"], new Vector3Int(7,unit,0)),
        //         (t["b212"], new Vector3Int(7,5*unit,0)),
        //         
        //         (t["b212"], new Vector3Int(8,2*unit,0)),
        //         (t["b212"], new Vector3Int(8,4*unit,0)),
        //         
        //         (t["b312"], new Vector3Int(9,unit,0)),
        //         (t["b312"], new Vector3Int(9,3*unit,0)),
        //         (t["b312"], new Vector3Int(9,5*unit,0)),
        //         (t["b312"], new Vector3Int(9,7*unit,0)),
        //     }, new Vector3Int(0, 0, 0));
        //     var sample = ToSampleGrid(patterns, legoTiles,false);
        //     return (bricks, sample);
        // }
        
        private (string[] bricks, SampleGrid sample) GetWindowPatternLarge()
        {
            var bricks = new string[] { "b112", "b212","b312", "window","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["window"], new Vector3Int(4,unit,0)),
                
                (t["b212"], new Vector3Int(3,0,0)),
                (t["b212"], new Vector3Int(11,0,0)),
                (t["b312"], new Vector3Int(5,0,0)),
                (t["b312"], new Vector3Int(8,0,0)),
                
                (t["b312"], new Vector3Int(1,unit,0)),
                (t["b112"], new Vector3Int(4,unit,0)),
                (t["b312"], new Vector3Int(12,unit,0)),
                (t["b112"], new Vector3Int(11,unit,0)),
                
                (t["b312"], new Vector3Int(0,2*unit,0)),
                (t["b112"], new Vector3Int(3,2*unit,0)),
                (t["b112"], new Vector3Int(12,2*unit,0)),
                (t["b312"], new Vector3Int(13,2*unit,0)),
                
                (t["b312"], new Vector3Int(1,3*unit,0)),
                (t["b312"], new Vector3Int(12,3*unit,0)),
                
                (t["b212"], new Vector3Int(3,4*unit,0)),
                (t["b212"], new Vector3Int(11,4*unit,0)),
                
                (t["b312"], new Vector3Int(1,5*unit,0)),
                (t["b212"], new Vector3Int(4,5*unit,0)),
                (t["b212"], new Vector3Int(10,5*unit,0)),
                (t["b312"], new Vector3Int(12,5*unit,0)),
                
                (t["b212"], new Vector3Int(3,6*unit,0)),
                (t["b312"], new Vector3Int(5,6*unit,0)),
                (t["b312"], new Vector3Int(8,6*unit,0)),
                (t["b212"], new Vector3Int(11,6*unit,0)),
                
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,16,0,7*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,16,0,7*unit,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles,false);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetOddCurlPattern()
        {
            var bricks = new string[] { "b212","b312", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b312"], new Vector3Int(1,0,0)),
                (t["b212"], new Vector3Int(4,0,0)),
                (t["b312"], new Vector3Int(6,0,0)),
                
                (t["b312"], new Vector3Int(0,unit,0)),
                (t["b212"], new Vector3Int(3,unit,0)),
                (t["b312"], new Vector3Int(5,unit,0)),
                
                
                (t["b312"], new Vector3Int(1,2*unit,0)),
                (t["b212"], new Vector3Int(4,2*unit,0)),
                (t["b312"], new Vector3Int(6,2*unit,0)),
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,10,0,3*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,10,0,3*unit,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles,false);
            return (bricks, sample);
        }

        private (string[] bricks, SampleGrid sample) GetSmallWindowPattern()
        {
            var bricks = new string[] { "b212","b312", "windowSmall","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["windowSmall"], new Vector3Int(3,unit,0)),
                (t["b312"], new Vector3Int(2,0,0)),
                (t["b312"], new Vector3Int(5,0,0)),
                (t["b312"], new Vector3Int(1,unit,0)),
                (t["b312"], new Vector3Int(7,unit,0)),
                (t["b312"], new Vector3Int(0,2*unit,0)),
                (t["b312"], new Vector3Int(8,2*unit,0)),
                (t["b312"], new Vector3Int(1,3*unit,0)),
                (t["b312"], new Vector3Int(7,3*unit,0)),
                (t["b312"], new Vector3Int(2,4*unit,0)),
                (t["b312"], new Vector3Int(5,4*unit,0)),
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,11,0,4*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,11,0,4*unit,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles,false);
            return (bricks, sample);

        }
        private (string[] bricks, SampleGrid sample) GetWindowPattern()
        {
            var bricks = new string[] { "b212","b312", "window","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["window"], new Vector3Int(3,2*unit,0)),
                
                (t["b312"], new Vector3Int(1,0,0)),
                // (t["b212"], new Vector3Int(2,0,0)),
                (t["b312"], new Vector3Int(4,0,0)),
                (t["b312"], new Vector3Int(7,0,0)),
                (t["b312"], new Vector3Int(10,0,0)),
                // (t["b212"], new Vector3Int(10,0,0)),
                
                (t["b312"], new Vector3Int(0,unit,0)),
                (t["b212"], new Vector3Int(3,unit,0)),
                (t["b212"], new Vector3Int(5,unit,0)),
                (t["b212"], new Vector3Int(7,unit,0)),
                (t["b212"], new Vector3Int(9,unit,0)),
                (t["b312"], new Vector3Int(11,unit,0)),
                
                // (t["b212"], new Vector3Int(2,2*unit,0)),
                // (t["b212"], new Vector3Int(10,2*unit,0)),
                (t["b312"], new Vector3Int(1,2*unit,0)),
                (t["b312"], new Vector3Int(10,2*unit,0)),
                
                (t["b312"], new Vector3Int(0,3*unit,0)),
                (t["b312"], new Vector3Int(11,3*unit,0)),
                
                // (t["b212"], new Vector3Int(2,4*unit,0)),
                // (t["b212"], new Vector3Int(10,4*unit,0)),
                
                (t["b312"], new Vector3Int(1,4*unit,0)),
                (t["b312"], new Vector3Int(10,4*unit,0)),
                
                (t["b312"], new Vector3Int(0,5*unit,0)),
                (t["b312"], new Vector3Int(11,5*unit,0)),
                
                (t["b312"], new Vector3Int(1,6*unit,0)),
                // (t["b212"], new Vector3Int(2,6*unit,0)),
                (t["b312"], new Vector3Int(4,6*unit,0)),
                (t["b312"], new Vector3Int(7,6*unit,0)),
                (t["b312"], new Vector3Int(10,6*unit,0)),
                // (t["b212"], new Vector3Int(10,6*unit,0)),
                
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,16,0,7*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,16,0,7*unit,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles,false);
            return (bricks, sample);
        }
        private (string[] bricks, SampleGrid sample) GetOddBrickPattern2()
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
            LayerAdd(ref patterns, new Range3D(0,9,0,9,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) GetOddBrickPattern()
        {
            var bricks = new string[] { "b212","b312","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b312"], new Vector3Int(0, 0, 0)),
                (t["b312"], new Vector3Int(3, 0, 0)),
                (t["b312"], new Vector3Int(6, 0, 0)),
                (t["b312"], new Vector3Int(11, 0, 0)),
                
                (t["b212"], new Vector3Int(9, 0, 0)),
                (t["b212"], new Vector3Int(5, unit, 0)),
                (t["b212"], new Vector3Int(9, 2*unit, 0)),
                
                (t["b312"], new Vector3Int(0, 2*unit, 0)),
                (t["b312"], new Vector3Int(3, 2*unit, 0)),
                (t["b312"], new Vector3Int(6, 2*unit, 0)),
                (t["b312"], new Vector3Int(11, 2*unit, 0)),
                
                (t["b312"], new Vector3Int(2, unit, 0)),
                (t["b312"], new Vector3Int(7, unit, 0)),
                (t["b312"], new Vector3Int(10, unit, 0)),
                (t["b312"], new Vector3Int(13, unit, 0)),
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,16,0,3*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,16,0,3*unit,3,4), t["void"]);
            var sample = ToSampleGrid(patterns, legoTiles, false);
            return (bricks, sample);
        }
        
        private (string[] bricks, SampleGrid sample) GetOddBrickPattern2D()
        {
            var bricks = new string[] { "b212","b312","void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var patterns = TranslatePattern(new Patterns()
            {
                (t["b312"], new Vector3Int(0, 0, 0)),
                (t["b312"], new Vector3Int(3, 0, 0)),
                (t["b312"], new Vector3Int(6, 0, 0)),
                (t["b312"], new Vector3Int(11, 0, 0)),
                
                (t["b212"], new Vector3Int(9, 0, 0)),
                (t["b212"], new Vector3Int(14, 0, 0)),
                (t["b212"], new Vector3Int(14, 2*unit, 0)),
                (t["b212"], new Vector3Int(0, unit, 0)),
                (t["b212"], new Vector3Int(5, unit, 0)),
                (t["b212"], new Vector3Int(9, 2*unit, 0)),
                
                (t["b312"], new Vector3Int(0, 2*unit, 0)),
                (t["b312"], new Vector3Int(3, 2*unit, 0)),
                (t["b312"], new Vector3Int(6, 2*unit, 0)),
                (t["b312"], new Vector3Int(11, 2*unit, 0)),
                
                (t["b312"], new Vector3Int(2, unit, 0)),
                (t["b312"], new Vector3Int(7, unit, 0)),
                (t["b312"], new Vector3Int(10, unit, 0)),
                (t["b312"], new Vector3Int(13, unit, 0)),
            }, new Vector3Int(0, 0, 1));
            LayerAdd(ref patterns, new Range3D(0,16,0,3*unit,0,1), t["void"]);
            LayerAdd(ref patterns, new Range3D(0,16,0,3*unit,3,4), t["void"]);
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
        
        private (string[] bricks, SampleGrid sample) GetRunningStackedBrickPattern()
        {
            var bricks = new string[] { "b412", "b212", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var evenConnectorPattern = TranslatePattern(new Patterns()
            {
                (t["b412"], new Vector3Int(2, 0, 0)),
                (t["b412"], new Vector3Int(2, 2 * unit, 0)),
                (t["b412"], new Vector3Int(0, unit, 0)),
                
                (t["b212"], new Vector3Int(4, unit, 0)),
                (t["b412"], new Vector3Int(6, 0, 0)),
                (t["b412"], new Vector3Int(6, unit, 0)),
                (t["b412"], new Vector3Int(6, 2*unit, 0)),
                
                (t["b412"], new Vector3Int(10, 0, 0)),
                (t["b412"], new Vector3Int(10, 2 * unit, 0)),
                (t["b412"], new Vector3Int(12, unit, 0)),
                
                (t["b212"], new Vector3Int(10, unit, 0)),
                
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
            // LayerAdd(ref doorPattern, new Range3D(0,16,0,6*unit, 0,1), t["void"]); // Layer in front.
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
        
        
        
        private (string[] bricks, SampleGrid cornerSample) GetCornerPattern(bool includeVoidLayer=true, bool includeFrontVoid=false)
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

            if (includeFrontVoid)
            {
                corners = TranslatePattern(corners, new Vector3Int(0, 0, 1));
            }
            
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
        
        private (string[] bricks, SampleGrid cornerSample) GetCornerPatternOddSingle(bool includeVoidLayer=true)
        {
            var bricks = new string[] { "b312", "b213", "b111", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var corners = new Patterns()
            {
                (t["b213"], new Vector3Int(0,unit,0)),
                (t["b213"], new Vector3Int(0,unit,3)),
                
                (t["b312"], new Vector3Int(0,unit,6)),
                (t["b312"], new Vector3Int(3,unit,6)),
                
                (t["b213"], new Vector3Int(6,unit,5)),
                (t["b213"], new Vector3Int(6,unit,2)),
                
                (t["b312"], new Vector3Int(2,unit,0)),
                (t["b312"], new Vector3Int(5,unit,0)),
                
            };
            LayerAdd(ref corners, new Range3D(0,2,0,1,0,8), t["b111"]);
            LayerAdd(ref corners, new Range3D(6,8,0,1,0,8), t["b111"]);
            LayerAdd(ref corners, new Range3D(2,6,0,1,0,2), t["b111"]);
            LayerAdd(ref corners, new Range3D(2,6,0,1,6,8), t["b111"]);
            var cornerSample = ToSampleGrid(corners, legoTiles, extraLayer:includeVoidLayer ? new Vector3Int(1,0,1) : null);
            return (bricks, cornerSample);
        }
        
        private (string[] bricks, SampleGrid cornerSample) GetCornerPatternEvenSingle(bool includeVoidLayer=true)
        {
            var bricks = new string[] { "b312", "b213", "b111", "void" };
            var (unit, legoTiles, t) = PatternData(bricks);
            var corners = new Patterns()
            {
                (t["b412"], new Vector3Int(0,0,0)),
                (t["b412"], new Vector3Int(4,0,0)),
                
                (t["b214"], new Vector3Int(8,0,0)),
                (t["b214"], new Vector3Int(8,0,4)),
                
                (t["b412"], new Vector3Int(6,0,8)),
                (t["b412"], new Vector3Int(2,0,8)),
                
                (t["b214"], new Vector3Int(0,0,6)),
                (t["b214"], new Vector3Int(0,0,2)),
                
            };
            LayerAdd(ref corners, new Range3D(0,2,0,1,0,10), t["b111"]);
            LayerAdd(ref corners, new Range3D(8,10,0,1,0,10), t["b111"]);
            LayerAdd(ref corners, new Range3D(2,8,0,1,0,2), t["b111"]);
            LayerAdd(ref corners, new Range3D(2,8,0,1,8,10), t["b111"]);
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

        private (string[] bricks, SampleGrid brickPattern214Grid) Get214BrickPattern(bool leftVoid=true, bool rightVoid=true)
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
            
            if (leftVoid) LayerAdd(ref brickPattern214, new Range3D(0, 1, 0,3*unit, 0, 8), t["void"]);
            if (rightVoid) LayerAdd(ref brickPattern214, new Range3D(3, 4, 0,3*unit, 0, 8), t["void"]);

            var brickPattern214Grid = ToSampleGrid(brickPattern214, legoTiles, false);
            return (bricks, brickPattern214Grid);
        }

        private (string[] bricks, SampleGrid brickPattern412Grid) Get412BrickPattern(bool rearVoid=true, bool frontVoid=true)
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
            
            if (frontVoid) LayerAdd(ref brickPattern412, new Range3D(0,8,0,3*unit,0,1), t["void"]);
            if (rearVoid) LayerAdd(ref brickPattern412, new Range3D(0,8,0,3*unit,3,4), t["void"]);

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