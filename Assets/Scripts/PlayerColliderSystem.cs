using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct PlayerColliderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawnerComponent>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var job = new CollisionJob()
            {
                playerComponentLookup =  SystemAPI.GetComponentLookup<PlayerComponent>(),
                commandBuffer = commandBuffer.AsParallelWriter(),
                entity = SystemAPI.GetSingleton<PlayerSpawnerComponent>().playerPrefab
            };
            
            job.Schedule(simulation, state.Dependency).Complete();
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }

        [BurstCompile]
        struct CollisionJob : ICollisionEventsJob
        {
            public ComponentLookup<PlayerComponent> playerComponentLookup;
            public EntityCommandBuffer.ParallelWriter commandBuffer;
            public Entity entity;
            
            public void Execute(CollisionEvent collisionEvent)
            {
                var entityA = playerComponentLookup.HasComponent(collisionEvent.EntityA);
                var entityB = playerComponentLookup.HasComponent(collisionEvent.EntityB);
                
                if (entityA || entityB)
                {
                    var playerEntity = entityA ? collisionEvent.EntityA : collisionEvent.EntityB;
                    var player = playerComponentLookup.GetRefRW(playerEntity);
                    player.ValueRW.hp -= 1;
                    if (player.ValueRW.hp <= 0)
                    {
                        var newPlayerEntity = commandBuffer.Instantiate(0, entity);
                        commandBuffer.DestroyEntity(1, playerEntity);
                    }
                    
                }
            }
        }
    }
}