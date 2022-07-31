using UnityEngine;


namespace P90brush
{
    public class SurfController
    {
        private ISurfControllable _surfer;
        private MovementConfig _config;
        private float _deltaTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfer"></param>
        /// <param name="config"></param>
        /// <param name="deltaTime"></param>
        public void ProcessMovement(ISurfControllable surfer, MovementConfig config, float deltaTime)
        {
            // cache instead of passing around parameters
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;

            // apply gravity
            if (_surfer.PlayerData.GroundObject == null)
            {
                _surfer.PlayerData.Velocity.y -= (_surfer.PlayerData.GravityFactor * _config.Gravity * _deltaTime);
                _surfer.PlayerData.Velocity.y += _surfer.BaseVelocity.y * _deltaTime;
            }

            // input velocity, check for ground
            CheckGrounded();
            CalculateMovementVelocity();

            // increment origin
            _surfer.PlayerData.Origin += _surfer.PlayerData.Velocity * _deltaTime;

            // don't penetrate walls
            SurfPhysics.ResolveCollisions(_surfer.Collider, ref _surfer.PlayerData.Origin, ref _surfer.PlayerData.Velocity);

            _surfer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CalculateMovementVelocity()
        {
            if (_surfer.PlayerData.GroundObject == null)
            {
                // apply movement from input
                _surfer.PlayerData.Velocity += AirInputMovement();

                // let the magic happen
                SurfPhysics.Reflect(ref _surfer.PlayerData.Velocity, _surfer.Collider, _surfer.PlayerData.Origin, _deltaTime);
            }
            else
            {
                // apply movement from input
                _surfer.PlayerData.Velocity += GroundInputMovement();

                // jump/friction
                if (_surfer.InputData.JumpPressed)
                {
                    Jump();
                }
                else
                {
                    var friction = _surfer.PlayerData.SurfaceFriction * _config.Friction;
                    var stopSpeed = _config.StopSpeed;
                    SurfPhysics.Friction(ref _surfer.PlayerData.Velocity, stopSpeed, friction, _deltaTime);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 GroundInputMovement()
        {
            GetWishValues(out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed);

            if ((wishSpeed != 0.0f) && (wishSpeed > _config.MaxSpeed))
            {
                wishVel *= _config.MaxSpeed / wishSpeed;
                wishSpeed = _config.MaxSpeed;
            }

            wishSpeed *= _surfer.PlayerData.WalkFactor;

            return SurfPhysics.Accelerate(_surfer.PlayerData.Velocity, wishDir,
                wishSpeed, _config.Accel, _deltaTime, _surfer.PlayerData.SurfaceFriction);
        }

        private Vector3 AirInputMovement()
        {
            GetWishValues(out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed);

            if (_config.ClampAirSpeed && (wishSpeed != 0f && (wishSpeed > _config.MaxSpeed)))
            {
                wishVel = wishVel * (_config.MaxSpeed / wishSpeed);
                wishSpeed = _config.MaxSpeed;
            }

            return SurfPhysics.AirAccelerate(_surfer.PlayerData.Velocity, wishDir,
                wishSpeed, _config.AirAccel, _config.AirCap, _deltaTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wishVel"></param>
        /// <param name="wishDir"></param>
        /// <param name="wishSpeed"></param>
        private void GetWishValues(out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed)
        {
            wishVel = Vector3.zero;
            wishDir = Vector3.zero;
            wishSpeed = 0f;

            Vector3 forward = _surfer.FpsCamera.transform.forward,
                right = _surfer.FpsCamera.transform.right;

            forward[1] = 0;
            right[1] = 0;
            forward.Normalize();
            right.Normalize();

            for (int i = 0; i < 3; i++)
                wishVel[i] = forward[i] * _surfer.InputData.ForwardMove + right[i] * _surfer.InputData.SideMove;
            wishVel[1] = 0;

            wishSpeed = wishVel.magnitude;
            wishDir = wishVel.normalized;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Jump()
        {
            if (!_config.AutoBhop && _surfer.InputData.JumpPressedLastUpdate)
                return;

            _surfer.PlayerData.Velocity.y += _config.JumpPower;
        }

        private bool CheckGrounded()
        {
            _surfer.PlayerData.SurfaceFriction = 1f;
            var movingUp = _surfer.PlayerData.Velocity.y > 0f;
            var trace = TraceToFloor();
            if (trace.HitCollider == null
                || trace.HitCollider.gameObject.layer == LayerMask.NameToLayer("Trigger")
                || trace.PlaneNormal.y < 0.7f
                || movingUp)
            {
                SetGround(null);
                if (movingUp)
                    _surfer.PlayerData.SurfaceFriction = _config.AirFriction;
                return false;
            }
            else
            {
                SetGround(trace.HitCollider.gameObject);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void SetGround(GameObject obj)
        {
            if (obj != null)
            {
                _surfer.PlayerData.GroundObject = obj;
                _surfer.PlayerData.Velocity.y = 0;
            }
            else
            {
                _surfer.PlayerData.GroundObject = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        private Trace TraceBounds(Vector3 start, Vector3 end, int layerMask)
        {
            return Tracer.TraceCollider(_surfer.Collider, start, end, layerMask);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Trace TraceToFloor()
        {
            var down = _surfer.PlayerData.Origin;
            down.y -= 0.1f;
            return Tracer.TraceCollider(_surfer.Collider, _surfer.PlayerData.Origin, down, SurfPhysics.GroundLayerMask);
        }
    }
}
