﻿using UnityEngine;

namespace P90brush
{
    public class SurfPhysics
    {
        public static int GroundLayerMask = (1 << 0);

        private static Collider[] _colliders = new Collider[MaxCollisions];
        private static Vector3[] _planes = new Vector3[MaxClipPlanes];
        public const float HU2M = 52.4934383202f;
        private const int MaxCollisions = 128;
        private const int MaxClipPlanes = 5;
        private const int NumBumps = 1;

        public const float SurfSlope = 0.7f;

        public static void ResolveCollisions(Collider collider, ref Vector3 origin, ref Vector3 velocity) {
            // manual collision resolving
            int numOverlaps = 0;
            if (collider is CapsuleCollider) {
                var capc = collider as CapsuleCollider;

                Vector3 point1, point2;
                GetCapsulePoints(capc, origin, out point1, out point2);

                numOverlaps = Physics.OverlapCapsuleNonAlloc(point1, point2, capc.radius,
                    _colliders, GroundLayerMask, QueryTriggerInteraction.Ignore);
            } else if (collider is BoxCollider) {
                numOverlaps = Physics.OverlapBoxNonAlloc(origin, collider.bounds.extents, _colliders,
                    Quaternion.identity, GroundLayerMask, QueryTriggerInteraction.Ignore);
            }

            for (int i = 0; i < numOverlaps; i++) {
                if (Physics.ComputePenetration(collider, origin,
                    Quaternion.identity, _colliders[i], _colliders[i].transform.position,
                    _colliders[i].transform.rotation, out Vector3 direction, out float distance))
                {
                    direction.Normalize();
                    Vector3 penetrationVector = direction * distance;
                    Vector3 velocityProjected = Vector3.Project(velocity, -direction);
                    velocityProjected.y = 0; // don't touch y velocity, we need it to calculate fall damage elsewhere
                    origin += penetrationVector;
                    velocity -= velocityProjected;
                }
            }
        }

        /// <summary>
        /// Friction
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="stopSpeed"></param>
        /// <param name="friction"></param>
        /// <param name="deltaTime"></param>
        public static void Friction(ref Vector3 velocity, float stopSpeed, float friction, float deltaTime)
        {
            var speed = velocity.magnitude;

            if (speed < 0.0001905f)
            {
                return;
            }

            var drop = 0f;

            // apply ground friction
            var control = (speed < stopSpeed) ? stopSpeed : speed;
            drop += control * friction * deltaTime;

            // scale the velocity
            var newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;

            if (newspeed != speed)
            {
                newspeed /= speed;
                velocity *= newspeed;
            }
        }

        public static int Reflect(ref Vector3 velocity, Collider collider, Vector3 origin, float deltaTime) {
            float d;
            var newVelocity = Vector3.zero;
            var blocked = 0;           // Assume not blocked
            var numplanes = 0;           //  and not sliding along any planes
            var originalVelocity = velocity;  // Store original velocity
            var primalVelocity = velocity;

            var allFraction = 0f;
            var timeLeft = deltaTime;   // Total time for this movement operation.

            for (int bumpcount = 0; bumpcount < NumBumps; bumpcount++) {
                if (velocity.magnitude == 0f) {
                    break;
                }

                // Assume we can move all the way from the current origin to the
                //  end point.
                var end = GameUtils.VectorMa(origin, timeLeft, velocity);
                var trace = Tracer.TraceCollider(collider, origin, end, GroundLayerMask);

                allFraction += trace.Fraction;

                if (trace.Fraction > 0) {
                    // actually covered some distance
                    originalVelocity = velocity;
                    numplanes = 0;
                }

                // If we covered the entire distance, we are done
                //  and can return.
                if (trace.Fraction == 1) {
                    break;      // moved the entire distance
                }

                // If the plane we hit has a high z component in the normal, then
                //  it's probably a floor
                if (trace.PlaneNormal.y > SurfSlope) {
                    blocked |= 1;       // floor
                }
                // If the plane has a zero z component in the normal, then it's a 
                //  step or wall
                if (trace.PlaneNormal.y == 0) {
                    blocked |= 2;       // step / wall
                }

                // Reduce amount of m_flFrameTime left by total time left * fraction
                //  that we covered.
                timeLeft -= timeLeft * trace.Fraction;

                // Did we run out of planes to clip against?
                if (numplanes >= MaxClipPlanes) {
                    // this shouldn't really happen
                    //  Stop our movement if so.
                    velocity = Vector3.zero;
                    //Con_DPrintf("Too many planes 4\n");
                    break;
                }

                // Set up next clipping plane
                _planes[numplanes] = trace.PlaneNormal;
                numplanes++;

                // modify original_velocity so it parallels all of the clip planes
                //

                // reflect player velocity 
                // Only give this a try for first impact plane because you can get yourself stuck in an acute corner by jumping in place
                //  and pressing forward and nobody was really using this bounce/reflection feature anyway...
                if (numplanes == 1) {
                    for (int i = 0; i < numplanes; i++) {
                        if (_planes[i][1] > SurfSlope) {
                            // floor or slope
                            return blocked;
                            //ClipVelocity(originalVelocity, _planes[i], ref newVelocity, 1f);
                            //originalVelocity = newVelocity;
                        } else {
                            ClipVelocity(originalVelocity, _planes[i], ref newVelocity, 1f);
                        }
                    }
                    velocity = newVelocity;
                    originalVelocity = newVelocity;
                } else {
                    int i = 0;
                    for (i = 0; i < numplanes; i++) {
                        ClipVelocity(originalVelocity, _planes[i], ref velocity, 1);

                        int j = 0;

                        for (j = 0; j < numplanes; j++)
                            if (j != i) {
                                // Are we now moving against this plane?
                                if (Vector3.Dot(velocity, _planes[j]) < 0)
                                    break;
                            }
                        if (j == numplanes)  // Didn't have to clip, so we're ok
                            break;
                    }

                    // Did we go all the way through plane set
                    if (i != numplanes) {   // go along this plane
                        // pmove.velocity is set in clipping call, no need to set again.
                        ;
                    } else {   // go along the crease
                        if (numplanes != 2) {
                            velocity = Vector3.zero;
                            break;
                        }
                        var dir = Vector3.Cross(_planes[0], _planes[1]).normalized;
                        d = Vector3.Dot(dir, velocity);
                        velocity = dir * d;
                    }

                    //
                    // if original velocity is against the original velocity, stop dead
                    // to avoid tiny occilations in sloping corners
                    //
                    d = Vector3.Dot(velocity, primalVelocity);
                    if (d <= 0f) {
                        //Con_DPrintf("Back\n");
                        velocity = Vector3.zero;
                        break;
                    }
                }
            }

            if (allFraction == 0f) {
                velocity = Vector3.zero;
            }

            // Check if they slammed into a wall
            //float fSlamVol = 0.0f;

            //var primal2dLen = new Vector2(primal_velocity.x, primal_velocity.z).magnitude;
            //var vel2dLen = new Vector2(_moveData.Velocity.x, _moveData.Velocity.z).magnitude;
            //float fLateralStoppingAmount = primal2dLen - vel2dLen;
            //if (fLateralStoppingAmount > PLAYER_MAX_SAFE_FALL_SPEED * 2.0f)
            //{
            //    fSlamVol = 1.0f;
            //}
            //else if (fLateralStoppingAmount > PLAYER_MAX_SAFE_FALL_SPEED)
            //{
            //    fSlamVol = 0.85f;
            //}

            //PlayerRoughLandingEffects(fSlamVol);

            return blocked;
        }

        /// <summary>
        /// Air Accelerate
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="wishdir"></param>
        /// <param name="wishspeed"></param>
        /// <param name="accel"></param>
        /// <param name="airCap"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 AirAccelerate(Vector3 velocity, Vector3 wishdir, float wishspeed, float accel, float airCap, float deltaTime)
        {
            var wishspd = wishspeed;

            // Cap speed
            wishspd = Mathf.Min(wishspd, airCap);

            // Determine veer amount
            var currentspeed = Vector3.Dot(velocity, wishdir);

            // See how much to add
            var addspeed = wishspd - currentspeed;

            // If not adding any, done.
            if (addspeed <= 0)
            {
                return Vector3.zero;
            }

            // Determine acceleration speed after acceleration
            var accelspeed = accel * wishspeed * deltaTime;

            // Cap it
            accelspeed = Mathf.Min(accelspeed, addspeed);

            var result = Vector3.zero;

            // Adjust pmove vel.
            for (int i = 0; i < 3; i++)
            {
                result[i] += accelspeed * wishdir[i];
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="wishdir"></param>
        /// <param name="wishspeed"></param>
        /// <param name="accel"></param>
        /// <param name="deltaTime"></param>
        /// <param name="surfaceFriction"></param>
        /// <returns></returns>
        public static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishdir, float wishspeed, float accel, float deltaTime, float surfaceFriction)
        {
            // See if we are changing direction a bit
            var currentspeed = Vector3.Dot(currentVelocity, wishdir);

            // Reduce wishspeed by the amount of veer.
            var addspeed = wishspeed - currentspeed;

            // If not going to add any speed, done.
            if (addspeed <= 0)
            {
                return Vector3.zero;
            }

            // Determine amount of accleration.
            var accelspeed = accel * deltaTime * wishspeed * surfaceFriction;

            // Cap at addspeed
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            var result = Vector3.zero;

            // Adjust velocity.
            for (int i = 0; i < 3; i++)
            {
                result[i] += accelspeed * wishdir[i];
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capc"></param>
        /// <param name="origin"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public static void GetCapsulePoints(CapsuleCollider capc, Vector3 origin, out Vector3 p1, out Vector3 p2)
        {
            var distanceToPoints = capc.height / 2f - capc.radius;
            p1 = origin + capc.center + Vector3.up * distanceToPoints;
            p2 = origin + capc.center - Vector3.up * distanceToPoints;
        }



        public static int ClipVelocity(Vector3 input, Vector3 normal, ref Vector3 output, float overbounce) {
            var angle = normal[1];
            var blocked = 0x00;         // Assume unblocked.
            if (angle > 0)          // If the plane that is blocking us has a positive z component, then assume it's a floor.
                blocked |= 0x01;    // 
            if (angle == 0)             // If the plane has no Z, it is vertical (wall/step)
                blocked |= 0x02;    // 

            // Determine how far along plane to slide based on incoming direction.
            var backoff = Vector3.Dot(input, normal) * overbounce;

            for (int i = 0; i < 3; i++) {
                var change = normal[i] * backoff;
                output[i] = input[i] - change;
            }

            // iterate once to make sure we aren't still moving through the plane
            float adjust = Vector3.Dot(output, normal);
            if (adjust < 0.0f) {
                output -= (normal * adjust);
                //		Msg( "Adjustment = %lf\n", adjust );
            }

            // Return blocking flags.
            return blocked;
        }
    }
}
