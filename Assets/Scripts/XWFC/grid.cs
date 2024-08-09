using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace XWFC
{
    public abstract class AbstractGrid<T>
    {
        protected int Width { get; }
        protected int Height { get; }
        protected int Depth { get; }
        public T DefaultFillValue { get; }
        private T[,,] _grid;
        
        protected AbstractGrid(int width, int height, int depth, T defaultFillValue)
        {
            Width = width;
            Height = height;
            Depth = depth;
            DefaultFillValue = defaultFillValue;
            _grid = new T[Height, Width, Depth];
        }

        protected AbstractGrid(Vector3 extent, T defaultFillValue)
        {
            (Width, Height, Depth) = Vector3Util.CastInt(extent);
            DefaultFillValue = defaultFillValue;
            _grid = new T[Height, Width, Depth];
        }
        
        protected AbstractGrid(T[,,] grid, T defaultFillValue)
        {
            Height = grid.GetLength(0);
            Width = grid.GetLength(1);
            Depth = grid.GetLength(2);
            DefaultFillValue = defaultFillValue; 
            _grid = grid;
        }

        public Vector3Int GetExtent()
        {
            return new Vector3Int(Width, Height, Depth);
        }

        public void Populate(T value)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        _grid[i, j, k] = value;
                    }
                }
            }
        }

        public void Map(Action<T> func)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        func(_grid[i, j, k]);
                    }
                }
            }
        }

        public T[,,] GetGrid()
        {
            return _grid;
        }

        public IEnumerable<T1> MapYield<T1>(Func<T, T1> func)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        yield return func(_grid[i, j, k]);
                    }
                }
            }
        }

        public bool WithinBounds(int x, int y, int z)
        {
            return x < Width && y < Height && z < Depth && x >= 0 && y >= 0 && z >= 0;
        }

        public bool WithinBounds(Vector3 vector)
        {
            return WithinBounds((int)vector.x, (int)vector.y, (int)vector.z);
        }

        public T Get(int x, int y, int z)
        {
            return _grid[y, x, z];
        }

        public T Get(Vector3Int coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return Get(x, y, z);
        }

        public void Set(int x, int y, int z, T value)
        {
            if (WithinBounds(x, y, z))
            {
                _grid[y, x, z] = value;
            }
        }

        public void Set(Vector3Int coord, T value)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            Set(x, y, z, value);
        }
        
        public string GridToString()
        {
            string s = "";
            for (int y = 0; y < _grid.GetLength(0); y++)
            {
                s += "[\n";
                for (int x = 0; x < _grid.GetLength(1); x++)
                {
                    s += "[";
                    for (int z = 0; z < _grid.GetLength(2); z++)
                    {
                        s += "(" + string.Join(",", _grid[y, x, z]) + "), ";
                    }
                    s += "]\n";
                }
                s += "\n]\n";
            }
            return s;
        }
    }

    public class Grid<T> : AbstractGrid<T>
    {
        public Grid(Vector3Int extent, T defaultFillValue) : base(extent.x, extent.y, extent.z, defaultFillValue)
        {
            Populate(defaultFillValue);
        }
        public Grid(int width, int height, int depth, T defaultFillValue) : base(width, height, depth, defaultFillValue)
        {
            Populate(defaultFillValue);
        }

        public Grid(T[,,] grid, T defaultFilValue) : base(grid, defaultFilValue){}
        
        public bool IsOccupied(int x, int y, int z)
        {
            var choice = Get(x, y, z);
            // Larger than 0: choice != default
            return DefaultFillValue != null
                ? !EqualityComparer<T>.Default.Equals(choice, DefaultFillValue)
                : choice != null;
        }

        public bool IsOccupied(Vector3Int coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return IsOccupied(x, y, z);
        }

        public Grid<T> Deepcopy()
        {
            var copy = new Grid<T>(Width, Height, Depth, DefaultFillValue);
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            for (int z = 0; z < Depth; z++)
                copy.Set(x, y, z, Get(x, y, z));
            return copy;
        }
    }
    
    public class SampleGrid : AbstractGrid<string>
    {
        private string _voidValue;
        public SampleGrid(int width, int height, int depth, string defaultFillValue="", string voidValue="void") : base(width, height, depth, defaultFillValue)
        {
            _voidValue = voidValue;
            Populate(defaultFillValue);
        }

        public SampleGrid(Vector3 extent, string defaultFillValue = "", string voidValue="void") : base(extent, defaultFillValue)
        {
            _voidValue = voidValue;
            Populate(defaultFillValue);
        }

        public SampleGrid(string[,,] grid, string defaultFillValue, string voidValue="void") : base(grid, defaultFillValue)
        {
            _voidValue = voidValue;
            Populate(defaultFillValue);
        }

        public void PlaceNut(NonUniformTile nut, Vector3Int coord)
        {
            var atomCoords = nut.AtomIndexToIdMapping.Keys;
            foreach (var atomCoord in atomCoords)
            {
                var c = atomCoord + coord;
                var value = Get(c);
                if (!value.Equals(_voidValue) && !value.Equals(DefaultFillValue))
                {
                    throw new Exception($"Tiles may not overlap: {(nut.UniformAtomValue, coord)}, value: {value}");
                }

                var atomValue = nut.GetAtomValue(atomCoord); 
                Set(c, atomValue);
            }
        }
    }

    public class AtomGrid : AbstractGrid<List<int>>
    {
        public AtomGrid(int width, int height, int depth) : base(width, height, depth, new List<int>())
        {
            Populate();
        }

        public AtomGrid(Vector3 extent) : base(extent, new List<int>())
        {
            Populate();
        }

        public AtomGrid(List<int>[,,] grid) : base(grid, new List<int>())
        {
            Populate();
        }

        private void Populate()
        {
            var (x,y,z) = Vector3Util.CastInt(GetExtent());
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        Set(i, j, k, new List<int>());
                    }
                }
            }
        }

        public override string ToString()
        {
            var grid = GetGrid();
            var s = new StringBuilder();
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                s.Append("[\n");
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    s.Append("[");
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        var ss = grid[y, x, z].Aggregate("", (current, item) => current + ("," + item));
                        s.Append("(" + ss + "), ");
                    }
                    s.Append("]\n");
                }
                s.Append("\n]\n");
            }

            return s.ToString();
        }
    }

    public class GridManager
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public Grid<int> Grid;
        public Grid<float> Entropy { get; }
        public Grid<bool[]> Wave { get; }
        public Grid<int[]> ChoiceIds { get; }
        public Grid<float[]> ChoiceWeights { get; }

        public GridManager(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Grid = new Grid<int>(width, height, depth, -1);
            Entropy = new Grid<float>(width, height, depth, -1.0f);
            Wave = new Grid<bool[]>(width, height, depth, new bool[1]);
            ChoiceIds = new Grid<int[]>(width, height, depth, new int[1]);
            ChoiceWeights = new Grid<float[]>(width, height, depth, new float[1]);
        }

        private GridManager(int width, int height, int depth, Grid<int> grid, Grid<float> entropy,
            Grid<bool[]> wave, Grid<int[]> choiceIds,
            Grid<float[]> choiceWeights)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Grid = grid;
            Entropy = entropy;
            Wave = wave;
            ChoiceIds = choiceIds;
            ChoiceWeights = choiceWeights;
        }

        public GridManager(Grid<int> seededGrid, Dictionary<int, float> defaultWeights, float maxEntropy)
        {
            (Width, Height, Depth) = Vector3Util.CastInt(seededGrid.GetExtent());
            Grid = seededGrid;
            
            Entropy = new Grid<float>(Width, Height, Depth, -1.0f);
            Wave = new Grid<bool[]>(Width, Height, Depth, new bool[1]);
            ChoiceIds = new Grid<int[]>(Width, Height, Depth, new int[1]);
            ChoiceWeights = new Grid<float[]>(Width, Height, Depth, new float[1]);
            
            InitChoiceWeights(defaultWeights, seededGrid);
            InitEntropy(maxEntropy);
        }

        public GridManager Deepcopy()
        {
            return new GridManager(Width, Height, Depth,
                Grid.Deepcopy(), Entropy.Deepcopy(), Wave.Deepcopy(), ChoiceIds.Deepcopy(),
                ChoiceWeights.Deepcopy());
        }

        public void InitChoiceWeights(Dictionary<int, float> defaultWeights, [CanBeNull] Grid<int> grid=null)
        {
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int d = 0; d < Depth; d++)
                    {
                        if (grid != null && grid.Get(w,h,d) != grid.DefaultFillValue) continue;
                        Wave.Set(w, h, d, Enumerable.Repeat(true, defaultWeights.Count).ToArray());
                        ChoiceIds.Set(w, h, d, defaultWeights.Keys.ToArray());
                        ChoiceWeights.Set(w, h, d, defaultWeights.Values.ToArray());
                    }
                }
            }
        }

        public void InitEntropy(float maxEntropy)
        {
            Entropy.Populate(maxEntropy);
        }

        public bool WithinBounds(int x, int y, int z)
        {
            return x < Width && y < Height && z < Depth && x >= 0 && y >= 0 && z >= 0;
        }

        public bool WithinBounds(Vector3Int coord)
        {
            return WithinBounds(coord.x, coord.y, coord.z);
        }
    }
}