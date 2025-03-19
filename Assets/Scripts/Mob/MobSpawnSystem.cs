using System;
using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Mob
{
    [UpdateAfter(typeof(MobMovementSystem))]
    public partial struct MobSpawnSystem : ISystem, IDisposable
    {
        private NativeArray<Random> _random;
        private EntityCommandBuffer.ParallelWriter _ecbParallel;
        private EntityCommandBuffer _ecb;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MobSpawner>();
            _random = new NativeArray<Random>(1, Allocator.Persistent);
            _random[0] = Random.CreateFromIndex((uint)System.DateTime.Now.Ticks);
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
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // Create ECB
            _ecb = new EntityCommandBuffer(Allocator.TempJob);
            _ecbParallel = _ecb.AsParallelWriter();

            // Dead状態のモブを削除
            state.Dependency = new DeadMobCleanupJob
            {
                ecb = _ecbParallel
            }.ScheduleParallel(state.Dependency);

            // 新しいモブをスポーン
            state.Dependency = new MobSpawnJob
            {
                deltaTime = deltaTime,
                random = _random,
                ecb = _ecbParallel
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
            
            // Execute and dispose ECB
            _ecb.Playback(state.EntityManager);
            _ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct DeadMobCleanupJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;

        void Execute([ReadOnly] in MobComponent mob, Entity entity, [EntityIndexInQuery] int sortKey)
        {
            if (mob.state == MobState.Dead)
            {
                ecb.DestroyEntity(sortKey, entity);
            }
        }
    }

    [BurstCompile]
    public partial struct MobSpawnJob : IJobEntity
    {
        public float deltaTime;
        public NativeArray<Random> random;
        public EntityCommandBuffer.ParallelWriter ecb;
        private const float SPAWN_RADIUS = 5f;

        void Execute(ref MobSpawner spawner, [ReadOnly] in LocalTransform transform, [EntityIndexInQuery] int sortKey)
        {
            if (!spawner.IsActive) return;
            
            spawner.TimeUntilNextSpawn -= deltaTime;
            if (spawner.TimeUntilNextSpawn > 0) return;

            var r = random[0];
            
            // スポーン間隔をランダムに設定
            spawner.TimeUntilNextSpawn = r.NextFloat(1f, 5f);

            // スポーン位置をランダムに設定
            var randomAngle = r.NextFloat(-math.PI, math.PI);
            var randomDistance = r.NextFloat(0f, SPAWN_RADIUS);
            var offset = new float3(
                math.cos(randomAngle) * randomDistance,
                0,
                math.sin(randomAngle) * randomDistance
            );

            var mobEntity = ecb.Instantiate(sortKey, spawner.MobPrefabEntity);
            ecb.SetComponent(sortKey, mobEntity, new LocalTransform
            {
                Position = transform.Position + offset,
                Scale = 1f,
                Rotation = quaternion.RotateY(randomAngle)
            });

            random[0] = r; // 更新された乱数の状態を保存
        }
    }
}