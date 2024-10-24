﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace XWFC
{
    public class InputHandler
    {
        public TileSet Tiles;
        private Bidict<int, int> _tileIdToIndexMapping;

        public static List<SampleGrid> PatternsToGrids(
            List<List<(int, Vector3Int)>> patterns, TileSet tileSet, string defaultFillValue)
        {
            var tileIds = new HashSet<int>();
            var grids = new List<SampleGrid>();
            foreach (var pattern in patterns)
            {
                var (grid, ids) = ToSampleGrid(pattern, tileSet, defaultFillValue);
                
                foreach (var id in ids.Where(id => !tileIds.Contains(id)))
                {
                    tileIds.Add(id);
                }
                grids.Add(grid);
            }
            return grids;
        }

        public static (SampleGrid placedTiles, HashSet<int> tileIds) ToSampleGrid(List<(int, Vector3Int)> tileIdOrigins, TileSet tileSet, string defaultFillValue)
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

            var placedTiles = new SampleGrid(maxCoord, defaultFillValue);
            
            foreach (var (tileId, origin) in tileIdOrigins)
            {
                var tile = tileSet[tileId];
                foreach (var atomCoord in tile.OrientedIndices[0])
                {
                    var value = placedTiles.Get(atomCoord + origin);
                    if (value!=null && !value.Equals(placedTiles.DefaultFillValue)) throw new Exception($"Tiles may not overlap: {(tileId, origin)}, value: {value}");
                    placedTiles.Set(atomCoord + origin, tile.GetAtomValue(atomCoord));
                }
            }

            return (placedTiles, tileIds);
        }
    }
}