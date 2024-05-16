using System.Collections.Generic;
using Newtonsoft.Json;

public class JsonFormatter
{
    public static string Serialize(Dictionary<string,string> s)
    {
        var j = JsonConvert.SerializeObject(s);
        return j;
    }

    public static T Deserialize<T>(string s)
    {
        var j = JsonConvert.DeserializeObject<T>(s);
        return j;
    }

    public static string[] ListTrimSplit(string s)
    {
        return s.Trim(new char[] { '[', ']' }).Split(",");
        
    }
}