using Unity.Entities;
using Unity.Mathematics;

public struct PlayerComponent : IComponentData
{
    public float currentSpeed;
    public float maxSpeed;
    public float acceleration;
    public float deceleration;
    public float rotationSpeed;
    public float3 forward;
}