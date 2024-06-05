using System;
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

        public static (List<Grid<(int, int)>> grids, int[] tileIds) PatternsToGrids(
            List<List<(int, Vector3)>> patterns, TileSet tileSet, (int, int) defaultFillValue)
        {
            var tileIds = new HashSet<int>();
            var grids = new List<Grid<(int, int)>>();
            foreach (var pattern in patterns)
            {
                var (grid, ids) = ToInputGrid(pattern, tileSet, defaultFillValue);
                
                foreach (var id in ids.Where(id => !tileIds.Contains(id)))
                {
                    tileIds.Add(id);
                }
                grids.Add(grid);
            }
            return (grids, tileIds.ToArray());
        }

        public static (Grid<(int, int)> grid, HashSet<int> tileIds) ToInputGrid(List<(int, Vector3)> tileIdOrigins, TileSet tileSet, (int,int) defaultFillValue)
        {
            var maxCoord = new Vector3();
            var tileIds = new HashSet<int>();
            
            // Find extent such that all tiles can fit in the placed tiles grid.
            foreach (var (tileId, origin) in tileIdOrigins)
            {
                if (origin.x < 0 || origin.y < 0 || origin.z < 0)
                    throw new Exception($"Origin must not be negative. Got: {origin}");
                var tile = tileSet[tileId];
                
                tileIds.Add(tileId);
                
                foreach (var atomCoord in tile.OrientedIndices[0])
                {
                    var coord = atomCoord + origin;
                    if (coord.x > maxCoord.x) maxCoord.x = coord.x;
                    if (coord.y > maxCoord.y) maxCoord.y = coord.y;
                    if (coord.z > maxCoord.z) maxCoord.z = coord.z;
                }
            }

            maxCoord += new Vector3(1, 1, 1); // Account for index - length difference. 

            var placedTiles = new Grid<(int tileId, int instanceId)>(maxCoord, defaultFillValue);
            
            var instanceIds = tileSet.Keys.ToDictionary(k => k, _ => 0);
            foreach (var (tileId, origin) in tileIdOrigins)
            {
                var tile = tileSet[tileId];
                foreach (var atomCoord in tile.OrientedIndices[0])
                {
                    var value = placedTiles.Get(atomCoord + origin);
                    if (value != placedTiles.DefaultFillValue) throw new Exception($"Tiles may not overlap: {(tileId, origin)}, value: {value}");
                    placedTiles.Set(atomCoord + origin, (tileId, instanceIds[tileId]));
                }

                instanceIds[tileId] += 1;
            }

            return (placedTiles, tileIds);
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