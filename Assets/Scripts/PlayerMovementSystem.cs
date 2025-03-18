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
            state.RequireForUpdate<PlayerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (transform, player, playerInput) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerComponent>, RefRO<PlayerInputComponent>>())
            {
                // Handle rotation
                if (math.abs(playerInput.ValueRO.horizontal) > 0.1f)
                {
                    var rotationAmount = playerInput.ValueRO.horizontal * player.ValueRO.rotationSpeed * deltaTime;
                    var rotation = quaternion.RotateY(math.radians(rotationAmount));
                    transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rotation);
                    player.ValueRW.forward = math.rotate(transform.ValueRO.Rotation, new float3(0, 0, 1));
                }

                // Handle acceleration/deceleration
                if (playerInput.ValueRO.vertical > 0.1f)
                {
                    // Accelerate
                    player.ValueRW.currentSpeed = math.min(
                        player.ValueRO.currentSpeed + player.ValueRO.acceleration * deltaTime,
                        player.ValueRO.maxSpeed
                    );
                }
                else if (playerInput.ValueRO.vertical < -0.1f)
                {
                    // Reverse
                    player.ValueRW.currentSpeed = math.max(
                        player.ValueRO.currentSpeed - player.ValueRO.acceleration * deltaTime,
                        -player.ValueRO.maxSpeed * 0.5f // Reverse speed is half of forward speed
                    );
                }
                else
                {
                    // Decelerate
                    player.ValueRW.currentSpeed = math.lerp(
                        player.ValueRO.currentSpeed,
                        0,
                        player.ValueRO.deceleration * deltaTime
                    );
                }

                // Apply movement
                if (math.abs(player.ValueRO.currentSpeed) > 0.01f)
                {
                    transform.ValueRW.Position += player.ValueRO.forward * player.ValueRO.currentSpeed * deltaTime;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}