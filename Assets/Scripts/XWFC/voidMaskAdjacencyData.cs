using System.Collections.Generic;
using UnityEngine;

namespace XWFC
{
    public class VoidMaskAdjacencyData
    {
        public NonUniformTile NonUniformTile { get; }
        public int TerminalId { get; }
        private Vector3 Offset { get; }
        public int[] Rotations { get; }
        public int OffsetDirectionIndex { get; }
        private int? Volume { get; set; }
        public Dictionary<int, int[,]> VoidMasks { get; private set; }

        public VoidMaskAdjacencyData(NonUniformTile tile, int terminalId, Vector3 offset, int[] rotations,
            int offsetDirectionIndex)
        {
            NonUniformTile = tile;
            TerminalId = terminalId;
            Offset = offset;
            Rotations = rotations;
            OffsetDirectionIndex = offsetDirectionIndex;
            VoidMasks = NonUniformTile.OrientedVoidMasks[Offset];
        }

        public (int, int, int) MaskShape(int orientation)
        {
            var m = NonUniformTile.OrientedMask[orientation];
            return (m.GetLength(0), m.GetLength(1), m.GetLength(2));
        }

        public int GetVolume()
        {
            Volume ??= NonUniformTile.OrientedMask[0].Length;
            return Volume.Value;
        }
    }
}