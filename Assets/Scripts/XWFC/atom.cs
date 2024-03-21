using UnityEngine;

namespace XWFC
{
     public class Atom
    {
        // public int Id { get; }
        public string Path { get; }

        public Atom(string path = "parts/1x1x1.glb")
        {
            // Id = id;
            Path = path;
        }
    }

    public record AtomInfo
    {
        public int TerminalId;
        public Vector3 RelativeCoord;
        public int Orientation;

        public AtomInfo(int terminalId, Vector3 relativeCoord, int orientation)
        {
            TerminalId = terminalId;
            RelativeCoord = relativeCoord;
            Orientation = orientation;
        }
    }
}
