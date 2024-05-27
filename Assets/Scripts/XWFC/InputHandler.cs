using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using InputTiles = System.Collections.Generic.Dictionary<int, (bool[,,] mask, UnityEngine.Color color)>;
using TileAtomRangeMapping = System.Collections.Generic.Dictionary<int, (int start, int end)>;

namespace XWFC
{
    public class InputHandler
    {
        public TileSet Tiles;
        private Bidict<int, int> _tileIdToIndexMapping;
        private TileAtomRangeMapping _tileAtomRangeMapping;
        
        public InputHandler(InputTiles tiles, Grid<(int tileId,int instanceId)> grid)
        {
            /*
             * Need:
             * tile set
             * grid
             *
             * Grid:
             * Each cell should contain NUT id and instance id. 
             */
            
            // Convert to tile set.
            Tiles = ToTileSet(tiles);
            
            // Map tile id to tile index.
            _tileIdToIndexMapping = MapTileIdToIndex(Tiles);
            
            // Create tile to atom range mapping.
            _tileAtomRangeMapping = MapTileAtomRange(Tiles);
            
            // Infer inner atom adjacency constraints.
            
            
            // Infer inter atom adjacency constraints.

        }

        public static TileSet ToTileSet(InputTiles tiles)
        {
            var t = new TileSet();
            foreach (var (k, (mask, color)) in tiles)
            {
                var extent = new Vector3(mask.GetLength(1), mask.GetLength(0), mask.GetLength(2));
                t[k] = new Tile(extent, mask: mask, color: color, distinctOrientations:null, computeAtomEdges:true, description:"");
            }

            return t;
        }
        
        private TileAtomRangeMapping MapTileAtomRange(TileSet tiles)
        {
            int nAtoms = 0;
            var mapping = new TileAtomRangeMapping();
            // Map each terminal to a range in the mapping list.
            foreach (int tileId in tiles.Keys)
            {
                Tile t = tiles[tileId];
                int tAtoms = t.NAtoms;
                mapping[tileId] = (nAtoms, nAtoms + tAtoms);
                nAtoms += tAtoms;
            }

            return mapping;
        }

        private Bidict<int, int> MapTileIdToIndex(TileSet tiles)
        {
            var mapping = new Bidict<int, int>();
            var keyArray = tiles.Keys.ToArray();
            for (int i = 0; i < keyArray.Length; i++)
            {
                mapping.AddPair(keyArray[i], i);
            }

            return mapping;
        }

        private void InnerAtomAdjacencyConstraints()
        {
            
        }

        public void AtomizeGrid()
        {
            
        }
    }
}