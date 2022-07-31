using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace P90brush
{
    public class GameUtils
    {
        public static float ClampAngle(float angle, float min, float max) {
            angle %= 360;
            if ((angle >= -360F) && (angle <= 360F)) {
                if (angle < -360F) {
                    angle += 360F;
                }
                if (angle > 360F) {
                    angle -= 360F;
                }
            }
            return Mathf.Clamp(angle, min, max);
        }

        public static Vector3 VectorMa(Vector3 start, float scale, Vector3 direction) {
            var dest = new Vector3() {
                x = start.x + direction.x * scale,
                y = start.y + direction.y * scale,
                z = start.z + direction.z * scale
            };
            return dest;
        }
    }
}
