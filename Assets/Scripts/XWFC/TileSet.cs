using System.Collections.Generic;
using Newtonsoft.Json;

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

        public static TileSet FromJson(string s)
        {
            var formatter = new JsonFormatter();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
            var tiles = new TileSet();
            foreach (var (k,v )in json)
            {
                var id = int.Parse(k);
                var terminal = Terminal.FromJson(JsonFormatter.Deserialize<Dictionary<string,string>>(v));
                tiles.Add(id, terminal);
            }

            return tiles;
        }
    }
}