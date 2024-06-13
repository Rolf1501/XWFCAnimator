using UnityEngine;

namespace XWFC
{
    public class Component
    {
        public Vector3 Source;
        public Vector3 Extent;
        public TileSet Tiles;
        public InputGrid[] InputGrids;
        public Grid<int> Grid;

        public Component(Vector3 source, Vector3 extent, TileSet tileSet, InputGrid[] inputGrids)
        {
            Source = source;
            Extent = extent;
            Tiles = tileSet;
            InputGrids = inputGrids;
            Grid = new Grid<int>(Extent, -1);
        }
    }
}