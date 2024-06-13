using UnityEngine;

namespace XWFC
{
    public class Component
    {
        public Vector3 Source;
        public Vector3 Extent;
        public TileSet Tiles;
        public InputGrid InputGrid;
        public Grid<int> Grid;

        public Component(Vector3 source, Vector3 extent, TileSet tileSet, InputGrid inputInputGrid)
        {
            Source = source;
            Extent = extent;
            Tiles = tileSet;
            InputGrid = inputInputGrid;
            Grid = new Grid<int>(Extent, -1);
        }
    }
}