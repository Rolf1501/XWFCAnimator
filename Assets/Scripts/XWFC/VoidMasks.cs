namespace XWFC
{
    public class VoidMasks
    {
        public static (int[,] negative, int[,] positive) CalcVoidMask(bool[,,] mask, int axis)
        {
            // Void mask is created by looking at/projecting a mask onto a plane. For this we need an up (0,1,0) and looking direction.

            var max = mask.GetLength(axis);

            var (up, looking) = CalcUpAndLookingDirections(axis);

            var (voidMaskMaxY, voidMaskMaxX) = (mask.GetLength(up), mask.GetLength(looking));

            var posMask = new int[voidMaskMaxY, voidMaskMaxX];
            var posMaskDone = new bool[voidMaskMaxY, voidMaskMaxX];

            var negMask = new int[voidMaskMaxY, voidMaskMaxX];
            var negMaskDone = new bool[voidMaskMaxY, voidMaskMaxX];

            for (int y = 0; y < mask.GetLength(0); y++)
            {
                for (int x = 0; x < mask.GetLength(1); x++)
                {
                    for (int z = 0; z < mask.GetLength(2); z++)
                    {
                        // Get the index of the last layer in the axis' direction.
                        int[] indexArray = { y, x, z };
                        int[] indexArrayComplement = { y, x, z };
                        indexArrayComplement[axis] = max - indexArray[axis] - 1;

                        var negative = mask[indexArray[0], indexArray[1], indexArray[2]];
                        var positive = mask[indexArrayComplement[0], indexArrayComplement[1], indexArrayComplement[2]];

                        // If we found an occupied cell, stop counting empty cells.
                        UpdateMask(negative, indexArray[up], indexArray[looking], negMask, negMaskDone);
                        UpdateMask(positive, indexArray[up], indexArray[looking], posMask, posMaskDone);
                    }
                }
            }

            return (negMask, posMask);
        }
        
        private static void UpdateMask(bool entry, int iY, int iX, int[,] voidMask, bool[,] voidMaskDone)
        {
            if (entry || voidMaskDone[iY, iX])
            {
                voidMaskDone[iY, iX] = true;
            }
            else
            {
                voidMask[iY, iX]++;
            }
        }
        
        public static (int up, int looking) CalcUpAndLookingDirections(int axis, bool useYxz = true)
        {
            /*
             * Calculates the up and looking direction. Y is default up. Looking direction is always orthogonal to axis.
             * In case axis is Y, up is X.
             */
            // Y is default up direction. Assumes YXZ axes by default. XYZ otherwise.
            var (y, x, z) = (0, 1, 2);
            if (!useYxz)
                (y, x) = (1, 0);

            return axis != y ? (y, axis == z ? x : z) : (x, z);
        }
    }
}