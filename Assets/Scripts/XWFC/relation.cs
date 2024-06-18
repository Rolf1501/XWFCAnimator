using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace XWFC
{
    public record Relation
    {
        public int Other { get; }
        public float Weight { get; }
        public int[] Rotations { get; } = { 0 };

        #nullable enable
        public Relation(int other, int[]? rotations, float weight = 1)
        {
            Other = other;
            Rotations = rotations ?? new[] { 0 };
            Weight = weight;
        }

        public Relation(string other, string rotations, string weight)
        {
            Other = int.Parse(other);
            var list = StringUtil.ListTrimSplit(rotations.Replace("\"", ""));
            Rotations = list.Select(int.Parse).ToArray();
            Weight = float.Parse(weight);
        }

        public string ToJson()
        {
            var rels = new StringBuilder();
            rels.Append("[");
            foreach (var r in Rotations) rels.Append($"\"{r}\",");
            rels.Remove(rels.Length - 1, 1);
            rels.Append("\"]");
            var dict = new Dictionary<string, string>()
            {
                { "other", Other.ToString() },
                { "weight", Weight.ToString() },
                { "rotations", rels.ToString() }
            };
            return JsonConvert.SerializeObject(dict);
            // return JsonConvert.SerializeObject(this);
        }

        public static Relation FromJson(string s)
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
            if (json == null) throw new Exception($"Cannot deserialize input {s} to JSON.");
            return new Relation(json["other"], json["rotations"], json["weight"]);
            // return new Relation(int.Parse(json["other"]), rotations, float.Parse(json["weight"]));
        }
    }
}