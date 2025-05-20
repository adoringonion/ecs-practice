using DefaultNamespace.Mob;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace Mob
{
    /// <summary>
    /// Animatorコンポーネントの更新を行うシステム
    /// このシステムはBurst対応していません（UnityEngineオブジェクト操作のため）
    /// </summary>
    [UpdateAfter(typeof(MobAnimationSystem))]
    public partial class AnimatorSyncSystem : SystemBase
    {
        // Animator参照キャッシュ
        private List<(Entity entity, Animator animator, AnimatorReference reference)> _animators = new();
        private bool _initialized = false;
        
        protected override void OnCreate()
        {
            Debug.Log("AnimatorSyncSystem: OnCreateが呼び出されました");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Debug.Log("AnimatorSyncSystem: OnStartRunningが呼び出されました");
            _initialized = false;  // 再初期化を強制
        }
        
        // Animatorリファレンスの更新と初期化
        private void UpdateAnimatorReferences()
        {
            _animators.Clear();
            
            // 必要なコンポーネントを持つエンティティを検索
            EntityQuery query = GetEntityQuery(
                ComponentType.ReadOnly<MobAnimationComponent>(),
                ComponentType.ReadOnly<AnimatorReference>()
            );
            
            if (query.CalculateEntityCount() > 0)
            {
                Debug.Log($"AnimatorSyncSystem: AnimatorReferenceを持つエンティティが{query.CalculateEntityCount()}個見つかりました");
                
                // エンティティを列挙
                var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach (var entity in entities)
                {
                    var animRef = EntityManager.GetComponentObject<AnimatorReference>(entity);
                    if (animRef != null && animRef.Value != null)
                    {
                        // 初期化
                        if (!animRef.Initialized)
                        {
                            animRef.Initialize();
                        }
                        
                        _animators.Add((entity, animRef.Value, animRef));
                        Debug.Log($"AnimatorSyncSystem: エンティティ{entity.Index}のAnimator '{animRef.Value.name}' を登録しました");
                    }
                }
                
                entities.Dispose();
            }
            
            _initialized = true;
        }

        protected override void OnUpdate()
        {
            if (!_initialized || UnityEngine.Time.frameCount % 60 == 0)  // 60フレームごとに再チェック
            {
                UpdateAnimatorReferences();
            }
            
            if (_animators.Count == 0)
            {
                return;
            }
            
            foreach (var (entity, animator, reference) in _animators)
            {
                if (animator == null) continue;
                
                if (EntityManager.HasComponent<MobAnimationComponent>(entity))
                {
                    var animComp = EntityManager.GetComponentData<MobAnimationComponent>(entity);
                    
                    try
                    {
                        // ハッシュを使って高速アクセス
                        if (reference.Initialized)
                        {
                            animator.SetFloat(reference.SpeedParameterHash, animComp.MovementSpeed);
                            animator.SetBool(reference.IsMovingParameterHash, animComp.IsMoving);
                            animator.SetBool(reference.IsEscapingParameterHash, animComp.IsEscaping);
                            animator.SetBool(reference.IsDyingParameterHash, animComp.IsDying);
                        }
                        else
                        {
                            // フォールバック：文字列パラメータ名を使用
                            if (HasParameter(animator, reference.SpeedParameterName))
                                animator.SetFloat(reference.SpeedParameterName, animComp.MovementSpeed);
                            
                            if (HasParameter(animator, reference.IsMovingParameterName))
                                animator.SetBool(reference.IsMovingParameterName, animComp.IsMoving);
                            
                            if (HasParameter(animator, reference.IsEscapingParameterName))
                                animator.SetBool(reference.IsEscapingParameterName, animComp.IsEscaping);
                            
                            if (HasParameter(animator, reference.IsDyingParameterName))
                                animator.SetBool(reference.IsDyingParameterName, animComp.IsDying);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"AnimatorSyncSystem: パラメータ更新中にエラー: {e.Message}");
                    }
                }
            }
        }
        
        // アニメーターに指定したパラメータが存在するか確認するヘルパーメソッド
        private static bool HasParameter(Animator animator, string paramName)
        {
            if (string.IsNullOrEmpty(paramName) || animator == null) return false;
            
            foreach (var param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
