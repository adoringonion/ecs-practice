using DefaultNamespace.Mob;
using Unity.Entities;
using UnityEngine;

namespace Mob
{
    public class MobAnimationAuthoring : MonoBehaviour
    {
        // Animatorコンポーネントへの参照（従来のGameObjectベースのアニメーション）
        [SerializeField] private Animator animator;
        
        // AnimatorControllerの設定
        [SerializeField] private RuntimeAnimatorController animatorController;
        
        // アニメーションのブレンド時間
        [SerializeField] private float animationBlendTime = 0.2f;
        
        // Awake/Start時にAnimatorがない場合は自動検索
        private void Start()
        {
            if (animator == null)
            {
                // 自身のGameObjectから検索
                animator = GetComponent<Animator>();
                
                // 子オブジェクトから検索
                if (animator == null)
                {
                    animator = GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        Debug.Log($"MobAnimationAuthoring: 子オブジェクト '{animator.gameObject.name}' からAnimatorを自動取得しました");
                    }
                }
            }
            
            // AnimatorControllerの設定確認
            if (animator != null && animator.runtimeAnimatorController == null && animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log($"MobAnimationAuthoring: Animatorにコントローラーを設定しました - {animatorController.name}");
            }
            
            // MobAnimatorSetupコンポーネントがなければ追加
            if (animator != null && !GetComponent<MobAnimatorSetup>())
            {
                var setup = gameObject.AddComponent<MobAnimatorSetup>();
                if (animatorController != null)
                {
                    setup.SendMessage("SetDefaultController", animatorController, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        
        // アニメーターパラメータ名（インスペクターでカスタマイズ可能）
        [Header("Animator Parameters")]
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private string isMovingParameterName = "IsMoving";
        [SerializeField] private string isEscapingParameterName = "IsEscaping";
        [SerializeField] private string isDyingParameterName = "IsDying";
        
        // オプション：特定の状態のリンクエンティティ名
        [Header("Animation States")]
        [SerializeField] private string idleAnimationStateName = "Idle";
        [SerializeField] private string walkAnimationStateName = "Walk";
        [SerializeField] private string runAnimationStateName = "Run";
        [SerializeField] private string dyingAnimationStateName = "Die";
        
        class MobAnimationBaker : Baker<MobAnimationAuthoring>
        {
            public override void Bake(MobAnimationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // 従来のGameObjectベースのアニメーションシステムとの橋渡し
                if (authoring.animator != null)
                {
                    // アニメーターリファレンスコンポーネントを追加（ハイブリッドアプローチ）
                    AddComponentObject(entity, authoring.animator);
                    
                    // 新しいAnimatorReferenceコンポーネントも追加
                    var animatorRef = new AnimatorReference
                    {
                        Value = authoring.animator,
                        SpeedParameterName = authoring.speedParameterName,
                        IsMovingParameterName = authoring.isMovingParameterName,
                        IsEscapingParameterName = authoring.isEscapingParameterName,
                        IsDyingParameterName = authoring.isDyingParameterName
                    };
                    
                    AddComponentObject(entity, animatorRef);
                    Debug.Log($"MobAnimationBaker: エンティティ{entity.Index}にAnimatorReferenceを追加しました - {authoring.animator.name}");
                }
                
                // ECS用のアニメーションコンポーネントを追加
                var animationComponent = new MobAnimationComponent
                {
                    MovementSpeed = 0f,
                    IsMoving = false,
                    IsEscaping = false,
                    IsDying = false,
                    AnimationBlendTime = authoring.animationBlendTime,
                    
                    // アニメーターパラメータ名を追加
                    SpeedParameterName = authoring.speedParameterName,
                    IsMovingParameterName = authoring.isMovingParameterName,
                    IsEscapingParameterName = authoring.isEscapingParameterName,
                    IsDyingParameterName = authoring.isDyingParameterName
                };
                
                AddComponent(entity, animationComponent);
            }
        }
    }
}
