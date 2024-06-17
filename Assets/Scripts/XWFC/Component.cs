using Unity.VisualScripting;
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
        public InputGrid[] InputGrids;
        public Grid<int> Grid;

        public Component(Vector3Int origin, Vector3Int extent, TileSet tileSet, InputGrid[] inputGrids)
        {
            Origin = origin;
            Extent = extent;
            Tiles = tileSet;
            InputGrids = inputGrids;
            Grid = new Grid<int>(Extent, -1);
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

        public void CalcVoidMasks(Range3D region)
        {
            /*
             * Find dimension that is either an exact fit/touching.
             * If there is no such dimension, try each of the three dimensions, in order y, x, z.
             */
            var thisRange = new Range3D(Origin, Origin + Extent);
            var isZeroLength = region.IsZero();

            // Follows the yxz axis indices. Index corresponds to the orientation of the supposed plane of intersection. 
            var offsetDimensionIndex = isZeroLength.y ? 0 : isZeroLength.x ? 1 : isZeroLength.z ? 2 : -1;
            
            if (offsetDimensionIndex == -1)
            {
                offsetDimensionIndex = 0; 
                // Intersection region is 3D.
                // So, any of the dimension are valid. For now, assume y direction.
            }
        }
    }
}