using System.ComponentModel;
using Unity.Entities;

namespace DefaultNamespace.Mob
{
    public struct MobComponent : IComponentData
    {
        public MobState state;
        public float timeUntilNextDirectionChange;
    }

    public enum MobState
    {
        Normal,
        EsCape,
        Death
    }
}