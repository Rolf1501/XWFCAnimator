using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Numpy;
using UnityEngine;

namespace XWFC
{
    public class Tile
    {
        public Vector3 Extent { get; }
        public Color Color { get; private set; }
        public bool[,,] Mask { get; }
        
        public string[,,] AtomValues { get; }
        public int[] DistinctOrientations { get; private set; }
        public string Description { get; }
        public Dictionary<Vector3, Atom> AtomIndexToIdMapping { get; } = new();
        public Dictionary<int, bool[,,]> OrientedMask { get; } = new();
        private Dictionary<int, bool[,,,]> OrientedAtomMask { get; } = new();
        public Dictionary<int, Vector3[]> OrientedIndices { get; } = new();
        public Dictionary<Vector3, Dictionary<int, int[,]>> OrientedVoidMasks { get; } = new();
        [CanBeNull] public Dictionary<Vector3Int, HashSet<BorderOutline.Edge>> AtomEdges;
        public readonly bool IsEmptyTile = false;

        public int NAtoms { get; private set; }

        #nullable enable
        public Tile(Vector3 extent, Color color, bool[,,]? mask = null, int[]? distinctOrientations = null,
            string description = "", bool computeAtomEdges=true)
        {
            Extent = extent;
            var (x, y, z) = Vector3Util.CastInt(extent);
            Mask = mask ?? Util.Populate3D(x, y, z, true);
            
            Description = description;
            Init(computeAtomEdges, distinctOrientations, color);
        }

        public Tile(string[,,] atomValues, Color color, int[]? distinctOrientations = null, bool computeAtomEdges = true)
        {
            /*
             * Constructor for creating a tile from a string array representation of a tile.
             */
            AtomValues = atomValues;
            var (x, y, z) = (atomValues.GetLength(1), atomValues.GetLength(0), atomValues.GetLength(2));
            Extent = new Vector3(x,y,z);
            Mask = new bool[y,x,z];
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        if (!atomValues[i, j, k].Equals(""))
                        {
                            Mask[y, x, z] = true;
                        }
                    }
                }
            }
            
            Init(computeAtomEdges, distinctOrientations, color);
        }

        public Tile(string uniformAtomValue, Vector3 extent, Color color, bool[,,]? mask = null, int[]? distinctOrientations = null,
            bool computeAtomEdges = true, string description="", bool isEmptyTile=false)
        {
            Description = description;
            IsEmptyTile = isEmptyTile;
            if (mask == null)
            {
                var (x, y, z) = Vector3Util.CastInt(extent);
                AtomValues = Util.Populate3D(x, y, z, uniformAtomValue);
                Mask = Util.Populate3D(x, y, z, true);
                Extent = new Vector3(x, y, z);
            }
            else
            {
                var (x, y, z) = (Mask.GetLength(1), Mask.GetLength(0), Mask.GetLength(2));
                Extent = new Vector3(x, y, z);
                AtomValues = new string[y, x, z];
                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        for (int k = 0; k < z; k++)
                        {
                            AtomValues[y, x, z] = Mask[y, x, z] ? uniformAtomValue : "";
                        }
                    }
                }
            }
            Init(computeAtomEdges, distinctOrientations, color);
        }
        
        private void Init(bool computeAtomEdges, int[]? distinctOrientations, Color color)
        {
            DistinctOrientations = distinctOrientations ?? new int[] { 0 };
            Color = color;
            CalcOrientedMasks();
            CalcVoidMasks();
            if (computeAtomEdges)
            {
                AtomEdges = new BorderOutline().GetEdgesPerAtom(Mask);
            }
        }

        public string GetAtomValue(Vector3 coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return WithinBounds(coord) ? AtomValues[y, x, z] : "";
        }

        public Dictionary<string, string> ToJson()
        {
            return new Dictionary<string, string>
            {
                { "extent", Vector3Util.Vector3ToString(Extent) },
                { "color", ColorUtil.ColorToString(Color) },
                { "mask", MaskToString() },
                { "distinct_orientations",  OrientationsToString() },
                { "description", Description },
                { "edges", AtomEdges is { Count: > 0 } ? "1" : "0" }
            };
        }
        
        public static Tile FromJson(Dictionary<string, string> jsn)
        {
            var extent = Vector3Util.Vector3FromString(jsn["extent"]);
            return new Tile(
                extent,
                ColorUtil.ColorFromString(jsn["color"]),
                MaskFromString(jsn["mask"], extent),
                OrientationsFromString(jsn["distinct_orientations"]),
                jsn["description"],
                computeAtomEdges: jsn["edges"].Equals("1")
            );
            
        }

        private string MaskToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            for (int i = 0; i < Mask.GetLength(0); i++)
            {
                for (int j = 0; j < Mask.GetLength(1); j++)
                {
                    for (int k = 0; k < Mask.GetLength(2); k++)
                    {
                        char item = Mask[i, j, k] ? '1' : '0';
                        builder.Append(item + ",");
                    }
                }
            }

            var lastIndex = builder.Length - 1;
            if (builder[lastIndex].Equals(',')) builder.Remove(lastIndex, 1);
            builder.Append("]");
            return builder.ToString();
        }

        private static bool[,,] MaskFromString(string s, Vector3 extent)
        {
            var trimmed = s.Trim(new char[] { '[', ']' });
            var split = trimmed.Split(",");
            var mask = new bool[(int)extent.y, (int)extent.x, (int)extent.z];
            var index = 0;
            for (int i = 0; i < extent.y; i++)
            {
                for (int j = 0; j < extent.x; j++)
                {
                    for (int k = 0; k < extent.z; k++)
                    {
                        mask[i, j, k] = split[index].Equals("1");
                        index++;
                    }
                }
            }

            return mask;
        }

        private static int[] OrientationsFromString(string s)
        {
            var trimmed = s.Trim(new char[] { '[', ']' });
            var split = trimmed.Split(",");
            return split.Select(item => int.Parse(item)).ToArray();
        }
        
        private string OrientationsToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            foreach (int i in DistinctOrientations)
            {
                builder.Append(i + ",");
            }
            var lastIndex = builder.Length - 1;
            if (builder[lastIndex].Equals(',')) builder.Remove(lastIndex, 1);
            builder.Append("]");
            return builder.ToString();
        }

        private static IEnumerable<(int, int, int)> NonEmptyIndices3D<T>(T[,,] grid)
        {
            /*
             * Finds the coordinates of all non-empty cells in the given grid.
             */
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        var v = grid[y, x, z];
                        if (v != null && !v.Equals(default(T)))
                        {
                            yield return (y, x, z);
                        }
                    }
                }
            }
        }

        private static Vector3[] CalcAtomIndices(bool[,,] mask)
        {
            var nonEmptyCells = NonEmptyIndices3D(mask);

            return nonEmptyCells.Select(cell => new Vector3(cell.Item2, cell.Item1, cell.Item3)).ToArray();
        }

        private static T[,] NumpyTo2DArray<T>(NDarray npMatrix, int typeSize)
        {
            var (h, w) = (npMatrix.shape[0], npMatrix.shape[1]);
            T[] data = npMatrix.GetData<T>();
            var output = new T[h, w];
            Buffer.BlockCopy(data, 0, output, 0, data.Length * typeSize);
            return output;
        }

        private void CalcOrientedMasks()
        {
            // TODO: Rotate matrix without numpy.
            foreach (int d in DistinctOrientations)
            {
                var arr = np.array(Mask);
                NDarray rot = arr.rot90(d, axes: new int[] { 1, 2 });

                OrientedMask[d] = Mask;
                Vector3[] indices = CalcAtomIndices(OrientedMask[d]);
                OrientedIndices[d] = indices;
                int nIndices = indices.Length;
                NAtoms += nIndices;

                var s = new int[] { Mask.GetLength(0), Mask.GetLength(1), Mask.GetLength(2) };
                OrientedAtomMask[d] = new bool[s[0], s[1], s[2], nIndices];

                for (int i = 0; i < nIndices; i++)
                {
                    var (cx, cy, cz) = Vector3Util.CastInt(indices[i]);
                    OrientedAtomMask[d][cy, cx, cz, i] = true;

                    // Make sure to add a new entry for the newly found atom.
                    AtomIndexToIdMapping[indices[i]] = new Atom();
                }
            }
        }

        private void CalcVoidMasks()
        {
            /*
             * Precomputes the void masks for each direction for a tile.
             */

            // Maps the directions to their axes.
            var axes = new Dictionary<int, (Vector3, Vector3)>
            {
                { 0, (new Vector3(0, -1, 0), new Vector3(0, 1, 0)) }, // BOTTOM - TOP
                { 1, (new Vector3(-1, 0, 0), new Vector3(1, 0, 0)) }, // WEST - EAST
                { 2, (new Vector3(0, 0, -1), new Vector3(0, 0, 1)) } // SOUTH - NORTH
            };

            // Initialize the void mask placeholders.
            foreach (var (a0, a1) in axes.Values)
            {
                OrientedVoidMasks[a0] = new Dictionary<int, int[,]>();
                OrientedVoidMasks[a1] = new Dictionary<int, int[,]>();
            }

            // For each axis, calculate the void masks in the positive and negative directions along said axis.
            foreach (var d in DistinctOrientations)
            {
                foreach (var (axis, (cardinalAlong, cardinalAgainst)) in axes)
                {
                    var (negative, positive) = VoidMasks.CalcVoidMask(Mask, axis);
                    OrientedVoidMasks[cardinalAlong][d] = NumpyTo2DArray<int>(negative, sizeof(int));
                    OrientedVoidMasks[cardinalAgainst][d] = NumpyTo2DArray<int>(positive, sizeof(int));
                }
            }
        }

        public int GetRotationOrientation(int rotation)
        {
            /*
             * Determines which terminal orientation to use given the rotation.
             * First check if the rotation is present. If not, check if the direction corresponding to the rotation is in the orientations.
             * Otherwise, the terminal is assumed to be unaffected by rotation (e.g. cubes).
             */
            if (Array.IndexOf(DistinctOrientations, rotation) != -1)
            {
                return rotation;
            }

            if (Array.IndexOf(DistinctOrientations, (rotation + 2) % 3) != -1)
            {
                return (rotation + 2) % 3;
            }

            /*
             * TODO: get distinct orientations here for rotation.
             */
            return DistinctOrientations[0];
        }

        public bool ContainsAtom(int orientation, Vector3 coord)
        {
            var (x, y, z) = Vector3Util.CastInt(coord);
            return ContainsAtom(orientation, x, y, z);
        }

        public bool IsEmpty(Vector3 coord)
        {
            // TODO: if orientation support, pass orientation.
            var (x, y, z) = Vector3Util.CastInt(coord);
            return Mask[y, x, z];
        }

        public bool ContainsAtom(int orientation, int x, int y, int z)
        {
            try
            {
                return OrientedMask[orientation][y, x, z];
            }
            catch
            {
                return false;
            }
        }

        public bool WithinBounds(Vector3 coord)
        {
            return coord.x < Mask.GetLength(1) && coord.y < Mask.GetLength(0) && coord.z < Mask.GetLength(2) &&
                   coord is { x: >= 0, y: >= 0, z: >= 0 };
        }
    }

    
}