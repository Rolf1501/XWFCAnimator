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
}