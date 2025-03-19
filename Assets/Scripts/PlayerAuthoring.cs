using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float rotationSpeed = 180f;
        
    private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponent
            {
                CurrentSpeed = 0f,
                MaxSpeed = authoring.maxSpeed,
                Acceleration = authoring.acceleration,
                Deceleration = authoring.deceleration,
                RotationSpeed = authoring.rotationSpeed,
                Forward = new Unity.Mathematics.float3(0, 0, 1)
            });
            AddComponent(entity, new PlayerInputComponent());
        }
    }
}