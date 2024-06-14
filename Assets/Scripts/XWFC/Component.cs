using UnityEngine;

namespace XWFC
{
    public class Component
    {
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

        public (Range xRange, Range yRange, Range zRange) Ranges()
        {
            return (XRange(), YRange(), ZRange());
        }
    }
}