using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Unity.VisualScripting;

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
        private const string LevelPrefix = "level";
        private const string LevelConcatPrefix = "level-concat";
        private const string FailureSuffix = "-failure";
        private string _configPath;
        private string _stateActionPath;
        private string _stateActionQueuePath;
        [CanBeNull] private Grid<string> _level;

        public TrainingDataFormatter(string directory, string fileName)
        {
            _directoryPath = directory;
            _fileName = fileName;
            // Time stamp used to uniquely identify data files.
            InitTimeStamp();
            InitConfig();
            // InitStateAction();
            // InitStateActionQueue();
            InitLevel();
            InitLevelConcat();
        }

        private void InitTimeStamp()
        {
            _timeStamp = FormatTimeStamp(DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }

        private static string FormatTimeStamp(string timeStamp)
        {
            return timeStamp.Replace("-","").Replace("/", "-").Replace("\\","-").Replace(" ", "-").Replace(":", "");
        }

        private string GetFilePath(string filePrefix, bool failure=false)
        {
            var name = filePrefix + (failure ? FailureSuffix : "");
            return FilePath($"{name}-{_timeStamp}-{_fileName}");
        }

        public string CreateFile(string filePrefix)
        {
            var path = GetFilePath(filePrefix);// FilePath($"{filePrefix}-{_timeStamp}-{_fileName}");
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
            string header = "timestamp,gridSizeX,gridSizeY,gridSizeZ,randomSeed,tiles,adjacencyMatrix,atomMapping(atomId-atomCoord-tileId)";
            builder.Append(header + "\n");
            _configPath = file;
            File.WriteAllText(file, builder.ToString());
        }
        
        public void WriteConfig(Vector3 gridSize, string randomSeed, AdjacencyMatrix adjacencyMatrix, bool failure=false)
        {
            /*
             * Structure:
             * w,h,d,seed: 1 cell each.
             * adjacency matrix: nOffsets * nTiles * nTiles cells.
             */
            var builder = new StringBuilder();
            var fileName = ConfigPrefix;
            if (failure) fileName += FailureSuffix;
            var file = CreateFile(fileName);
            var adjMatrixString = adjacencyMatrix.AtomAdjacencyMatrixToFlattenedString();
            builder.Append($"{_timeStamp},{gridSize.x},{gridSize.y},{gridSize.z},{randomSeed},");
            foreach (var key in adjacencyMatrix.TileIds)
            {
                builder.Append($"{key}");
            }

            builder.Append(",");
            builder.Append(adjMatrixString);
            if (!adjMatrixString.EndsWith(",")) builder.Append(",");
            builder.Append(adjacencyMatrix.AtomMappingToString());
            builder.Append("\n");
            File.AppendAllText(file, builder.ToString());
        }

        // public void InitStateAction()
        // {
        //     var builder = new StringBuilder();
        //     var file = CreateFile(StateActionPrefix);
        //     builder.Append("state,action");
        //     builder.Append("\n");
        //     _stateActionPath = file;
        //     File.WriteAllText(file, builder.ToString());
        // }
        //
        // public void WriteStateAction(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow)
        // {
        //     /*
        //      * Structure:
        //      * - state: whd * nTiles cells.
        //      * - action: nTiles cells.
        //      */
        //     var builder = StateActionEntry(gridManager, action, index, observationWindow);
        //     var file = CreateFile(StateActionPrefix);
        //
        //     builder.Append("\n");
        //     File.AppendAllText(file, builder.ToString());
        // }
        //
        // private StringBuilder StateActionEntry(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow)
        // {
        //     /*
        //      * Structure:
        //      * - state: whd * nTiles cells.
        //      * - action: nTiles cells.
        //      */
        //     var builder = new StringBuilder();
        //     var padded = new Grid<bool[]>(observationWindow, new bool[gridManager.GetNChoices()]);
        //     var center = Vector3Util.Scale(observationWindow, 0.5f);
        //     var posBoundDiff = observationWindow - center;
        //     var negBoundDiff = -1 * center;
        //
        //     // Iterate sliding window.
        //     for (int yw = negBoundDiff.y; yw < posBoundDiff.y; yw++)
        //     {
        //         for (int xw = negBoundDiff.x; xw < posBoundDiff.x; xw++)
        //         {
        //             for (int zw = negBoundDiff.z; zw < posBoundDiff.z; zw++)
        //             {
        //                 var windowOffset = new Vector3Int(xw, yw, zw);
        //                 var gridIndex = index + windowOffset;
        //                 if (!gridManager.WithinBounds(gridIndex)) continue;
        //                 
        //                 var sliderWindowIndex = center + windowOffset;
        //                 padded.Set(sliderWindowIndex, gridManager.ChoiceBooleans.Get(gridIndex));
        //             }
        //         }
        //     }
        //     
        //     // var intFlattened = flattened.SelectMany(x => x.Select(x0 => x0 ? 1 : 0).ToArray()).ToArray();
        //     var paddedFlattened = Grid<bool[]>.Flatten(padded).SelectMany(x => x.Select(x0 => x0 ? 1 : 0).ToArray()).ToArray();
        //     
        //     foreach (var t in paddedFlattened)
        //     {
        //         builder.Append(t + ",");
        //     }
        //
        //     foreach (var value in action)
        //     {
        //         builder.Append(value + ",");
        //     }
        //
        //     return builder;
        // }
        //
        // private void InitStateActionQueue()
        // {
        //     var builder = new StringBuilder();
        //     var file = CreateFile(StateActionQueuePrefix);
        //     builder.Append("state,action,queue");
        //     builder.Append("\n");
        //     _stateActionQueuePath = file;
        //     File.WriteAllText(file, builder.ToString());
        // }
        //
        // public void WriteStateActionQueue(GridManager gridManager, int[] action, Vector3Int index, Vector3Int observationWindow,
        //     CollapsePriorityQueue collapsePriorityQueue)
        // {
        //     /*
        //      * Structure:
        //      * - state: whd * nTiles cells.
        //      * - action: nTiles cells.
        //      * - collapse queue: remainder of cells.
        //      */
        //     var builder = StateActionEntry(gridManager, action, index, observationWindow);
        //     var file = CreateFile(StateActionQueuePrefix);
        //     foreach (var collapse in collapsePriorityQueue.List)
        //     {
        //         builder.Append(collapse + ",");
        //     }
        //
        //     builder.Append("\n");
        //     File.AppendAllText(file, builder.ToString());
        // }
        private void InitLevel(int nCols=0)
        {
            var builder = new StringBuilder();
            var file = CreateFile(LevelPrefix);
            for (int i = 1; i <= nCols; i++)
            {
                builder.Append($"col{i},");
            }

            builder.Append("x,y,z,target\n");
            
            File.WriteAllText(file, builder.ToString());
        }

        public void WriteLevel(Vector3 coordinate, Grid<string> level)
        {
            var extent = level.GetExtent();
            if (_level == null || !_level.GetExtent().Equals(extent))
            {
                _level = level;
                InitLevel((int)Vector3Util.Product(extent));
            }
            
            var builder = new StringBuilder();
            var file = CreateFile(LevelPrefix);

            /*
             * State
             */
            for (int y = 0; y < extent.y; y++)
            {
                for (int x = 0; x < extent.x; x++)
                {
                    for (int z = 0; z < extent.z; z++)
                    {
                        var elem = level.Get(x, y, z);
                        builder.Append($"'{elem}',");
                    }
                }
            }
            
            /*
             * Position
             */
            builder.Append($"{coordinate.x},{coordinate.y},{coordinate.z},");
            
            /*
             * Action
             */
            builder.Append(level.Get(coordinate));
            
            File.AppendAllText(file, builder + "\n");
        }

        private void InitLevelConcat()
        {
            CreateFile(LevelConcatPrefix);
        }
        
        public string FilePath(string fileName)
        {
            return Path.Join(_directoryPath, fileName);
        }

        public void Reset()
        {
            File.Delete(_configPath);
            
            // File.Delete(_stateActionPath);
            // File.Delete(_stateActionQueuePath);
            // InitTimeStamp();
            InitConfig();
            // InitStateAction();
            // InitStateActionQueue();
            InitLevel();
            InitLevelConcat();
        }
        
        
        public void ResetLevel(bool delete=false)
        {
            if (delete)
            {
                Delete(LevelPrefix);
            }
            else
            {
                InitLevel();
            }
        }

        private void Delete(string filePrefix, bool failure=false)
        {
            var name = GetFilePath(filePrefix, failure);
            if (File.Exists(name))
            {
                File.Delete(name);
            }
        }

        public void ConcatLevel(bool failure=false)
        {
            // Fetch the correlating file containing the trace.
            var levelPath = LevelPrefix;
            var file = CreateFile(levelPath);
            
            var content = File.ReadAllLines(file);

            var concatPath = LevelConcatPrefix;
            if (failure) concatPath += FailureSuffix;
            var concatFile = CreateFile(concatPath);
            var builder = new StringBuilder();

            var i = 1; // skip header if header is already present.
            if (File.ReadAllLines(concatFile).Length == 0) i = 0;
            while (i < content.Length)
            {
                var line = content[i];
                builder.Append(line + ",\n");
                i++;
            }
            InitLevel();
            File.AppendAllText(concatFile, builder.ToString());
        }
    }
    
    
}