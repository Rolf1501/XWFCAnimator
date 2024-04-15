using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace XWFC
{
    public class TileSet : Dictionary<int, Terminal>
    {
        public static TileSet FromDict(Dictionary<int, Terminal> dictionary)
        {
            var tileSet = new TileSet();
            foreach (var (key, value) in dictionary)
            {
                tileSet.Add(key,value);
            }

            return tileSet;
        }
    }
}