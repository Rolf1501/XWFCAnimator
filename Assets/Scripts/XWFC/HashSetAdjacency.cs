using System.Collections.Generic;
using Newtonsoft.Json;

namespace XWFC
{
    public class HashSetAdjacency : HashSet<Adjacency>
    {
        // Class for shorthand notation of set of adjacency constraints.
        public string ToJson()
        {
            var dict = new Dictionary<string, string>();
            var set = new HashSet<string>();
            
            foreach (var adj in this)
            {
                set.Add(adj.ToJson());
            }

            dict["adjacencyConstraints"] = JsonConvert.SerializeObject(set);
            var json = JsonConvert.SerializeObject(dict);
            return json;
        }

        public static HashSetAdjacency FromJson(string s)
        {
            var set = new HashSetAdjacency();
            
            var dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(s);
            foreach (var (k,v) in dict)
            {
                var hashSetString = JsonConvert.DeserializeObject<HashSet<string>>(v);
                foreach (var adjacencyString in hashSetString)
                {
                    var adjDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(adjacencyString);
                    var adjacency = new Adjacency(adjDict["source"], adjDict["relations"], adjDict["offset"]);
                    set.Add(adjacency);
                }
            }
            return set;
        }
    } 
}