using DefaultNamespace.Mob;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;

namespace Mob
{
    /// <summary>
    /// モブのアニメーションを制御するシステム
    /// </summary>
    [UpdateAfter(typeof(MobMovementSystem))]
    public partial struct MobAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // アニメーション更新ジョブをスケジュール
            state.Dependency = new UpdateMobAnimationsJob
            {
                DeltaTime = deltaTime
            }.ScheduleParallel(state.Dependency);
            
            // アニメーション同期はAnimatorSyncSystemで処理
        }
    }

    /// <summary>
    /// モブのアニメーション状態を更新するジョブ
    /// </summary>
    [BurstCompile]
    public partial struct UpdateMobAnimationsJob : IJobEntity
    {
        public float DeltaTime;
        
        void Execute(
            RefRW<MobAnimationComponent> animComp,
            in MobComponent mob,
            in LocalTransform transform)
        {
            // 前回フレームからの移動量を計算するには本来であれば前回位置が必要
            // ここでは簡易的にモブコンポーネントの状態を元に計算
            
            // 状態に応じたアニメーションパラメータの更新
            switch (mob.state)
            {
                case MobState.Normal:
                    animComp.ValueRW.IsMoving = true;
                    animComp.ValueRW.IsEscaping = false;
                    animComp.ValueRW.IsDying = false;
                    animComp.ValueRW.MovementSpeed = mob.moveSpeed;
                    break;
                    
                case MobState.Escape:
                    animComp.ValueRW.IsMoving = true;
                    animComp.ValueRW.IsEscaping = true;
                    animComp.ValueRW.IsDying = false;
                    animComp.ValueRW.MovementSpeed = mob.escapeSpeed;
                    break;
                    
                case MobState.Dying:
                    animComp.ValueRW.IsMoving = false;
                    animComp.ValueRW.IsEscaping = false;
                    animComp.ValueRW.IsDying = true;
                    animComp.ValueRW.MovementSpeed = 0f;
                    break;
                    
                case MobState.Dead:
                    animComp.ValueRW.IsMoving = false;
                    animComp.ValueRW.IsEscaping = false;
                    animComp.ValueRW.IsDying = false;
                    animComp.ValueRW.MovementSpeed = 0f;
                    break;
            }
        }
    }

    // AnimatorSyncSystemは別ファイルに移動しました
}
