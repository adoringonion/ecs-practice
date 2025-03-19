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
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial struct MobMovementSystem : ISystem, IDisposable
    {
        private NativeArray<Random> _random;
        private NativeReference<float3> _playerPosition;
        private NativeReference<float> _playerSpeed;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            
            _random = new NativeArray<Random>(1, Allocator.Persistent);
            _random[0] = Random.CreateFromIndex((uint)DateTime.Now.Ticks);
            
            _playerPosition = new NativeReference<float3>(float3.zero, Allocator.Persistent);
            _playerSpeed = new NativeReference<float>(0f, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_random.IsCreated) _random.Dispose();
            if (_playerPosition.IsCreated) _playerPosition.Dispose();
            if (_playerSpeed.IsCreated) _playerSpeed.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            // プレイヤー情報を更新
            state.Dependency = new UpdatePlayerInfoJob
            {
                PlayerPosition = _playerPosition,
                PlayerSpeed = _playerSpeed
            }.Schedule(state.Dependency);

            // モブの更新
            state.Dependency = new MobUpdateJob
            {
                DeltaTime = deltaTime,
                random = _random,
                PlayerPosition = _playerPosition,
                PlayerSpeed = _playerSpeed
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
        }
    }

    [BurstCompile]
    public partial struct UpdatePlayerInfoJob : IJobEntity
    {
        public NativeReference<float3> PlayerPosition;
        public NativeReference<float> PlayerSpeed;

        void Execute(RefRO<LocalTransform> transform, RefRO<PlayerComponent> player)
        {
            PlayerPosition.Value = transform.ValueRO.Position;
            PlayerSpeed.Value = math.abs(player.ValueRO.CurrentSpeed);
        }
    }

    [BurstCompile]
    public partial struct MobUpdateJob : IJobEntity
    {
        public float DeltaTime;
        public NativeArray<Random> random;
        [ReadOnly] public NativeReference<float3> PlayerPosition;
        [ReadOnly] public NativeReference<float> PlayerSpeed;

        void Execute(RefRW<MobComponent> mob, RefRW<LocalTransform> transform)
        {
            var position = transform.ValueRO.Position;
            var state = mob.ValueRO.state;

            switch (state)
            {
                case MobState.Normal:
                    UpdateNormalState(ref mob, ref transform, position);
                    break;
                case MobState.Escape:
                    UpdateEscapeState(ref mob, ref transform, position);
                    break;
                case MobState.Dying:
                    UpdateDyingState(ref mob, ref transform);
                    break;
            }
        }

        private void UpdateNormalState(
            ref RefRW<MobComponent> mob,
            ref RefRW<LocalTransform> transform,
            float3 position)
        {
            var distanceToPlayer = math.length(PlayerPosition.Value - position);
            
            if (distanceToPlayer < mob.ValueRO.detectionRadius && PlayerSpeed.Value > mob.ValueRO.escapeThreshold)
            {
                mob.ValueRW.state = MobState.Escape;
                return;
            }

            mob.ValueRW.timeUntilNextDirectionChange -= DeltaTime;
            if (mob.ValueRO.timeUntilNextDirectionChange <= 0)
            {
                var r = random[0];
                var randomAngle = r.NextFloat(-math.PI, math.PI);
                mob.ValueRW.currentDirection = new float3(
                    math.cos(randomAngle),
                    0,
                    math.sin(randomAngle)
                );
                mob.ValueRW.timeUntilNextDirectionChange = r.NextFloat(0.5f, 2f);
                random[0] = r; // 更新された乱数の状態を保存
            }

            transform.ValueRW.Position += mob.ValueRO.currentDirection * mob.ValueRO.moveSpeed * DeltaTime;
        }

        private void UpdateEscapeState(
            ref RefRW<MobComponent> mob,
            ref RefRW<LocalTransform> transform,
            float3 position)
        {
            var directionToPlayer = position - PlayerPosition.Value;
            var distanceToPlayer = math.length(directionToPlayer);

            if (distanceToPlayer > mob.ValueRO.detectionRadius * 1.5f)
            {
                mob.ValueRW.state = MobState.Normal;
                return;
            }

            if (distanceToPlayer > 0.1f)
            {
                mob.ValueRW.currentDirection = math.normalize(directionToPlayer);
            }

            transform.ValueRW.Position += mob.ValueRW.currentDirection * mob.ValueRO.escapeSpeed * DeltaTime;
        }

        private void UpdateDyingState(
            ref RefRW<MobComponent> mob,
            ref RefRW<LocalTransform> transform)
        {
            mob.ValueRW.deathTimer -= DeltaTime;
            
            transform.ValueRW.Rotation = math.mul(
                transform.ValueRO.Rotation,
                quaternion.RotateY(math.radians(360f * DeltaTime))
            );

            if (mob.ValueRO.deathTimer <= 0)
            {
                mob.ValueRW.state = MobState.Dead;
            }
        }
    }
}