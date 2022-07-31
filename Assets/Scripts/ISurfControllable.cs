using UnityEngine;

namespace P90brush
{
    public interface ISurfControllable
    {
        PlayerData PlayerData { get; }
        InputData InputData { get; }
        Collider Collider { get; }
        Vector3 BaseVelocity { get; }
        Camera FpsCamera { get; }
    }
}
