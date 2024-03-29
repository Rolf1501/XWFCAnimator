using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using XWFC;

public class AdjacencyGridController
{
    private List<int> _tiles;
    public Bidict<int, int> TileToIndexMapping { get; }

    public string RowClass;
    public string CellClass;
    private const string TogglePrefix = "toggle";

    private Dictionary<Vector3, float[,]> _backendGrid;
    private readonly Vector3[] _offsets;
    private HashSetAdjacency _initialAdjacency;
    private readonly HashSetAdjacency _activeAdjacency;
    private const int DefaultValue = -1;
    // public VisualElement Grid { get; }
    public Dictionary<Vector3, VisualElement> Grids { get; }

    public AdjacencyGridController(List<int> tiles, HashSetAdjacency adjacencySet, Vector3[] offsets, string rowClass = "row", string cellClass = "cell")
    {
        _tiles = tiles;
        _offsets = offsets;
        _initialAdjacency = adjacencySet;
        _activeAdjacency = adjacencySet;
        
        RowClass = rowClass;
        CellClass = cellClass;

        TileToIndexMapping = new Bidict<int,int>();
        var i = 0;
        foreach (var tile in _tiles)
        {
            TileToIndexMapping.AddPair(tile, i);
            i++;
        }

        InitBackendGrid(_tiles.Count, adjacencySet);
        Grids = new Dictionary<Vector3, VisualElement>();
        foreach (var offset in _offsets)
        {
            Grids[offset] = GenerateGrid(offset);
        }

        SetTogglesFront(_activeAdjacency);
    }

    private void SetTogglesFront(HashSetAdjacency activeAdjacency)
    {
        foreach (var adjacency in activeAdjacency)
        {
            foreach (var relation in adjacency.Relations)
            {
                SetToggleFront(adjacency.Source, relation.Other, adjacency.Offset, true);
                SetToggleFront(relation.Other, adjacency.Source, Vector3Util.Negate(adjacency.Offset), true);
            }
        }
    }

    private void InitBackendGrid(int nTiles, HashSetAdjacency adjacencySet)
    {
        /*
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
                float2d[height, width] = value;
            }
        }
        return float2d;
    }

    private VisualElement GenerateGrid(Vector3 offset)
    {
        var gridContainer = new VisualElement();
        for (int j = 0; j < _tiles.Count; j++)
        {
            var row = GenerateRow();
            for (int k = 0; k < _tiles.Count; k++)
            {
                // Uses the actual tile ids, not their index.
                var cell = GenerateCellToggleWithCallback(TileToIndexMapping.GetKey(j), TileToIndexMapping.GetKey(k), offset);
                row.Add(cell);
            }
            gridContainer.Add(row);
        }

        return gridContainer;
    }

    private VisualElement GenerateRow()
    {
        var row = new VisualElement();
        row.AddToClassList(RowClass);
        return row;
    }

    private VisualElement GenerateCellToggleWithCallback(int source, int other, Vector3 offset)
    {
        var cell = new VisualElement();
        var toggle = new Toggle();
        toggle.name = GenerateToggleName(source, other);
        cell.AddToClassList(CellClass);
        cell.Add(toggle);
        toggle.RegisterValueChangedCallback(delegate { SetToggleBack(source, other, offset, toggle.value); });
        return cell;
    }
    
    // public static (int source, int other) GetTileIdPair(VisualElement cell)
    // {
    //     var name = cell.name;
    //     name = name.Replace(TogglePrefix, "");
    //     var tileIds = name.Split("-");
    //     
    //     if (tileIds.Length < 1) return (DefaultValue, DefaultValue);
    //     
    //     int source = int.Parse(tileIds[0]);
    //     int other = int.Parse(tileIds[1]);
    //     return (source, other);
    // }

    private string GenerateToggleName(int source, int other)
    {
        return TogglePrefix + $"{source}-{other}";
    }

    public void SetToggleFront(int source, int other, Vector3 offset, bool value, bool symmetric=true)
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

    public void SetToggleBack(int source, int other, Vector3 offset, bool value, bool symmetric=true)
    {
        /*
         * Updates the toggle value in the backend.
         */
        var val = value ? 1.0f : DefaultValue;
        int sourceIndex = TileToIndexMapping.GetValue(source);
        int otherIndex = TileToIndexMapping.GetValue(other);
        _backendGrid[offset][sourceIndex, otherIndex] = val;
        if (symmetric) _backendGrid[Vector3Util.Negate(offset)][otherIndex, sourceIndex] = val;
    }

    public void AddToggleListeners<T1, T2>(Func<T1, T2> func)
    {
        
    }
    
}
