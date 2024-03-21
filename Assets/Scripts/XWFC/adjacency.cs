using System.Collections.Generic;
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