using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Net;

namespace XWFC
{
    public class TrainingDataFormatter
    {
        private string _directoryPath;
        private readonly string _fileName;
        private string _timeStamp;
        private const string StateActionPrefix = "state-action";
        private const string ConfigPrefix = "config";
        private const string StateActionQueuePrefix = "state-action-queue";
        private string _configPath;
        private string _stateActionPath;
        private string _stateActionQueuePath;

        public TrainingDataFormatter(string directory, string fileName)
        {
            _directoryPath = directory;
            _fileName = fileName;
            // Time stamp used to uniquely identify data files.
            InitTimeStamp();
            InitConfig();
            InitStateAction();
            InitStateActionQueue();
        }

        private void InitTimeStamp()
        {
            _timeStamp = FormatTimeStamp(DateTime.Now.ToString(CultureInfo.InvariantCulture));
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
            Debug.Log($"Created file {path}");
            return path;
        }

        public void InitConfig()
        {
            var builder = new StringBuilder();
            var file = CreateFile(ConfigPrefix);
            string header = "gridSizeX,gridSizeY,gridSizeZ,randomSeed,adjacencyMatrix,atomMapping(atomId-atomCoord-tileId)";
            builder.Append(header + "\n");
            _configPath = file;
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
            var adjMatrixString = adjacencyMatrix.AtomAdjacencyMatrixToFlattenedString();
            builder.Append($"{gridSize.x},{gridSize.y},{gridSize.z},{randomSeed},");
            builder.Append(adjMatrixString);
            if (!adjMatrixString.EndsWith(",")) builder.Append(",");
            builder.Append(adjacencyMatrix.AtomMappingToString());
            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }

        public void InitStateAction()
        {
            var builder = new StringBuilder();
            var file = CreateFile(StateActionPrefix);
            builder.Append("state,action");
            builder.Append("\n");
            _stateActionPath = file;
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteStateAction(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             */
            var builder = StateActionEntry(gridManager, action, index, observationWindow);
            var file = CreateFile(StateActionPrefix);

            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }

        private StringBuilder StateActionEntry(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             */
            var builder = new StringBuilder();
            var padded = new Grid<bool[]>(observationWindow, new bool[gridManager.GetNChoices()]);
            var center = Vector3Util.Scale(observationWindow, 0.5f);
            var posBoundDiff = observationWindow - center;
            var negBoundDiff = -1 * center;

            // Iterate sliding window.
            for (int yw = negBoundDiff.y; yw < posBoundDiff.y; yw++)
            {
                for (int xw = negBoundDiff.x; xw < posBoundDiff.x; xw++)
                {
                    for (int zw = negBoundDiff.z; zw < posBoundDiff.z; zw++)
                    {
                        var windowOffset = new Vector3Int(xw, yw, zw);
                        var gridIndex = index + windowOffset;
                        if (!gridManager.WithinBounds(gridIndex)) continue;
                        
                        var sliderWindowIndex = center + windowOffset;
                        padded.Set(sliderWindowIndex, gridManager.ChoiceBooleans.Get(gridIndex));
                    }
                }
            }
            
            // var intFlattened = flattened.SelectMany(x => x.Select(x0 => x0 ? 1 : 0).ToArray()).ToArray();
            var paddedFlattened = Grid<bool[]>.Flatten(padded).SelectMany(x => x.Select(x0 => x0 ? 1 : 0).ToArray()).ToArray();
            
            foreach (var t in paddedFlattened)
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
            _stateActionQueuePath = file;
            File.WriteAllText(file, builder.ToString());
        }

        public void WriteStateActionQueue(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow,
            CollapsePriorityQueue collapsePriorityQueue)
        {
            /*
             * Structure:
             * - state: whd * nTiles cells.
             * - action: nTiles cells.
             * - collapse queue: remainder of cells.
             */
            var builder = StateActionEntry(gridManager, action, index, observationWindow);
            var file = CreateFile(StateActionQueuePrefix);
            foreach (var collapse in collapsePriorityQueue.List)
            {
                builder.Append(collapse + ",");
            }

            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }
        
        public string FilePath(string fileName)
        {
            return Path.Join(_directoryPath, fileName);
        }

        public void Reset()
        {
            File.Delete(_configPath);
            File.Delete(_stateActionPath);
            File.Delete(_stateActionQueuePath);
            // InitTimeStamp();
            InitConfig();
            InitStateAction();
            InitStateActionQueue();
        }
    }
}