using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Mob
{
    /// <summary>
    /// Animatorへの参照を保持するマネージドコンポーネント
    /// これによりECSシステムからAnimatorにアクセスしやすくなります
    /// </summary>
    public class AnimatorReference : IComponentData
    {
        public Animator Value;
        
        // パラメーター名キャッシュ（オプション）
        public string SpeedParameterName = "Speed";
        public string IsMovingParameterName = "IsMoving";
        public string IsEscapingParameterName = "IsEscaping";
        public string IsDyingParameterName = "IsDying";
        
        // キャッシュされたパラメーターインデックス（パフォーマンス向上に役立ちます）
        public int SpeedParameterHash;
        public int IsMovingParameterHash;
        public int IsEscapingParameterHash; 
        public int IsDyingParameterHash;
        
        public bool Initialized = false;
        
        public void Initialize()
        {
            if (Value != null && !Initialized)
            {
                // パラメーターハッシュを初期化
                SpeedParameterHash = Animator.StringToHash(SpeedParameterName);
                IsMovingParameterHash = Animator.StringToHash(IsMovingParameterName);
                IsEscapingParameterHash = Animator.StringToHash(IsEscapingParameterName);
                IsDyingParameterHash = Animator.StringToHash(IsDyingParameterName);
                
                Initialized = true;
            }
        }
    }
}
