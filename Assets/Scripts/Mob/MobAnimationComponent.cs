using Unity.Entities;
using Unity.Collections;

namespace DefaultNamespace.Mob
{
    // アニメーションの状態を管理するコンポーネント
    public struct MobAnimationComponent : IComponentData
    {
        // アニメーションのパラメータ値
        public float MovementSpeed;     // 移動速度パラメータ
        public bool IsMoving;           // 移動中フラグ
        public bool IsEscaping;         // 逃走中フラグ
        public bool IsDying;            // 死亡中フラグ
        
        // アニメーション遷移用タイマー
        public float AnimationBlendTime;
        
        // Animatorパラメータ名（カスタマイズ可能）
        public FixedString32Bytes SpeedParameterName;
        public FixedString32Bytes IsMovingParameterName;
        public FixedString32Bytes IsEscapingParameterName;
        public FixedString32Bytes IsDyingParameterName;
    }
}
