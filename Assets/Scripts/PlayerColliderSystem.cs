using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct PlayerColliderSystem : ISystem
    {
        private Random _random;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawnerComponent>();
            state.RequireForUpdate<SimulationSingleton>();
            _random = new Random(1234);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var job = new CollisionJob()
            {
                playerComponentLookup = SystemAPI.GetComponentLookup<PlayerComponent>(),
                mobComponentLookup = SystemAPI.GetComponentLookup<MobComponent>(false), // false for read-write access
                Random = _random
            };
            
            job.Schedule(simulation, state.Dependency).Complete();
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }

        [BurstCompile]
        struct CollisionJob : ICollisionEventsJob
        {
            public ComponentLookup<PlayerComponent> playerComponentLookup;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<MobComponent> mobComponentLookup;
            public Random Random;
            
            public void Execute(CollisionEvent collisionEvent)
            {
                var entityAIsPlayer = playerComponentLookup.HasComponent(collisionEvent.EntityA);
                var entityBIsPlayer = playerComponentLookup.HasComponent(collisionEvent.EntityB);
                var entityAIsMob = mobComponentLookup.HasComponent(collisionEvent.EntityA);
                var entityBIsMob = mobComponentLookup.HasComponent(collisionEvent.EntityB);
                
                // プレイヤーとモブの衝突を処理
                if ((entityAIsPlayer && entityBIsMob) || (entityBIsPlayer && entityAIsMob))
                {
                    var playerEntity = entityAIsPlayer ? collisionEvent.EntityA : collisionEvent.EntityB;
                    var mobEntity = entityAIsMob ? collisionEvent.EntityA : collisionEvent.EntityB;
                    
                    var player = playerComponentLookup.GetRefRW(playerEntity);
                    var mob = mobComponentLookup.GetRefRW(mobEntity);
                    
                    // プレイヤーの速度が閾値を超えている場合のみモブを死亡させる
                    if (math.abs(player.ValueRO.currentSpeed) > mob.ValueRO.escapeThreshold)
                    {
                        // モブを吹き飛ばす方向を計算（プレイヤーの進行方向に基づく）
                        var knockbackDirection = math.normalize(math.mul(
                            quaternion.RotateY(math.radians(
                                Random.NextFloat(-30f, 30f))), // ランダムな角度のばらつきを追加
                            player.ValueRO.forward
                        ));
                        
                        // モブの状態を死亡状態に変更
                        mob.ValueRW.state = MobState.Dying;
                        mob.ValueRW.currentDirection = knockbackDirection * player.ValueRO.currentSpeed * 2f; // 現在の速度の2倍の力で吹き飛ばす
                    }
                }
            }
        }
    }
}