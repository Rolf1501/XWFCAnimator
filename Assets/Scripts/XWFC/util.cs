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

        public static float Product(Vector3 v)
        {
            return v.x * v.y * v.z;
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