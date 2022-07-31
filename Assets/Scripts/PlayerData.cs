using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace P90brush
{
    public class PlayerData
    {
        public Vector3 Origin;
        public Vector3 ViewAngles;//Todo: Change to Vector2
        public Vector3 Velocity;
        public float SurfaceFriction = 1f;
        public float GravityFactor = 1f;
        public float WalkFactor = 1f;
        public GameObject GroundObject;
        public Vector3 HookedPosition = Vector3.zero;



        public bool IsGrounded() {
            return GroundObject != null;
        }

        public bool IsHooked() {
            return HookedPosition != Vector3.zero;
        }
    }
}
