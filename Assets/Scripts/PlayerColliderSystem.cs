using System;
using DefaultNamespace;
using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
public partial struct PlayerColliderSystem : ISystem, IDisposable
{
    private NativeArray<Random> _random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerSpawnerComponent>();
        state.RequireForUpdate<SimulationSingleton>();
            
        // システム時刻をシード値として使用
        _random = new NativeArray<Random>(1, Allocator.Persistent);
        _random[0] = Random.CreateFromIndex((uint)DateTime.Now.Ticks);
    }

    public void Dispose()
    {
        if (_random.IsCreated)
        {
            _random.Dispose();
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var job = new CollisionJob
        {
            PlayerComponentLookup = SystemAPI.GetComponentLookup<PlayerComponent>(true),
            MobComponentLookup = SystemAPI.GetComponentLookup<MobComponent>(),
            random = _random
        };
            
        state.Dependency = job.Schedule(simulation, state.Dependency);
        state.Dependency.Complete();
    }

    [BurstCompile]
    struct CollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<PlayerComponent> PlayerComponentLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<MobComponent> MobComponentLookup;
        public NativeArray<Random> random;
            
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityAIsPlayer = PlayerComponentLookup.HasComponent(collisionEvent.EntityA);
            var entityBIsPlayer = PlayerComponentLookup.HasComponent(collisionEvent.EntityB);
            var entityAIsMob = MobComponentLookup.HasComponent(collisionEvent.EntityA);
            var entityBIsMob = MobComponentLookup.HasComponent(collisionEvent.EntityB);
                
            if (!((entityAIsPlayer && entityBIsMob) || (entityBIsPlayer && entityAIsMob)))
            {
                return;
            }

            var playerEntity = entityAIsPlayer ? collisionEvent.EntityA : collisionEvent.EntityB;
            var mobEntity = entityAIsMob ? collisionEvent.EntityA : collisionEvent.EntityB;
                
            var player = PlayerComponentLookup.GetRefRO(playerEntity);
            var mob = MobComponentLookup.GetRefRW(mobEntity);
                
            if (math.abs(player.ValueRO.CurrentSpeed) <= mob.ValueRO.escapeThreshold)
            {
                return;
            }

            // Burstコンパイル対応のランダム値生成
            var randomAngle = random[0].NextFloat(-30f, 30f);
                
            // モブを吹き飛ばす方向を計算（プレイヤーの進行方向に基づく）
            var knockbackDirection = math.normalize(math.mul(
                quaternion.RotateY(math.radians(randomAngle)),
                player.ValueRO.Forward
            ));
                
            // モブの状態を死亡状態に変更
            mob.ValueRW.state = MobState.Dying;
            mob.ValueRW.currentDirection = knockbackDirection * player.ValueRO.CurrentSpeed * 2f;
        }
    }
}