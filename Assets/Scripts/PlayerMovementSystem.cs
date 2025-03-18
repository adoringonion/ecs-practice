using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, playerInput) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerInputComponent>>())
            {
                var direction = math.normalizesafe(new Vector2(playerInput.ValueRO.horizontal, playerInput.ValueRO.vertical));
                var move = direction * 5 * state.WorldUnmanaged.Time.DeltaTime;
                transform.ValueRW.Position += new float3(move.x, 0, move.y);
            }
            
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}