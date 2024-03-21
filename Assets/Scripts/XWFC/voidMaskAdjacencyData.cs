using System.Collections.Generic;
using UnityEngine;

namespace XWFC
{
    public class VoidMaskAdjacencyData
    {
        public Terminal Terminal { get; }
        public int TerminalId { get; }
        private Vector3 Offset { get; }
        public int[] Rotations { get; }
        public int OffsetDirectionIndex { get; }
        private int? Volume { get; set; }
        public Dictionary<int, int[,]> VoidMasks { get; private set; }

        public VoidMaskAdjacencyData(Terminal terminal, int terminalId, Vector3 offset, int[] rotations,
            int offsetDirectionIndex)
        {
            Terminal = terminal;
            TerminalId = terminalId;
            Offset = offset;
            Rotations = rotations;
            OffsetDirectionIndex = offsetDirectionIndex;
            VoidMasks = Terminal.OrientedVoidMasks[Offset];
        }

        public (int, int, int) MaskShape(int orientation)
        {
            var m = Terminal.OrientedMask[orientation];
            return (m.GetLength(0), m.GetLength(1), m.GetLength(2));
        }

        public int GetVolume()
        {
            Volume ??= Terminal.OrientedMask[0].Length;
            return Volume.Value;
        }
    }
}