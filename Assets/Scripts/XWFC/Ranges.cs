using System;

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
        
        public Range3D() {}

        public int GetZLength()
        {
            return ZRange.End - ZRange.Start;
        }
    }
}