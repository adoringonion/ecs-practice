using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;

namespace DefaultNamespace.Mob
{
    public struct MobComponent : IComponentData
    {
        public MobState state;
        public float timeUntilNextDirectionChange;
        public float moveSpeed;
        public float escapeSpeed;
        public float deathTimer;
        public float3 currentDirection;
        public float detectionRadius;
        public float escapeThreshold; // プレイヤーの速度がこの値を超えると逃げ出す
    }

    public enum MobState
    {
        Normal,
        Escape,
        Dying,
        Dead
    }
}