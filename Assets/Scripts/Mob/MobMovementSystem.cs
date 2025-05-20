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
                playerPosition = _playerPosition,
                playerSpeed = _playerSpeed
            }.Schedule(state.Dependency);

            // モブの更新
            state.Dependency = new MobUpdateJob
            {
                deltaTime = deltaTime,
                random = _random,
                playerPosition = _playerPosition,
                playerSpeed = _playerSpeed
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
        }
    }

    [BurstCompile]
    public partial struct UpdatePlayerInfoJob : IJobEntity
    {
        public NativeReference<float3> playerPosition;
        public NativeReference<float> playerSpeed;

        void Execute(RefRO<LocalTransform> transform, RefRO<PlayerComponent> player)
        {
            playerPosition.Value = transform.ValueRO.Position;
            playerSpeed.Value = math.abs(player.ValueRO.CurrentSpeed);
        }
    }

    [BurstCompile]
    public partial struct MobUpdateJob : IJobEntity
    {
        public float deltaTime;
        public NativeArray<Random> random;
        [ReadOnly] public NativeReference<float3> playerPosition;
        [ReadOnly] public NativeReference<float> playerSpeed;

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
            var distanceToPlayer = math.length(playerPosition.Value - position);
            
            if (distanceToPlayer < mob.ValueRO.detectionRadius && playerSpeed.Value > mob.ValueRO.escapeThreshold)
            {
                mob.ValueRW.state = MobState.Escape;
                return;
            }

            mob.ValueRW.timeUntilNextDirectionChange -= deltaTime;
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

            // 移動を適用
            transform.ValueRW.Position += mob.ValueRO.currentDirection * mob.ValueRO.moveSpeed * deltaTime;
            
            // 移動方向に向きを設定（アニメーション視覚化のため）
            if (math.lengthsq(mob.ValueRO.currentDirection) > 0.001f)
            {
                quaternion targetRotation = quaternion.LookRotation(mob.ValueRO.currentDirection, new float3(0, 1, 0));
                transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRotation, deltaTime * 10f);
            }
        }

        void UpdateEscapeState(
            ref RefRW<MobComponent> mob,
            ref RefRW<LocalTransform> transform,
            float3 position)
        {
            var directionToPlayer = position - playerPosition.Value;
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
            
            // 移動適用
            transform.ValueRW.Position += mob.ValueRW.currentDirection * mob.ValueRO.escapeSpeed * deltaTime;
            
            // 逃走方向を向く（素早く回転）
            if (math.lengthsq(mob.ValueRO.currentDirection) > 0.001f)
            {
                quaternion targetRotation = quaternion.LookRotation(mob.ValueRO.currentDirection, new float3(0, 1, 0));
                transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRotation, deltaTime * 15f); // 逃走時は素早く回転
            }
        }

        private void UpdateDyingState(
            ref RefRW<MobComponent> mob,
            ref RefRW<LocalTransform> transform)
        {
            mob.ValueRW.deathTimer -= deltaTime;
            
            transform.ValueRW.Rotation = math.mul(
                transform.ValueRO.Rotation,
                quaternion.RotateY(math.radians(360f * deltaTime))
            );

            if (mob.ValueRO.deathTimer <= 0)
            {
                mob.ValueRW.state = MobState.Dead;
            }
        }
    }
}