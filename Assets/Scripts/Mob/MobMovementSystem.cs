using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DefaultNamespace.Mob
{
    [UpdateAfter(typeof(PlayerMovementSystem))]
    public partial struct MobMovementSystem : ISystem
    {
        private Unity.Mathematics.Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerComponent>();
            random = Unity.Mathematics.Random.CreateFromIndex(1234);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            random.InitState((uint)System.DateTime.Now.Ticks);

            // プレイヤーの位置と速度を取得
            float3 playerPosition = float3.zero;
            float playerSpeed = 0f;
            foreach (var (transform, player) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerComponent>>())
            {
                playerPosition = transform.ValueRO.Position;
                playerSpeed = math.abs(player.ValueRO.currentSpeed);
                break; // 最初のプレイヤーのみを対象とする
            }

            foreach (var (mob, transform) in SystemAPI.Query<RefRW<MobComponent>, RefRW<LocalTransform>>())
            {
                switch (mob.ValueRO.state)
                {
                    case MobState.Normal:
                        UpdateNormalState(mob, transform, deltaTime, playerPosition, playerSpeed);
                        break;
                    
                    case MobState.Escape:
                        UpdateEscapeState(mob, transform, deltaTime, playerPosition);
                        break;
                    
                    case MobState.Dying:
                        UpdateDyingState(mob, transform, deltaTime);
                        break;
                }
            }
        }

        private void UpdateNormalState(
            RefRW<MobComponent> mob,
            RefRW<LocalTransform> transform,
            float deltaTime,
            float3 playerPosition,
            float playerSpeed)
        {
            // プレイヤーとの距離をチェック
            var distanceToPlayer = math.length(playerPosition - transform.ValueRO.Position);
            
            // プレイヤーが近くにいて、速度が閾値を超えている場合は逃避状態に移行
            if (distanceToPlayer < mob.ValueRO.detectionRadius && playerSpeed > mob.ValueRO.escapeThreshold)
            {
                mob.ValueRW.state = MobState.Escape;
                return;
            }

            // 通常の移動処理
            mob.ValueRW.timeUntilNextDirectionChange -= deltaTime;
            if (mob.ValueRO.timeUntilNextDirectionChange <= 0)
            {
                // ランダムな方向を設定
                var randomAngle = random.NextFloat(-math.PI, math.PI);
                mob.ValueRW.currentDirection = new float3(
                    math.cos(randomAngle),
                    0,
                    math.sin(randomAngle)
                );
                mob.ValueRW.timeUntilNextDirectionChange = random.NextFloat(0.5f, 2f);
            }

            // 移動を適用
            transform.ValueRW.Position += mob.ValueRO.currentDirection * mob.ValueRO.moveSpeed * deltaTime;
        }

        private void UpdateEscapeState(
            RefRW<MobComponent> mob,
            RefRW<LocalTransform> transform,
            float deltaTime,
            float3 playerPosition)
        {
            var directionToPlayer = transform.ValueRO.Position - playerPosition;
            var distanceToPlayer = math.length(directionToPlayer);

            // プレイヤーから十分離れたら通常状態に戻る
            if (distanceToPlayer > mob.ValueRO.detectionRadius * 1.5f)
            {
                mob.ValueRW.state = MobState.Normal;
                return;
            }

            // プレイヤーから逃げる方向を設定
            if (distanceToPlayer > 0.1f)
            {
                mob.ValueRW.currentDirection = math.normalize(directionToPlayer);
            }

            // 逃避速度で移動
            transform.ValueRW.Position += mob.ValueRW.currentDirection * mob.ValueRO.escapeSpeed * deltaTime;
        }

        private void UpdateDyingState(
            RefRW<MobComponent> mob,
            RefRW<LocalTransform> transform,
            float deltaTime)
        {
            mob.ValueRW.deathTimer -= deltaTime;
            
            // 死亡アニメーション（ここでは単純に回転させる）
            transform.ValueRW.Rotation = math.mul(
                transform.ValueRO.Rotation,
                quaternion.RotateY(math.radians(360f * deltaTime))
            );

            if (mob.ValueRO.deathTimer <= 0)
            {
                mob.ValueRW.state = MobState.Dead;
            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}