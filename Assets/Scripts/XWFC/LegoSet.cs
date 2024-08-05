using System.Collections.Generic;
using Patterns = System.Collections.Generic.List<(int,UnityEngine.Vector3Int)>;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace XWFC
{
    public class LegoSet
    {
        public static TileSet GetLegoSet()
        {
            // b211, b312, b412, p111, p211, door, window, void
            var tiles = new NonUniformTile[]
            {
                // Bricks
                new NonUniformTile(
                    "b112",
                    new Vector3Int(1,3,2),
                    new Color32(35, 120, 65, 255)
                ),
                new NonUniformTile(
                    "b312",
                    new Vector3Int(3,3,2),
                    new Color32(0, 85, 191, 255)
                ),
                new NonUniformTile(
                    "b412",
                    new Vector3Int(4,3,2),
                    new Color32(201, 26, 9, 255)
                ),
                new NonUniformTile(
                    "b212",
                    new Vector3Int(1,3,1),
                    new Color32(0, 185, 241, 255)
                ),
                new NonUniformTile(
                    "b111",
                    new Vector3Int(1,3,1),
                    new Color32(242, 243, 242, 255)
                ),
                
                // Bricks 90deg
                new NonUniformTile(
                    "b211",
                    new Vector3Int(2,3,1),
                    new Color32(35, 120, 65, 255)
                ),
                new NonUniformTile(
                    "b213",
                    new Vector3Int(2,3,3),
                    new Color32(0, 85, 191, 255)
                ),
                new NonUniformTile(
                    "b214",
                    new Vector3Int(2,3,4),
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
                    new Vector3Int(3, 15, 1),
                    new Color(0.4f, 0.2f, 0.1f)
                ),
                new NonUniformTile(
                    "window",
                    new Vector3Int(3,9,1),
                    new Color(0, 0, 0.8f)
                ),
                
                // Compound bricks 90deg
                new NonUniformTile(
                    "door90",
                    new Vector3Int(1, 15, 3),
                    new Color(0.4f, 0.2f, 0.1f)
                ),
                new NonUniformTile(
                    "window90",
                    new Vector3Int(1,9,3),
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



        public static (TileSet t, List<Patterns>) WallPerimeterExample()
        {
            var bricks = new string[] { "b412", "b214", "b213", "b312", "b212" };
            var legoTiles = GetLegoSubset(bricks);
            
            var t = new Dictionary<string, int>();
            foreach (var tile in bricks)
            {
                t[tile] = legoTiles.GetTileIdFromValue(tile);
            }

            var (NORTH, EAST, SOUTH, WEST, UP, DOWN) = (Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
                Vector3.up, Vector3.down);

            var corner0 = new Patterns()
            {
                (t["b412"], new Vector3Int(0, 0, 0)),
                (t["b412"], new Vector3Int(2, 3, 0)),
                (t["b412"], new Vector3Int(0, 6, 0)),
                (t["b214"], new Vector3Int(0, 0, 2)),
                (t["b214"], new Vector3Int(0, 3, 0)),
                (t["b214"], new Vector3Int(0, 6, 2)),
            };

            var brickPattern412 = new Patterns()
            {
                (t["b412"], new Vector3Int(2, 0, 0)),
                (t["b412"], new Vector3Int(0, 3, 0)),
                (t["b412"], new Vector3Int(4, 3, 0)),
                (t["b412"], new Vector3Int(2, 6, 0)),
            };
            return (legoTiles, new List<Patterns>() { brickPattern412 });
        }

        public static TileSet GetLegoSubset(string[] strings)
        {
            var legoTiles = GetLegoSet();
            return legoTiles.GetSubset(strings);
        }
    }
}