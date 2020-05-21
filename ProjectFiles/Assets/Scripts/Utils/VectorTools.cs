using System.Linq;
using UnityEngine;

namespace Utils {
    public class VectorTools {
        public static Vector3 GetMinVector(params Vector3[] vectors) {
            var minX = vectors.Min(vector3 => vector3.x);
            var minY = vectors.Min(vector3 => vector3.y);
            var minZ = vectors.Min(vector3 => vector3.z);
            return new Vector3 {
                x = minX,
                y = minY,
                z = minZ,
            };
        }
    
        public static Vector3 GetMaxVector(params Vector3[] vectors) {
            var maxX = vectors.Max(vector3 => vector3.x);
            var maxY = vectors.Max(vector3 => vector3.y);
            var maxZ = vectors.Max(vector3 => vector3.z);
            return new Vector3 {
                x = maxX,
                y = maxY,
                z = maxZ,
            };
        }
    }
}