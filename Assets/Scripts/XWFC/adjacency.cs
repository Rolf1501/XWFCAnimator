using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace XWFC
{
    public class Adjacency
    {
        public int Source { get; set; }
        public List<Relation> Relations { get; set; }
        public Vector3 Offset { get; set; }
        public bool Symmetric { get; set; } = true;

        public Adjacency(int source, List<Relation> relations, Vector3 offset)
        {
            Source = source;
            Relations = relations;
            Offset = offset;
        }
        
        public Adjacency(int source, IEnumerable<int> relations, Vector3 offset)
        {
            Source = source;
            Relations = relations.Select(x => new Relation(x)).ToList();
            Offset = offset;
        }

        public string ToJson()
        {
            var relations = new List<string>();
            var nRelations = Relations.Count;
            // Relations.ForEach(s => relations.Add(s.ToJson()));
            // var builder = new StringBuilder();
            var output = new Dictionary<string, string>()
            {
                { "source", Source.ToString() },
                // {"relations", JsonConvert.SerializeObject(relations)}, 
                { "offset", Vector3Util.Vector3ToString(Offset) }
            };
            for (int i = 0; i < nRelations; i++)
            {
                relations.Add(Relations[i].ToJson());
                // output[$"relation{i.ToString()}"] = Relations[i].ToJson();
            }

            output["relations"] = JsonConvert.SerializeObject(relations);
            return JsonConvert.SerializeObject(output);
        }

        public Adjacency(string source, string relations, string offset)
        {
            Source = int.Parse(source);
            var rels = JsonConvert.DeserializeObject<List<string>>(relations);
            Relations = rels.Select(s => Relation.FromJson(s)).ToList();
            // var rels = JsonFormatter.ListTrimSplit(relations);
            
            Offset = Vector3Util.Vector3FromString(offset);
        }

        public static Adjacency FromJson(string s)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(s);
            // var dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(s);
            var rels = JsonConvert.DeserializeObject<List<string>>(dict["relations"]);

            return new Adjacency(dict["source"], dict["relations"], dict["offset"]);

            // return new Adjacency(
            //     int.Parse(dict["source"]), 
            //     rels.Select(Relation.FromJson).ToList(), 
            //     Vector3Util.Vector3FromString(dict["offset"]));
        }

        // public Adjacency(int source, List<Relation> relations, Vector3 offset, bool symmetric = true)
        // {
        //     Source = source;
        //     Relations = relations;
        //     Offset = offset;
        //     Symmetric = symmetric;
        // }

        // TODO: write to file.
        // public static Adjacency FromJson(string json)
        // {
        //     var options = new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true
        //     };
        //     return JsonSerializer.Deserialize<Adjacency>(json, options);
        // }

        // public string ToJson()
        // {
        //     var options = new JsonSerializerOptions
        //     {
        //         WriteIndented = true
        //     };
        //     return JsonSerializer.Serialize(this, options);
        // }
    }
}