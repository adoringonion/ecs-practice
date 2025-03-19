using Unity.Entities;
using Unity.Mathematics;

public struct PlayerComponent : IComponentData
{
    public float CurrentSpeed;
    public float MaxSpeed;
    public float Acceleration;
    public float Deceleration;
    public float RotationSpeed;
    public float3 Forward;
}