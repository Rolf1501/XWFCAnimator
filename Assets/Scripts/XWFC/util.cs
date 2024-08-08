using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace XWFC
{
    public class Vector3Util
    {
        public static Vector3 Negate(Vector3 vec)
        {
            return vec * -1;
        }

        public static Vector3Int Negate(Vector3Int vec)
        {
            return vec * -1;
        }

        public static (int, int, int) CastInt(Vector3 vec)
        {
            return ((int)vec.x, (int)vec.y, (int)vec.z);
        }

        public static Vector3Int VectorToVectorInt(Vector3 vector)
        {
            var (x,y,z) = CastInt(vector);
            return new Vector3Int(x, y, z);
        }
        
        public static Vector3 Mult(Vector3 v0, Vector3 v1) => new(v0.x * v1.x, v0.y * v1.y, v0.z * v1.z);

        public static Vector3 Div(Vector3 v0, Vector3 v1)
        {
            if (v0.x != 0 && v0.y != 0 && v0.z != 0 && v1.x != 0 && v1.y != 0 && v1.z != 0)
            {
                return new Vector3(v0.x / v1.x, v0.y / v1.y, v0.z / v1.z);
            }

            return v0;
        }

        public static Vector3 Scale(Vector3 v, float scalar)
        {
            return new Vector3(v.x * scalar, v.y * scalar, v.z * scalar);
        }

        public static string Vector3ToString(Vector3 v)
        {
            return $"<{v.x},{v.y},{v.z}>";
        }

        public static Vector3 Vector3FromString(string v)
        {
            var trimmed = v.Trim(new char[] { '<', '>' });
            var split = trimmed.Split(",");
            return split.Length switch
            {
                3 => new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2])),
                _ => new Vector3()
            };
        }

        public static int GetByAxis(Vector3Int vector, int axis, bool useYXZ=true)
        {
            return axis switch
            {
                0 => useYXZ ? vector.y : vector.x,
                1 => useYXZ ? vector.x : vector.y,
                _ => vector.z
            };
        }

        public static Vector3Int SetByAxis(Vector3Int vector, int axis, int value, bool useYXZ = true)
        {
            switch (axis)
            {
                case 0:
                    if (useYXZ) vector.y = value; else vector.x = value;
                    break;
                case 1: 
                    if (useYXZ) vector.x = value; else vector.y = value;
                    break;
                default: 
                    vector.z = value;
                    break;
            }

            return vector;
        }

        public static Vector3Int PairWiseMin(Vector3Int v0, Vector3Int v1)
        {
            return new Vector3Int(Math.Min(v0.x, v1.x), Math.Min(v0.y, v1.y), Math.Min(v0.z, v1.z));
        }
        
        public static Vector3Int PairWiseMax(Vector3Int v0, Vector3Int v1)
        {
            return new Vector3Int(Math.Max(v0.x, v1.x), Math.Max(v0.y, v1.y), Math.Max(v0.z, v1.z));
        }

        public static bool Geq(Vector3Int v0, Vector3Int v1)
        {
            return v0.x >= v1.x && v0.y >= v1.y && v0.z >= v1.z;
        }
        
        public static bool Lt(Vector3Int v0, Vector3Int v1)
        {
            return v0.x < v1.x && v0.y < v1.y && v0.z < v1.z;
        }
    }
    
    public static class Util
    {
        public static T[,,] Populate3D<T>(int width, int height, int depth, T value)
        {
            T[,,] output = new T[height, width, depth];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        output[i, j, k] = value;
                    }
                }
            }

            return output;
        }
    }

    public static class ColorUtil
    {
        public static string ColorToString(Color c)
        {
            return $"<{c.r},{c.g},{c.b},{c.a}>";
        }

        public static Color ColorFromString(string c)
        {
            var trimmed = c.Trim(new char[] { '<', '>' });
            var split = trimmed.Split(",");
            return split.Length switch
            {
                4 => new Color(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]),
                    float.Parse(split[3])),
                3 => new Color(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]), 1.0f),
                _ => new Color()
            };
        }
    }

    public static class Vectorizor
    {
        public static List<Vector<byte>> VectorizeBool(bool[] array)
        {
            var count = Vector<byte>.Count;
            var nVectors = (int)Math.Ceiling(array.Length * 1.0 / count);
            var output = new List<Vector<byte>>();

            for (int i = 0; i < nVectors; i++)
            {
                var temp = new byte[count];
                for (int j = 0; j < count; j++)
                {
                    temp[j] = (byte)(array[i * count + j] ? 1 : 0);
                }

                output.Add(new Vector<byte>(temp));
            }

            return output;
        }

        public static List<Vector<byte>> Or(List<Vector<byte>> left, List<Vector<byte>> right)
        {
            var output = new List<Vector<byte>>();
            for (var i = 0; i < left.Count; i++)
            {
                output.Add(Vector.BitwiseOr(left[i], right[i]));
            }

            return output;
        }

        public static byte GetAtIndex(int index, List<Vector<byte>> list)
        {
            var count = Vector<byte>.Count;
            var listIndex = (int)(index / (1.0d * count));
            var vectorIndex = index - listIndex * count;
            return list[listIndex][vectorIndex];
        }
    }
}