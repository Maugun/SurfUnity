using UnityEngine;

namespace P90brush
{
    public class Tracer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static Trace TraceCollider(Collider collider, Vector3 origin, Vector3 end, int layerMask)
        {
            if (collider is BoxCollider)
            {
                return TraceBox(origin, end, collider.bounds.extents, collider.contactOffset, layerMask);
            }
            else if (collider is CapsuleCollider capc)
            {
                SurfPhysics.GetCapsulePoints(capc, origin, out Vector3 point1, out Vector3 point2);
                return TraceCapsule(point1, point2, capc.radius, origin, end, capc.contactOffset, layerMask);
            }

            throw new System.NotImplementedException("Trace missing for collider: " + collider.GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="radius"></param>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <param name="contactOffset"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static Trace TraceCapsule(Vector3 point1, Vector3 point2, float radius, Vector3 start, Vector3 destination, float contactOffset, int layerMask)
        {
            var result = new Trace()
            {
                StartPos = start,
                EndPos = destination
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            radius *= (1f - contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance(start, destination) + longSide;

            if (Physics.CapsuleCast(
                point1: point1,
                point2: point2,
                radius: radius,
                direction: direction,
                hitInfo: out RaycastHit hit,
                maxDistance: maxDistance,
                layerMask: layerMask))
            {
                result.Fraction = hit.distance / maxDistance;
                result.HitCollider = hit.collider;
                result.HitPoint = hit.point;
                result.PlaneNormal = hit.normal;
            }
            else
            {
                result.Fraction = 1;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <param name="extents"></param>
        /// <param name="contactOffset"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static Trace TraceBox(Vector3 start, Vector3 destination, Vector3 extents, float contactOffset, int layerMask)
        {
            var result = new Trace()
            {
                StartPos = start,
                EndPos = destination
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance(start, destination) + longSide;
            extents *= (1f - contactOffset);

            if (Physics.BoxCast(center: start,
                halfExtents: extents,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                hitInfo: out RaycastHit hit,
                layerMask: layerMask))
            {
                result.Fraction = hit.distance / maxDistance;
                result.HitCollider = hit.collider;
                result.HitPoint = hit.point;
                result.PlaneNormal = hit.normal;
            }
            else
            {
                result.Fraction = 1;
            }

            return result;
        }
    }
}