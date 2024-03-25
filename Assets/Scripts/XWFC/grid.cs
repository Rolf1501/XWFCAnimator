using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool WithinBounds(int x, int y, int z)
        {
            return x < Width && y < Height && z < Depth && x >= 0 && y >= 0 && z >= 0;
        }

        public T Get(int x, int y, int z)
        {
            return _grid[y, x, z];
        }

        public T Get(Vector3 coord)
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

        public void Set(Vector3 coord, T value)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            Set(x, y, z, value);
        }
        
        
        //
        // public void PrintXY()
        // {
        //     for (int z = 0; z < Depth; z++)
        //     {
        //         for (int y = 0; y < Height; y++)
        //         {
        //             for (int x = 0; x < Width; x++)
        //             {
        //                 Console.Write(_grid[y, x, z] + " ");
        //             }
        //             Console.WriteLine();
        //         }
        //         Console.WriteLine();
        //     }
        // }
        //
        // public void PrintXZ()
        // {
        //     for (int y = 0; y < Height; y++)
        //     {
        //         for (int x = 0; x < Width; x++)
        //         {
        //             for (int z = 0; z < Depth; z++)
        //             {
        //                 Console.Write(_grid[y, x, z] + " ");
        //             }
        //             Console.WriteLine();
        //         }
        //         Console.WriteLine();
        //     }
        // }
        //
        // public void PrintYZ()
        // {
        //     for (int x = 0; x < Width; x++)
        //     {
        //         for (int y = 0; y < Height; y++)
        //         {
        //             for (int z = 0; z < Depth; z++)
        //             {
        //                 Console.Write(_grid[y, x, z] + " ");
        //             }
        //             Console.WriteLine();
        //         }
        //         Console.WriteLine();
        //     }
        // }
        //
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
                        s += _grid[y, x, z] + ", ";
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
        public Grid(Vector3 extent, T defaultFillValue) : base((int)extent.x, (int)extent.y, (int)extent.z, defaultFillValue)
        {
            Populate(defaultFillValue);
        }
        public Grid(int width, int height, int depth, T defaultFillValue) : base(width, height, depth, defaultFillValue)
        {
            Populate(defaultFillValue);
        }

        public bool IsChosen(int x, int y, int z)
        {
            var choice = Get(x, y, z);
            // Larger than 0: choice > default
            return DefaultFillValue != null
                ? Comparer<T>.Default.Compare(choice, DefaultFillValue) > 0
                : choice != null;
        }

        public bool IsChosen(Vector3 coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return IsChosen(x, y, z);
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

    public class GridManager
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public Grid<int> Grid { get; }
        public Grid<float> Entropy { get; }
        public Grid<bool[]> ChoiceBooleans { get; }
        public Grid<int[]> ChoiceIds { get; }
        public Grid<float[]> ChoiceWeights { get; }

        public GridManager(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Grid = new Grid<int>(width, height, depth, -1);
            Entropy = new Grid<float>(width, height, depth, -1.0f);
            ChoiceBooleans = new Grid<bool[]>(width, height, depth, new bool[1]);
            ChoiceIds = new Grid<int[]>(width, height, depth, new int[1]);
            ChoiceWeights = new Grid<float[]>(width, height, depth, new float[1]);
        }

        private GridManager(int width, int height, int depth, Grid<int> grid, Grid<float> entropy,
            Grid<bool[]> choiceBooleans, Grid<int[]> choiceIds,
            Grid<float[]> choiceWeights)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Grid = grid;
            Entropy = entropy;
            ChoiceBooleans = choiceBooleans;
            ChoiceIds = choiceIds;
            ChoiceWeights = choiceWeights;
        }

        public GridManager Deepcopy()
        {
            return new GridManager(Width, Height, Depth,
                Grid.Deepcopy(), Entropy.Deepcopy(), ChoiceBooleans.Deepcopy(), ChoiceIds.Deepcopy(),
                ChoiceWeights.Deepcopy());
        }

        public void InitChoiceWeights(Dictionary<int, float> defaultWeights)
        {
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int d = 0; d < Depth; d++)
                    {
                        ChoiceBooleans.Set(w, h, d, Enumerable.Repeat(true, defaultWeights.Count).ToArray());
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

        public bool WithinBounds(Vector3 coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return WithinBounds(x, y, z);
        }
    }
}