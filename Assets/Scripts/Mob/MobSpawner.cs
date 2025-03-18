using Unity.Entities;

namespace Mob
{
    public struct MobSpawner : IComponentData
    {
        public Entity mobPrefabEntity;
        public float timeUntilNextSpawn;
        public bool isActive;
    }
}