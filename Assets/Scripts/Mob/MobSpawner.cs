using Unity.Entities;

namespace Mob
{
    public struct MobSpawner : IComponentData
    {
        public Entity MobPrefabEntity;
        public float TimeUntilNextSpawn;
        public bool IsActive;
    }
}