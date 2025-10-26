using UnityEngine;

namespace XWFC
{
    public interface ICollapsePriorityQueue
    {
        void Clear();
        bool Contains(Vector3Int coord);
        CollapsePriorityQueue Copy();
        Collapse DeleteHead();
        void Insert(Collapse collapse);
        void Insert(Vector3Int coord, float entropy);
        bool IsDone();
        Collapse PeekHead();
    }
}