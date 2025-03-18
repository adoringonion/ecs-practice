using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Mob
{
    [UpdateAfter(typeof(MobMovementSystem))]
    public partial struct MobSpawnSystem : ISystem
    {
        private Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MobSpawner>();
            random = Random.CreateFromIndex(1234);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            random.InitState((uint)System.DateTime.Now.Ticks);
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Dead状態のモブを削除
            foreach (var (mob, entity) in SystemAPI.Query<RefRO<MobComponent>>().WithEntityAccess())
            {
                if (mob.ValueRO.state == MobState.Dead)
                {
                    ecb.DestroyEntity(entity);
                }
            }

            // 新しいモブをスポーン
            foreach (var (spawner, transform) in SystemAPI.Query<RefRW<MobSpawner>, RefRO<LocalTransform>>())
            {
                if (!spawner.ValueRO.isActive) continue;
                spawner.ValueRW.timeUntilNextSpawn -= deltaTime;
                if (!(spawner.ValueRO.timeUntilNextSpawn <= 0)) continue;
                // スポーン間隔をランダムに設定
                spawner.ValueRW.timeUntilNextSpawn = random.NextFloat(1f, 5f);

                // スポーン位置をランダムに設定（スポナーの位置を中心に）
                var spawnRadius = 5f; // スポーン範囲の半径
                var randomAngle = random.NextFloat(-math.PI, math.PI);
                var randomDistance = random.NextFloat(0f, spawnRadius);
                var offset = new float3(
                    math.cos(randomAngle) * randomDistance,
                    0,
                    math.sin(randomAngle) * randomDistance
                );

                var mobEntity = ecb.Instantiate(spawner.ValueRO.mobPrefabEntity);
                ecb.SetComponent(mobEntity, new LocalTransform
                {
                    Position = transform.ValueRO.Position + offset,
                    Scale = 1f,
                    Rotation = quaternion.RotateY(randomAngle)
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}