using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Mob
{
    public partial struct MobSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MobSpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            foreach (var (spawner, transform) in SystemAPI.Query<RefRW<MobSpawner>, RefRW<LocalTransform>>())
            {
                if (spawner.ValueRO.isActive)
                {
                    spawner.ValueRW.timeUntilNextSpawn -= deltaTime;
                    if (spawner.ValueRO.timeUntilNextSpawn <= 0)
                    {
                        spawner.ValueRW.timeUntilNextSpawn = UnityEngine.Random.Range(1, 5);
                        var mobEntity = state.EntityManager.Instantiate(spawner.ValueRO.mobPrefabEntity);
                        state.EntityManager.SetComponentData(mobEntity, new LocalTransform
                        {
                            Position = transform.ValueRO.Position
                        });
                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}