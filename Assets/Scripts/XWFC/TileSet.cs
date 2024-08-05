using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace XWFC
{
    public class TileSet : Dictionary<int, NonUniformTile>
    {
        public static TileSet FromDict(Dictionary<int, NonUniformTile> dictionary)
        {
            var tileSet = new TileSet();
            foreach (var (key, value) in dictionary)
            {
                tileSet.Add(key,value);
            }

            return tileSet;
        }

        public static TileSet FromJson(string s)
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
            var tiles = new TileSet();
            foreach (var (k,v )in json)
            {
                var id = int.Parse(k);
                var terminal = NonUniformTile.FromJson(JsonConvert.DeserializeObject<Dictionary<string,string>>(v));
                tiles.Add(id, terminal);
            }

            return tiles;
        }

        public int GetTileIdFromValue(string value)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].UniformAtomValue.Equals(value)) return i;
            }

            return -1;
        }
        
        public TileSet GetSubset(IEnumerable<string> tileNames)
        {
            var subset = new TileSet();
            var list = tileNames.ToArray();
            for (var i = 0; i < this.Count; i++)
            {
                var tile = this[i];
                if (list.Contains(tile.UniformAtomValue))
                {
                    subset[i] = tile;
                }
            }

            return subset;
        }
    }
}