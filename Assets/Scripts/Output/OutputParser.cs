using System;
using System.Globalization;
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
        private readonly string _timeStamp;
        private const string StateActionPrefix = "state-action";
        private const string ConfigPrefix = "config";
        private const string StateActionQueuePrefix = "state-action-queue";

        public OutputParser(string directory, string fileName)
        {
            _directoryPath = directory;
            _fileName = fileName;
            // Time stamp used to uniquely identify data files.
            _timeStamp = FormatTimeStamp(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            InitConfig();
            InitStateAction();
            InitStateActionQueue();
        }

        private static string FormatTimeStamp(string timeStamp)
        {
            return timeStamp.Replace("-","").Replace("/", "-").Replace("\\","-").Replace(" ", "-").Replace(":", "");
        }

        public string CreateFile(string filePreFix)
        {
            var path = FilePath($"{filePreFix}-{_timeStamp}-{_fileName}");
            if (File.Exists(path)) return path;
            var file = File.Create(path);
            file.Close();
            return path;
        }

        public void InitConfig()
        {
            var builder = new StringBuilder();
            var file = CreateFile(ConfigPrefix);
            string header = "gridSizeX,gridSizeY,gridSizeZ,randomSeed,adjacencyMatrix";
            builder.Append(header + "\n");
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteConfig(Vector3 gridSize, string randomSeed, AdjacencyMatrix adjacencyMatrix)
        {
            /*
             * Structure:
             * w,h,d,seed: 1 cell each.
             * adjacency matrix: nOffsets * nTiles * nTiles cells.
             */
            var builder = new StringBuilder();
            var file = CreateFile(ConfigPrefix);
            builder.Append($"{gridSize.x},{gridSize.y},{gridSize.z},{randomSeed},{adjacencyMatrix.AtomAdjacencyMatrixToFlattenedString()}");
            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
            
        }

        public void InitStateAction()
        {
            var builder = new StringBuilder();
            var file = CreateFile(StateActionPrefix);
            builder.Append("state,action");
            builder.Append("\n");
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteStateAction(GridManager gridManager, int[] action)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             */
            var builder = StateActionEntry(gridManager, action);
            var file = CreateFile(StateActionPrefix);
            

            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }

        private StringBuilder StateActionEntry(GridManager gridManager, int[] action)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             */
            var builder = new StringBuilder();
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

            return builder;
        }

        private void InitStateActionQueue()
        {
            var builder = new StringBuilder();
            var file = CreateFile(StateActionQueuePrefix);
            builder.Append("state,action,queue");
            builder.Append("\n");
            File.WriteAllText(file, builder.ToString());
        }

        public void WriteStateActionQueue(GridManager gridManager, int[] action,
            CollapsePriorityQueue collapsePriorityQueue)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             * - collapse queue: remainder of cells.
             */
            var builder = StateActionEntry(gridManager, action);
            var file = CreateFile(StateActionQueuePrefix);
            foreach (var collapse in collapsePriorityQueue.List)
            {
                builder.Append(collapse + ",");
            }
            File.AppendAllText(file, builder.ToString());
        }
        
        public string FilePath(string fileName)
        {
            return Path.Join(_directoryPath, fileName);
        }
    }
}