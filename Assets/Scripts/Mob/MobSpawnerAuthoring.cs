using Unity.Entities;
using UnityEngine;

namespace Mob
{
    public class MobSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] MobAuthoring mobPrefab;
        [SerializeField] float timeBetweenSpawns = 1f;
        [SerializeField] bool isActive = true;

        private class MobSpawnerAuthoringBaker : Baker<MobSpawnerAuthoring>
        {
            public override void Bake(MobSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var spawnerComponent = new MobSpawner()
                {
                    MobPrefabEntity = GetEntity(authoring.mobPrefab, TransformUsageFlags.Dynamic),
                    TimeUntilNextSpawn = authoring.timeBetweenSpawns,
                    IsActive = authoring.isActive
                };
                
                AddComponent(entity, spawnerComponent);
            }
        }
    }
}