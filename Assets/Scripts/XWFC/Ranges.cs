using System;
using Unity.VisualScripting;
using UnityEngine;

namespace XWFC
{
    public class Range
    {
        public int Start;
        public int End;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
            if (Start > End) throw new Exception("Range start must not be larger than end.");
        }

        public int GetLength()
        {
            return End - Start;
        }

        public Range()
        {
                
        }
        
        public static bool Intersect1D(Range r0, Range r1)
        {
            /*
             * Two regions intersect if there exists a number N in both ranges.
             * This means that r0_start <= N <= r0_end and r1_start <= N <= r1_end.
             * Here follows that r0_start <= r1_end and r1_start <= r0_end.
             * Note the <= sign. If two ranges touch, they should be considered intersecting.
             */
            return r0.Start <= r1.End && r1.Start <= r0.End;
        }

        public bool IsZero()
        {
            return Start == End;
        }

        public bool Equals(Range range)
        {
            return Start == range.Start && End == range.End;
        }
    }
    
    public record Range2D
    {
        public Range XRange { get; }
        public Range YRange { get; }

        public Range2D(int xStart, int xEnd, int yStart, int yEnd)
        {
            XRange = new Range(xStart, xEnd);
            YRange = new Range(yStart, yEnd);
        }

        public Range2D(Range xRange, Range yRange)
        {
            XRange = xRange;
            YRange = yRange;
        }
        
        public Range2D() {}

        public int GetYLength()
        {
            return YRange.GetLength();
        }
        
        public int GetXLength()
        {
            return XRange.GetLength();
        }
    }

    public record Range3D : Range2D
    {
        public Range ZRange;
        public Range3D(int xStart, int xEnd, int yStart, int yEnd, int zStart, int zEnd) : base(xStart, xEnd, yStart, yEnd)
        {
            ZRange = new Range(zStart, zEnd);
        }

        public Range3D(Range xRange, Range yRange, Range zRange) : base (xRange, yRange)
        {
            ZRange = zRange;
        }

        public Range3D(Vector3Int start, Vector3Int end) : base (new Range(start.x, end.x), new Range(start.y, end.y))
        {
            ZRange = new Range(start.z, end.z);
        }
        
        public Range3D() {}

        public int GetZLength()
        {
            return ZRange.End - ZRange.Start;
        }
        
        private static (bool intersects, Range) RangeIntersection(Range rangeSource, Range rangeOther)
        {
            // If two ranges intersect, the region of intersection is given by the maximum of the minima and the minimum of the maxima.
            return Range.Intersect1D(rangeSource, rangeOther)
                ? (true, new Range(Math.Max(rangeSource.Start, rangeOther.Start), Math.Min(rangeSource.End, rangeOther.End)))
                : (false, new Range());
        }
        
        public static (bool intersects, Range3D ranges) Intersect3D(Range3D r0, Range3D r1)
        {
            /*
             * Intersection 3D makes use of Components being represented as AABBs.
             * Two AABBs are intersecting iff all their axis projections overlap.
             */
            var noIntersect = (false, new Range3D());

            var (xIntersects, xRange) = RangeIntersection(r0.XRange, r1.XRange);
            var (yIntersects, yRange) = RangeIntersection(r0.YRange, r1.YRange);
            var (zIntersects, zRange) = RangeIntersection(r0.ZRange, r1.ZRange);
            
            // An intersection can only be present if there is an intersection in all dimensions. 
            if (!(xIntersects && yIntersects && zIntersects)) return noIntersect;
            
            // Corners should not result in an intersection.
            // For corners, at least 2 dimensions are just touching.
            var touchingDims = 0;
            if (xRange.Start == xRange.End) touchingDims++;
            if (yRange.Start == yRange.End) touchingDims++;
            if (zRange.Start == zRange.End) touchingDims++;

            return touchingDims > 1 ? noIntersect : (true, new Range3D(xRange, yRange, zRange));
        }

        public (bool x, bool y, bool z) IsZero()
        {
            return (XRange.IsZero(), YRange.IsZero(), ZRange.IsZero());
        }

        public (bool x, bool y, bool z) RangesEquals(Range3D range)
        {
            return (XRange.Equals(range.XRange), YRange.Equals(range.YRange), ZRange.Equals(range.ZRange));
        }

        public Vector3Int Origin()
        {
            return new Vector3Int(XRange.Start, YRange.Start, ZRange.Start);
        }
        public Vector3Int Extent()
        {
            return new Vector3Int(XRange.End, YRange.End, ZRange.End);
        }
    }
}