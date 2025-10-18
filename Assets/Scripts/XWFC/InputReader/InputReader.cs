using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XWFC;

#nullable enable
public class InputReader
{
    public InputReader()
    {

    }


    public void ReadPattern(TileSet nutSet)
    {

    }

    public TileSet ReadNUT()
    {
        using (var reader = new StreamReader(Path.Join(FileUtil.GetPathToScripts(), @"XWFC/SampleInput.txt")))
        {
            var tileSet = new TileSet();
            int nutCount = int.Parse(reader.ReadLine());
            int counter = 0;

            while (counter < nutCount)
            {
                NonUniformTile? nut = ParseNut(reader);
                if (nut != null)
                {
                    tileSet[counter] = nut;
                }
                counter++;
            }

            return tileSet;
        }
    }

    private NonUniformTile? ParseNut(StreamReader reader)
    {
        var line = reader.ReadLine();
        if (line == null) return null;
        var nutDescriptor = line;
        var colorText = reader.ReadLine().Split(",");
        if (colorText.Length != 3) return null;

        var colorValues = colorText.Select(c => float.Parse(c)).ToArray();
        var color = new Color(colorValues[0], colorValues[1], colorValues[2]);
        var extent = reader.ReadLine().Split(",");
        if (extent.Length != 3) return null;

        var width = Convert.ToInt32(extent[0]);
        var height = Convert.ToInt32(extent[1]);
        var depth = Convert.ToInt32(extent[2]);

        var mask = new bool[height, width, depth];

        for (int y = height - 1; y >= 0; y--)
        {
            for (int z = depth - 1; z >= 0; z--)
            {
                line = reader.ReadLine();
                var items = line.Split(",");
                if (items.Length != width) return null;

                for (int x = 0; x < items.Length; x++)
                {
                    mask[y, x, z] = items[x] == "1";
                }
            }
        }

        return new NonUniformTile(nutDescriptor, new Vector3Int(width, height, depth), color, mask);
    }
}