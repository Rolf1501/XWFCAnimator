using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XWFC
{
    public class SavePointManager
    {
        private Stack<SavePoint> _savePoints = new();
        private const int MaxSavePointAttempts = 100;
        private const int SavePointIntervals = 300; // < 1 ? in percentages : in cells.
        private int _savePointAttempts;

        public void Save(float progress, GridManager gridMan, CollapsePriorityQueue collapseQueue, int counter)
        {
            // If absolute number of cells or percentage matches, save.
            if (AbsoluteIntervalMatch(counter) || RelativeIntervalMatch(progress))
                _savePoints.Push(new SavePoint(gridMan.Deepcopy(), collapseQueue.Copy(), counter));
        }

        private bool AbsoluteIntervalMatch(int counter)
        {
            return SavePointIntervals >= 1 && counter % SavePointIntervals == 0;
        }

        private bool RelativeIntervalMatch(float progress)
        {
            return SavePointIntervals < 1 && progress % SavePointIntervals == 0;
        }

        public SavePoint Restore()
        {
            var savePoint = _savePoints.Peek();
            if (_savePointAttempts > MaxSavePointAttempts && _savePoints.Count > 0)
            {
                savePoint = _savePoints.Pop();
                _savePointAttempts = 0;
            }

            _savePointAttempts++;

            return savePoint;
        }
    }

    public record SavePoint
    {
        public GridManager GridManager;
        public CollapsePriorityQueue CollapseQueue;
        public int Counter;

        public SavePoint(GridManager gridManager, CollapsePriorityQueue collapseQueue, int counter)
        {
            GridManager = gridManager;
            CollapseQueue = collapseQueue;
            Counter = counter;
        }
        
    }
}