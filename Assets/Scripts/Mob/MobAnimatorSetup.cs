using UnityEngine;

namespace DefaultNamespace.Mob
{
    /// <summary>
    /// モブのAnimatorControllerを設定するヘルパークラス
    /// ランタイムにAnimatorControllerを設定します
    /// </summary>
    public class MobAnimatorSetup : MonoBehaviour
    {
        // モブ用のデフォルトAnimatorController
        [SerializeField] private RuntimeAnimatorController defaultController;
        
        // インスペクタで未設定の場合、自動的に検索する
        private static RuntimeAnimatorController _cachedDefaultController;
        
        // 外部からAnimatorControllerを設定するためのメソッド
        public void SetDefaultController(RuntimeAnimatorController controller)
        {
            if (controller != null)
            {
                defaultController = controller;
                _cachedDefaultController = controller;
                
                // 即時設定を試みる
                SetupAnimator();
            }
        }
        
        private void Start()
        {
            SetupAnimator();
        }
        
        // Animatorのセットアップを行う
        public void SetupAnimator()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning($"MobAnimatorSetup: {gameObject.name} にAnimatorコンポーネントが見つかりません");
                    return;
                }
            }
            
            // AnimatorControllerが設定されているか確認
            if (animator.runtimeAnimatorController == null)
            {
                // デフォルトのAnimatorControllerを使用
                RuntimeAnimatorController controller = defaultController;
                
                // デフォルトが未設定の場合は自動的に検索
                if (controller == null)
                {
                    controller = FindDefaultAnimatorController();
                }
                
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log($"MobAnimatorSetup: {gameObject.name} にAnimatorControllerを設定しました: {controller.name}");
                }
                else
                {
                    Debug.LogError($"MobAnimatorSetup: AnimatorControllerが見つかりません。Mobプレハブにコントローラーを設定してください");
                }
            }
        }
        
        // プロジェクト内のデフォルトAnimatorControllerを検索
        private RuntimeAnimatorController FindDefaultAnimatorController()
        {
            if (_cachedDefaultController != null)
            {
                return _cachedDefaultController;
            }
            
            // Resources/Animators フォルダ内のAnimatorControllerを検索
            RuntimeAnimatorController[] controllers = Resources.LoadAll<RuntimeAnimatorController>("Animators");
            if (controllers != null && controllers.Length > 0)
            {
                _cachedDefaultController = controllers[0];
                return _cachedDefaultController;
            }
            
            // 名前で検索する方法
            var controller = Resources.Load<RuntimeAnimatorController>("Mob");
            if (controller != null)
            {
                _cachedDefaultController = controller;
                return controller;
            }
            
            Debug.LogWarning("MobAnimatorSetup: デフォルトのAnimatorControllerが見つかりませんでした");
            return null;
        }
    }
}
