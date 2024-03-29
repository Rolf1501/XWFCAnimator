using System.Collections.Generic;
using UnityEngine.UIElements;
using XWFC;

public class GridGenerator
{
    private List<int> _tiles;
    public Bidict<int, int> TileToIndexMapping { get; }

    public string RowClass;
    public string CellClass;
    
    public GridGenerator(List<int> tiles, string rowClass = "row", string cellClass = "cell")
    {
        _tiles = tiles;
        RowClass = rowClass;
        CellClass = cellClass;

        TileToIndexMapping = new Bidict<int,int>();
        var i = 0;
        foreach (var tile in _tiles)
        {
            TileToIndexMapping.AddPair(tile, i);
            i++;
        }
    }

    public VisualElement Generate()
    {
        var gridContainer = new VisualElement();
        for (int j = 0; j < _tiles.Count; j++)
        {
            var row = GenerateRow();
            for (int k = 0; k < _tiles.Count; k++)
            {
                var cell = GenerateCellToggle(TileToIndexMapping.GetKey(j), TileToIndexMapping.GetKey(k));
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

    private VisualElement GenerateCellToggle(int x, int y)
    {
        var cell = new VisualElement();
        var toggle = new Toggle();
        cell.name = $"{x}{y}";
        cell.AddToClassList(CellClass);
        cell.Add(toggle);
        return cell;
    }
    
}
