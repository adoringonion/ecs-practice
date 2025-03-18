using DefaultNamespace.Mob;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Mob
{
    public class MobAuthoring : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float escapeSpeed = 6f;
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float escapeThreshold = 5f;

        [Header("State Settings")]
        [SerializeField] private float directionChangeInterval = 2f;
        [SerializeField] private float deathDuration = 3f;

        class MobAuthoringBaker : Baker<MobAuthoring>
        {
            public override void Bake(MobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var mobComponent = new MobComponent()
                {
                    state = MobState.Normal,
                    timeUntilNextDirectionChange = 0,
                    moveSpeed = authoring.moveSpeed,
                    escapeSpeed = authoring.escapeSpeed,
                    deathTimer = authoring.deathDuration,
                    currentDirection = new Unity.Mathematics.float3(0, 0, 1),
                    detectionRadius = authoring.detectionRadius,
                    escapeThreshold = authoring.escapeThreshold
                };
                
                AddComponent(entity, mobComponent);
            }
        }
    }
}