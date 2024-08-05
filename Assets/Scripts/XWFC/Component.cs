using System;
using System.Collections.Generic;
using UnityEngine;

namespace XWFC
{
    public class Component
    {
        /*
         * Class for representing components.
         * Components are essentially AABB with additional information used for running XWFC on them. 
         */
        public Vector3Int Origin;
        public Vector3Int Extent;
        public TileSet Tiles;
        public Grid<int> Grid;
        public AdjacencyMatrix AdjacencyMatrix;

        private Dictionary<Vector3, int[,]> _voidMasks = new ();

        public Component(Vector3Int origin, Vector3Int extent, TileSet tileSet, SampleGrid[] inputGrids, float[] tileWeights=null)
        {
            Origin = origin;
            Extent = extent;
            Tiles = tileSet;
            Grid = new Grid<int>(Extent, -1);
            AdjacencyMatrix = new AdjacencyMatrix(tileSet, inputGrids, AdjacencyMatrix.ToWeightDictionary(tileWeights, tileSet));
        }

        public Component(Vector3Int origin, Vector3Int extent, TileSet tileSet, HashSetAdjacency adjacencyConstraints,
            float[] tileWeights=null)
        {
            Origin = origin;
            Extent = extent;
            Tiles = tileSet;
            Grid = new Grid<int>(Extent, -1);
            AdjacencyMatrix = new AdjacencyMatrix(adjacencyConstraints, tileSet, new Dictionary<int, float>());
        }

        public Range XRange()
        {
            return new Range(Origin.x, Origin.x + Extent.x);
        }

        public Range YRange()
        {
            return new Range(Origin.y, Origin.y + Extent.y);
        }
        
        public Range ZRange()
        {
            return new Range(Origin.z, Origin.z + Extent.z);
        }

        public Range3D Ranges()
        {
            return new Range3D(XRange(), YRange(), ZRange());
        }

        public void CalcVoidMasks()
        {
            var (x, y, z) = Vector3Util.CastInt(Grid.GetExtent());
            var boolMask = new bool[y,x,z];
            for (int bx = 0; bx < x; bx++)
            {
                for (int by = 0; by < y; by++)
                {
                    for (int bz = 0; bz < z; bz++)
                    {
                        var value = Grid.Get(bx, by, bz);
                        if (value != Grid.DefaultFillValue)
                        {
                            boolMask[by, bx, bz] = true;
                        }
                    }
                }
            }

            var (negY, posY) = VoidMasks.CalcVoidMask(boolMask, 0);
            var (negX, posX) = VoidMasks.CalcVoidMask(boolMask, 1);
            var (negZ, posZ) = VoidMasks.CalcVoidMask(boolMask, 2);

            
            _voidMasks[new Vector3(0, 1, 0)] = posY;
            _voidMasks[new Vector3(0, -1, 0)] = negY;
            _voidMasks[new Vector3(1, 0, 0)] = posX;
            _voidMasks[new Vector3(-1, 0, 0)] = negX;
            _voidMasks[new Vector3(0, 0, 1)] = posZ;
            _voidMasks[new Vector3(0, 0, -1)] = negZ;
        }

        public (int offset, Vector3Int direction) CalcOffset(Range3D region, OffsetMode mode = OffsetMode.Max)
        {
            if (_voidMasks.Values.Count == 0)
            {
                CalcVoidMasks();
            }

            /*
             * Find dimension that is either an exact fit/touching.
             * If there is no such dimension, try each of the three dimensions, in order y, x, z.
             */
            var offsetDimensionIndex = region.OffsetDimensionIndex();

            /*
             * Sign is used to denote whether the void mask in positive or negative direction should be used.
             * The sign of the direction can be found by exploiting the fact that
             * it can only be the negative sign if the region's origin value at the offset
             * direction index matches that of the component's.
             */
            var sign = Vector3Util.GetByAxis(Origin, offsetDimensionIndex) 
                       == Vector3Util.GetByAxis(region.Min(), offsetDimensionIndex) ? -1 : 1;
            var direction = Vector3Util.SetByAxis(new Vector3Int(0, 0, 0), offsetDimensionIndex, sign);

            var relativeOrigin = region.Min() - Origin;
            var relativeExtent = region.Max() - Origin;

            // Go over all axes and only include those not equal to the offset dimension index.
            // This is used to determine the range that needs to be checked in the void mask.
            // Should the region lie outside of the component's grid's bounds, cap.

            var voidMaskStart = new List<int>();
            var voidMaskEnd = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                if (i == offsetDimensionIndex) continue;
                voidMaskStart.Add(Math.Max(0, Vector3Util.GetByAxis(relativeOrigin, i)));
                voidMaskEnd.Add(Math.Min(Vector3Util.GetByAxis(Grid.GetExtent(), i) , Vector3Util.GetByAxis(relativeExtent, i)));
            }

            var voidMask = _voidMasks[direction];

            // To find proper adjacency of components, the offset is equal to the largest number of voids.
            var offset = voidMask[0, 0];
            int avg = 0;
            int nItems = 0;
            var items = new List<int>();
            for (int i0 = voidMaskStart[0]; i0 < voidMaskEnd[0]; i0++)
            {
                for (int i1 = voidMaskStart[1]; i1 < voidMaskEnd[1]; i1++)
                {
                    var nVoids = voidMask[i0, i1]; 
                    switch (mode)
                    {
                        case OffsetMode.Max: 
                            if (nVoids < offset) continue;
                            offset = nVoids;
                            break;
                        case OffsetMode.Min:
                            if (nVoids > offset) continue;
                            offset = nVoids;
                            break;
                        case OffsetMode.Average:
                            avg += nVoids;
                            nItems++;
                            break;
                        case OffsetMode.Median:
                            items.Add(nVoids);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                    }
                }
            }
            
            items.Sort();
            
            return mode switch
            {
                OffsetMode.Average => ((int)(avg / (1.0f * nItems)), direction),
                OffsetMode.Median => (items[(int)(items.Count * 0.5)], direction),
                _ => (offset, direction)
            };
        }
    }

    public enum OffsetMode
    {
        Max = 0,
        Min = 1,
        Average = 2,
        Median = 3
    }
}