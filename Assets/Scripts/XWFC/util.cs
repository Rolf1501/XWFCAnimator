using Unity.VisualScripting;
using UnityEngine;

namespace XWFC
{
    public class Vector3Util
    {
        public static Vector3 Negate(Vector3 vec)
        {
            return vec * -1;
        }

        public static (int, int, int) CastInt(Vector3 vec)
        {
            return ((int)vec.x, (int)vec.y, (int)vec.z);
        }
        
        public static Vector3 Mult(Vector3 v0, Vector3 v1) => new(v0.x * v1.x, v0.y * v1.y, v0.z * v1.z);

        public static Vector3Int Scale(Vector3Int v, float scalar) => new((int)(v.x * scalar), (int)(v.y * scalar), (int)(v.z * scalar));

        public static bool CompLT(Vector3Int v0, Vector3Int v1)
        {
            return v0.x < v1.x || v0.y < v1.y || v0.z < v1.z;
        }

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
}