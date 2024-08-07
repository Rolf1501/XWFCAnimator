using System.Collections.Generic;
using UnityEngine;

namespace XWFC
{
    public class TetrisSet
    {
        public static TileSet GetTetrisTileSet()
        {
            var tileL = new NonUniformTile(
                "L",
            new Vector3Int(2, 1, 3),
                new Color(240 / 255.0f, 160 / 255.0f, 0),
                new bool[,,] { { { true, true, true }, { true, false, false } } },
                null,
                
                computeAtomEdges:true
            );
            var tileT = new NonUniformTile(
                "T",
                new Vector3Int(2, 1, 3),
                new Color(160 / 255.0f, 0, 240/255.0f),
                new bool[,,] { { { false, true, false }, { true, true, true }} },
                null,
                computeAtomEdges:true
            );
            
            var tileJ = new NonUniformTile(
                "J",
                new Vector3Int(2, 1, 3),
                new Color(0, 0, 240 / 255.0f),
                new bool[,,] { { { true, false, false }, { true, true, true } } },
                null,
                computeAtomEdges:true
            );
            
            var tileI = new NonUniformTile(
                "I",
                new Vector3Int(4, 1, 1),
                new Color(120 / 255.0f, 120 / 255.0f, 1 / 255.0f),
                new bool[,,] { { { true }, { true }, { true }, { true } } },
                null,
                computeAtomEdges:true
            );
            
            var tileZ = new NonUniformTile(
                "Z",
                new Vector3Int(2, 1, 3),
                new Color(240 / 255.0f, 0, 0),
                new bool[,,] { { { true, true, false }, { false, true, true }} },
                null,
                computeAtomEdges:true
            );
            var tileS = new NonUniformTile(
                "S",
                new Vector3Int(2, 1, 3),
                new Color(0, 240 / 255.0f, 0),
                new bool[,,] { { { false, true, true }, { true, true, false }} },
                null,
                computeAtomEdges:true
            );
            var tileO = new NonUniformTile(
                "O",
                new Vector3Int(2, 1, 2),
                new Color(0, 240 / 255.0f, 240 / 255.0f),

                // new Color(240 / 255.0f, 240 / 255.0f, 0),
                new bool[,,] { { { true, true }, { true, true }} },
                null,
                computeAtomEdges:true
            );

            var tetrisTiles = new NonUniformTile[] { tileL, tileT, tileJ, tileI, tileZ, tileS, tileO };
            var tileSet = new TileSet();
            for (var i = 0; i < tetrisTiles.Length; i++)
            {
                tileSet[i] = tetrisTiles[i];
            }
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

        public static TileSet GetLargeTetrisSet()
        {
            var largeL = GetBools(new Vector3Int(4, 1, 6), true, new List<Range3D>()
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
            
            var largeT = GetBools(new Vector3Int(4, 1, 6), true, new List<Range3D>()
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
            
            var largeJ = GetBools(new Vector3Int(4, 1, 6), true, new List<Range3D>()
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
            
            var largeI = GetBools(new Vector3Int(8, 1, 2), true, new List<Range3D>());
            var tileI = new NonUniformTile(
                "IL",
                new Vector3Int(),
                new Color(120 / 255.0f, 120 / 255.0f, 1 / 255.0f),
                largeI,
                null,
                computeAtomEdges:true
            );
            
            var largeZ = GetBools(new Vector3Int(4, 1, 6), true, new List<Range3D>()
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

            var largeS = GetBools(new Vector3Int(4, 1, 6), true, new List<Range3D>()
            {
                new(0, 2, 0, 1, 0,2),
                new(2, 4, 0, 1, 4, 6)
            });

            var tileS = new NonUniformTile(
                "SL",
                new Vector3Int(),
                new Color(0, 240 / 255.0f, 0),
                largeS,
                null,
                computeAtomEdges:true
            );

            var largeO = GetBools(new Vector3Int(4, 1, 4), true, new List<Range3D>());
            var tileO = new NonUniformTile(
                "OL",
                new Vector3Int(),
                new Color(0, 240 / 255.0f, 240 / 255.0f),

                // new Color(240 / 255.0f, 240 / 255.0f, 0),
                largeO,
                null,
                computeAtomEdges:true
            );
            
            var tetrisTiles = new NonUniformTile[] { tileL, tileT, tileJ, tileI, tileZ, tileS, tileO };
            var tileSet = new TileSet();
            for (var i = 0; i < tetrisTiles.Length; i++)
            {
                tileSet[i] = tetrisTiles[i];
            }
            return tileSet;
        }
    }
}