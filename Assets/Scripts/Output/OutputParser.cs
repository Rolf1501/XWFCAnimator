using System.IO;
using System.Text;
using UnityEngine;
using XWFC;
using System.Linq;

namespace Output
{
    public class OutputParser
    {
        private string _directoryPath;
        private readonly string _fileName;

        public OutputParser(string directory, string fileName)
        {
            _directoryPath = directory;
            _fileName = fileName;
            InitConfig();
            InitStateAction();
        }

        public void CreateFileName()
        {
            
        }

        public string CreateFile(string filePreFix)
        {
            var path = FilePath(filePreFix + _fileName);
            if (!File.Exists(path)) File.Create(path);
            return path;
        }

        public void InitConfig()
        {
            var builder = new StringBuilder();
            var file = CreateFile("config-");
            string header = "gridSizeX,gridSizeY,gridSizeZ,randomSeed,adjacencyMatrix";
            builder.Append(header);
            builder.Append("\n");
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteConfig(Vector3 gridSize, string randomSeed, AdjacencyMatrix adjacencyMatrix)
        {
            var builder = new StringBuilder();
            var file = CreateFile("config-");
            builder.Append($"{gridSize.x},{gridSize.y},{gridSize.z},{randomSeed},{adjacencyMatrix}");
            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }

        public void InitStateAction()
        {
            var builder = new StringBuilder();
            var file = CreateFile("state-action-");
            builder.Append("state,action");
            builder.Append("\n");
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteStateAction(GridManager gridManager, int[] action)
        {
            var builder = new StringBuilder();
            var file = CreateFile("state-action-");
            var flattened  = gridManager.ChoiceBooleans.Flatten();
            var intFlattened = flattened.SelectMany(x => x.Select(x0 => x0 ? 1 : 0).ToArray()).ToArray();
            foreach (var t in intFlattened)
            {
                builder.Append(t + ",");
            }

            foreach (var value in action)
            {
                builder.Append(value + ",");
            }

            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }
        
        public string FilePath(string fileName)
        {
            return Path.Join(_directoryPath, fileName);
        }

        
        /*
         * Random seed, tile to atoms?, constraints, tiles
         * Counter, grid, collapse queue.
         */
    }
}