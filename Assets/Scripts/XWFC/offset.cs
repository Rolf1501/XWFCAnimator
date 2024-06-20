using System;
using UnityEngine;

namespace XWFC
{
    public static class OffsetFactory
    {
        public static Vector3Int[] GetOffsets(int dimensions = 3)
        {
            if (dimensions is > 3 or < 1)
                throw new DimensionsNotSupportedException(
                    $"Dimension should not be less than 1 and should not exceed 3. Got: {dimensions}");

            var offsets = new Vector3Int[dimensions * 2];

            for (int i = 0; i < dimensions; i++)
            {
                var offsetPlus = new Vector3Int(0, 0, 0);
                var offsetMinus = new Vector3Int(0, 0, 0);
                offsetPlus[i] = 1;
                offsetMinus[i] = -1;
                offsets[i * 2] = offsetPlus;
                offsets[i * 2 + 1] = offsetMinus;
            }

            return offsets;
        }
    }

    public class DimensionsNotSupportedException : Exception
    {
        public DimensionsNotSupportedException(string message) : base(message)
        {
        }
    }
}