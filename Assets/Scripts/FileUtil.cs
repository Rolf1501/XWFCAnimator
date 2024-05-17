using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


public class FileUtil
{
    public static void WriteToFile(string content, string path, bool append=true)
    {
        var file = GetFile(path);
        if (append) File.AppendAllText(file, content);
        else File.WriteAllText(file, content);
    }

    public static string ReadFromFile(string path)
    {
        var file = GetFile(path);
        return File.ReadAllText(path);
    }

    public static string GetFile(string path)
    {
        if (File.Exists(path)) return path;
        var file = File.Create(path);
        file.Close();
        return path;
    }

    public static IEnumerable<string> FindFiles(string folderPath, string filePattern)
    {
        if (!Directory.Exists(folderPath)) return new string[]{};
        var files = Directory.EnumerateFiles(folderPath, filePattern);
        return files;
    }

    public static string RootPathTo(string folderName)
    {
        var dir = Directory.GetCurrentDirectory();
        var outPath = Path.Join(dir, $"/Assets/Scripts/{folderName}");
        if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

        return outPath;
    }
    
    public static string GetTimeStamp()
    {
        var timeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        return timeStamp.Replace("-","").Replace("/", "-").Replace("\\","-").Replace(" ", "-").Replace(":", "");
    }
}
