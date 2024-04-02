using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using XWFC;

public class AdjacencyGridController
{
    private List<int> _tiles;
    public Bidict<int, int> TileToIndexMapping { get; }

    public string RowClass;
    public string CellClass;
    public string GridHeaderClass = "header";
    private const string TogglePrefix = "toggle";

    private Dictionary<Vector3, float[,]> _backendGrid;
    private readonly Vector3[] _offsets;
    private readonly HashSetAdjacency _activeAdjacency;
    private const int DefaultValue = -1;

    public Dictionary<Vector3, VisualElement> Grids { get; }

    public AdjacencyGridController(List<int> tiles, HashSetAdjacency adjacencySet, Vector3[] offsets, string rowClass = "row", string cellClass = "cell")
    {
        _tiles = tiles;
        _offsets = offsets;
        _activeAdjacency = adjacencySet;
        
        RowClass = rowClass;
        CellClass = cellClass;

        TileToIndexMapping = new Bidict<int,int>();
        for (var i = 0; i < _tiles.Count; i++)
        {
            TileToIndexMapping.AddPair(_tiles[i], i);
        }

        InitBackendGrid(_tiles.Count, adjacencySet);
        Grids = new Dictionary<Vector3, VisualElement>();
        foreach (var offset in _offsets)
        {
            Grids[offset] = GenerateGrid(offset);
        }

        UpdateTogglesFront(_activeAdjacency);
    }

    private void UpdateTogglesFront(HashSetAdjacency activeAdjacency)
    {
        /*
         * Updates the states of the toggles to match the listed adjacency constraints.
         */
        foreach (var adjacency in activeAdjacency)
        {
            foreach (var relation in adjacency.Relations)
            {
                UpdateToggleFront(adjacency.Source, relation.Other, adjacency.Offset, true);
                UpdateToggleFront(relation.Other, adjacency.Source, Vector3Util.Negate(adjacency.Offset), true);
            }
        }
    }

    private void InitBackendGrid(int nTiles, HashSetAdjacency adjacencySet)
    {
        /*
         * Initializes the grid used in the backend for keeping track of adjacency constraints.
         * TODO: maintain existing adjacency pairs when removing or adding a tile.
         */
        _backendGrid = new Dictionary<Vector3, float[,]>();
        foreach (var offset in _offsets)
        {
            _backendGrid[offset] = Float2D(nTiles,nTiles, DefaultValue);
        }

        foreach (var adjacency in adjacencySet)
        {
            foreach (var relation in adjacency.Relations)
            {
                // Symmetric pairs.
                int source = TileToIndexMapping.GetValue(adjacency.Source);
                int other = TileToIndexMapping.GetValue(relation.Other);
                _backendGrid[adjacency.Offset][source, other] = relation.Weight;
                _backendGrid[Vector3Util.Negate(adjacency.Offset)][other, source] = relation.Weight;
            }
        }
    }

    private static float[,] Float2D(int width, int height, float value = 0)
    {
        var float2d = new float[height, width];
        if (value == 0.0f) return float2d;
        
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float2d[i, j] = value;
            }
        }
        return float2d;
    }

    private VisualElement GenerateGrid(Vector3 offset)
    {
        /*
         * Generates the back- and frontend of the grid representation of the adjacency constraints. 
         * First row and first column: tile ids. 
         */
        var gridContainer = new VisualElement();
        var cellSize = Length.Percent(100.0f / (_tiles.Count + 1));
        var header = GenerateHeader(cellSize);
        gridContainer.Add(header);
        
        for (int j = 0; j < _tiles.Count; j++)
        {
            var row = GenerateRow();
            var headerLabel = GenerateLabel($"{_tiles[j]}", new List<string>{CellClass});
            var rowLabel = GenerateLabel($"{_tiles[j]}", new List<string>{CellClass});
            row.Add(rowLabel);
            header.Add(headerLabel);
            UpdateCellSize(rowLabel, new StyleLength(cellSize));
            UpdateCellSize(headerLabel, new StyleLength(cellSize));
            
            for (int k = 0; k < _tiles.Count; k++)
            {
                // Uses the actual tile ids, not their index.
                var cell = GenerateCellToggleWithCallback(TileToIndexMapping.GetKey(j), TileToIndexMapping.GetKey(k), offset);
                row.Add(cell);
                UpdateCellSize(cell, new StyleLength(cellSize));
            }
            gridContainer.Add(row);
        }

        return gridContainer;
    }

    private VisualElement GenerateHeader(Length cellSize)
    {
        var header = GenerateRow();
        header.AddToClassList(GridHeaderClass);
        var empty = GenerateLabel("", new List<string>{CellClass});
        empty.style.width = new StyleLength(cellSize);
        empty.style.height = new StyleLength(cellSize);
        
        header.Add(empty);
        return header;
    }

    private void UpdateCellSize(VisualElement cell, StyleLength length)
    {
        cell.style.width = length;
        cell.style.height = length;
    }

    private VisualElement GenerateLabel(string text, List<string> classNames)
    {
        var container = new VisualElement();
        var label = new TextElement();
        label.text = text;
        foreach (string name in classNames) container.AddToClassList(name);
        container.Add(label);
        return container;
    }

    private VisualElement GenerateRow()
    {
        var row = new VisualElement();
        row.AddToClassList(RowClass);
        return row;
    }

    private VisualElement GenerateCellToggleWithCallback(int source, int other, Vector3 offset)
    {
        /*
         * Generates a single cell visual element with a toggle.
         * Registers the callback for said toggle.
         */
        var cell = new VisualElement();
        var toggle = new Toggle();
        toggle.name = GenerateToggleName(source, other);
        cell.AddToClassList(CellClass);
        cell.Add(toggle);
        toggle.RegisterValueChangedCallback(delegate { UpdateToggleValues(source, other, offset, toggle.value);});
        return cell;
    }

    private void UpdateToggleValues(int source, int other, Vector3 offset, bool value)
    {
        /*
         * Updates the toggle values in both the front- and backend.
         */
        UpdateToggleBack(source, other, offset, value); 
        UpdateToggleFront(source, other, offset, value);
    }

    private static string GenerateToggleName(int source, int other)
    {
        return TogglePrefix + $"{source}-{other}";
    }

    public void UpdateToggleFront(int source, int other, Vector3 offset, bool value, bool symmetric=true)
    {
        /*
         * Updates the toggle value in the frontend.
         */
        Toggle toggle = Grids[offset].Q<Toggle>(name: GenerateToggleName(source, other));
        if (symmetric)
        {
            var toggleSym = Grids[Vector3Util.Negate(offset)].Q<Toggle>(name: GenerateToggleName(other, source));
            toggleSym.value = value;
        }
        toggle.value = value;
    }

    public void UpdateToggleBack(int source, int other, Vector3 offset, bool value, bool symmetric=true)
    {
        /*
         * Updates the toggle value in the backend.
         */
        var val = value ? 1.0f : DefaultValue;
        int sourceIndex = TileToIndexMapping.GetValue(source);
        int otherIndex = TileToIndexMapping.GetValue(other);
        _backendGrid[offset][sourceIndex, otherIndex] = val;
        if (symmetric) _backendGrid[Vector3Util.Negate(offset)][otherIndex, sourceIndex] = val;

        Debug.Log($"CHANGES CELL: {source} {other}.\nUPDATED GRID: {offset} and {Vector3Util.Negate(offset)}");
        
    }

    public HashSetAdjacency ToAdjacencySet()
    {
        /*
         * Represents the adjacency constraint grid as a set of adjacency constraints.
         */
        var set = new HashSetAdjacency();
        for (int i = 0; i < _tiles.Count; i++)
        {
            foreach (var offset in _offsets)
            {
                var relations = new List<Relation>();
                var adj = new Adjacency(_tiles[i], relations, offset);
                for (int j = 0; j < _tiles.Count; j++)
                {
                    if (Math.Abs(_backendGrid[offset][i,j] - DefaultValue) > 0.001) adj.Relations.Add(new Relation(_tiles[j], null));
                }

                set.Add(adj);
            }
        }

        return set;
    }
    
}
