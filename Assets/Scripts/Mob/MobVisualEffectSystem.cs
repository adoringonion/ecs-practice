using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mob
{
    /// <summary>
    /// モブの視覚的なエフェクトを処理するシステム
    /// </summary>
    [UpdateAfter(typeof(MobMovementSystem))]
    public partial struct MobVisualEffectSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // 視覚エフェクト更新ジョブをスケジュール
            state.Dependency = new UpdateVisualEffectsJob
            {
                DeltaTime = deltaTime,
                ElapsedTime = (float)SystemAPI.Time.ElapsedTime
            }.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// モブの視覚エフェクトを更新するジョブ
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVisualEffectsJob : IJobEntity
    {
        public float DeltaTime;
        public float ElapsedTime; // 経過時間（波動関数用）
        
        void Execute(
            RefRW<MobComponent> mob,
            RefRW<LocalTransform> transform)
        {
            // 現在の状態に基づいた視覚効果を適用
            switch (mob.ValueRO.state)
            {
                case MobState.Normal:
                    ApplyNormalVisualEffects(ref mob.ValueRW, ref transform.ValueRW, ElapsedTime);
                    break;
                    
                case MobState.Escape:
                    ApplyEscapeVisualEffects(ref mob.ValueRW, ref transform.ValueRW, ElapsedTime);
                    break;
                    
                case MobState.Dying:
                    ApplyDyingVisualEffects(ref mob.ValueRW, ref transform.ValueRW, DeltaTime);
                    break;
                    
                case MobState.Dead:
                    // 死亡状態では何もしない
                    break;
            }
        }
        
        private void ApplyNormalVisualEffects(
            ref MobComponent mob,
            ref LocalTransform transform,
            float elapsedTime)
        {
            // 初期位置からのY座標を計算
            mob.bobPhase += DeltaTime * mob.bobFrequency;
            
            // Sin波を使って上下の揺れを適用
            float bobOffset = math.sin(mob.bobPhase) * mob.bobHeight;
            
            // 既存の位置を保持しつつY軸方向のみ変更
            float3 currentPos = transform.Position;
            transform.Position = new float3(currentPos.x, bobOffset, currentPos.z);
            
            // 必要に応じてスケールやその他の視覚効果も追加できる
        }
        
        private void ApplyEscapeVisualEffects(
            ref MobComponent mob,
            ref LocalTransform transform,
            float elapsedTime)
        {
            // 逃走時は早く揺れる
            mob.bobPhase += DeltaTime * mob.bobFrequency * 2.0f;
            
            // Sin波を使って上下の揺れを適用（揺れ幅を大きくする）
            float bobOffset = math.sin(mob.bobPhase) * mob.bobHeight * 1.5f;
            
            // 既存の位置を保持しつつY軸方向のみ変更
            float3 currentPos = transform.Position;
            transform.Position = new float3(currentPos.x, bobOffset, currentPos.z);
            
            // ヨロヨロと揺れるような効果を追加
            quaternion wobble = quaternion.RotateY(math.sin(elapsedTime * 15f) * 0.1f);
            transform.Rotation = math.mul(transform.Rotation, wobble);
        }
        
        private void ApplyDyingVisualEffects(
            ref MobComponent mob,
            ref LocalTransform transform,
            float deltaTime)
        {
            // 死亡中は縮小効果
            float scale = math.lerp(1.0f, 0.2f, 1.0f - (mob.deathTimer / 3.0f));
            transform.Scale = scale;
            
            // Y軸回転で回転させながら沈んでいくエフェクト
            transform.Rotation = math.mul(
                transform.Rotation,
                quaternion.RotateY(math.radians(720f * deltaTime))
            );
            
            // 徐々に地面に沈んでいく
            float sinkAmount = math.lerp(0f, -0.5f, 1.0f - (mob.deathTimer / 3.0f));
            float3 currentPos = transform.Position;
            transform.Position = new float3(currentPos.x, sinkAmount, currentPos.z);
        }
    }
}
