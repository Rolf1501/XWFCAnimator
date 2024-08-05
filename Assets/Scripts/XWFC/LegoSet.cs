using System.Collections.Generic;
using System.Linq;
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
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "b312",
                    new Vector3Int(3,3,2),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "b412",
                    new Vector3Int(4,3,2),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "b111",
                    new Vector3Int(1,3,1),
                    new Color(1,0,0)
                ),
                
                // Bricks 90deg
                new NonUniformTile(
                    "b211",
                    new Vector3Int(2,3,1),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "b213",
                    new Vector3Int(2,3,3),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "b214",
                    new Vector3Int(2,3,4),
                    new Color(1,0,0)
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
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "p212",
                    new Vector3Int(2,1,2),
                    new Color(1,0,0)
                ),
                
                // Compound bricks
                new NonUniformTile(
                    "door",
                    new Vector3Int(3, 15, 1),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "window",
                    new Vector3Int(3,9,1),
                    new Color(1,0,0)
                ),
                
                // Compound bricks 90deg
                new NonUniformTile(
                    "door90",
                    new Vector3Int(1, 15, 3),
                    new Color(1,0,0)
                ),
                new NonUniformTile(
                    "window90",
                    new Vector3Int(1,9,3),
                    new Color(1,0,0)
                ),
                
                // Void
                new NonUniformTile(
                    "void",
                    new Vector3Int(1,1,1),
                    new Color(1,0,0),
                    isEmptyTile:true
                ),
            };

            var tileSet = new TileSet();
            for (var i = 0; i < tiles.Length; i++)
            {
                tileSet[i] = tiles[i];
            }

            return tileSet;
        }

        public static TileSet GetLegoSubset(string[] tileNames)
        {
            var subset = new TileSet();
            var legoTiles = GetLegoSet();
            for (var i = 0; i < legoTiles.Count; i++)
            {
                var tile = legoTiles[i];
                if (tileNames.Contains(tile.UniformAtomValue))
                {
                    subset[i] = tile;
                }
            }

            return subset;
        }

        public static void NutAdjacencyConstraints2x4()
        {
            var legoTiles = GetLegoSet();
            var b24tiles = new string[] { "b412", "b214" };
            var tileIds = new Dictionary<string, int>();
            foreach (var b24Tile in b24tiles)
            {
                tileIds[b24Tile] = legoTiles.GetTileIdFromValue(b24Tile);
            }

            var (NORTH, EAST, SOUTH, WEST, UP, DOWN) = (Vector3.forward, Vector3.right, Vector3.back, Vector3.left, Vector3.up, Vector3.down);

            // var adj = new HashSetAdjacency()
            // {
            //     // 214 - 214
            //     new Adjacency(tileIds["b214"], new List<int>()
            //     { tileIds["b214"]}, 
            //         EAST),
            //     new Adjacency(tileIds["b214"], new List<int>()
            //             { tileIds["b214"]}, 
            //         WEST),
            //     new Adjacency(tileIds["b214"], new List<int>()
            //             { tileIds["b214"]}, 
            //         UP),
            //     new Adjacency(tileIds["b214"], new List<int>()
            //             { tileIds["b214"]}, 
            //         DOWN)
            // };
            // _adjacency = new HashSetAdjacency(){
            //     // 0-0
            //     // new(0, new List<Relation>() { new(0, null) }, NORTH),
            //     new(0, new List<Relation>() { new(0, null) }, EAST),
            //     // new(0, new List<Relation>() { new(0, null) }, SOUTH),
            //     new(0, new List<Relation>() { new(0, null) }, WEST),
            //     new(0, new List<Relation>() { new(0, null) }, TOP),
            //     new(0, new List<Relation>() { new(0, null) }, BOTTOM),
            //     // 1-0
            //     new(1, new List<Relation>() { new(0, null) }, NORTH),
            //     // new(1, new List<Relation>() { new(0, null) }, EAST),
            //     new(1, new List<Relation>() { new(0, null) }, SOUTH),
            //     // new(1, new List<Relation>() { new(0, null) }, WEST),
            //     new(1, new List<Relation>() { new(0, null) }, TOP),
            //     new(1, new List<Relation>() { new(0, null) }, BOTTOM),
            //     // 1-1
            //     // new(1, new List<Relation>() { new(0, null) }, NORTH),
            //     new(1, new List<Relation>() { new(0, null) }, EAST),
            //     // new(1, new List<Relation>() { new(0, null) }, SOUTH),
            //     new(1, new List<Relation>() { new(0, null) }, WEST),
            //     new(1, new List<Relation>() { new(0, null) }, TOP),
            //     new(1, new List<Relation>() { new(0, null) }, BOTTOM),
            //     // 2-0
            //     new(2, new List<Relation>() { new(0, null) }, NORTH),
            //     new(2, new List<Relation>() { new(0, null) }, EAST),
            //     new(2, new List<Relation>() { new(0, null) }, SOUTH),
            //     new(2, new List<Relation>() { new(0, null) }, WEST),
            //     new(2, new List<Relation>() { new(0, null) }, TOP),
            //     new(2, new List<Relation>() { new(0, null) }, BOTTOM),
            //     // 2-1
            //     new(2, new List<Relation>() { new(1, null) }, NORTH),
            //     new(2, new List<Relation>() { new(1, null) }, EAST),
            //     new(2, new List<Relation>() { new(1, null) }, SOUTH),
            //     new(2, new List<Relation>() { new(1, null) }, WEST),
            //     new(2, new List<Relation>() { new(1, null) }, TOP),
            //     new(2, new List<Relation>() { new(1, null) }, BOTTOM),
            //     // 2-2
            //     new(2, new List<Relation>() { new(2, null) }, NORTH),
            //     new(2, new List<Relation>() { new(2, null) }, EAST),
            //     new(2, new List<Relation>() { new(2, null) }, SOUTH),
            //     new(2, new List<Relation>() { new(2, null) }, WEST),
            //     new(2, new List<Relation>() { new(2, null) }, TOP),
            //     new(2, new List<Relation>() { new(2, null) }, BOTTOM),
            // };
        }
    }
}