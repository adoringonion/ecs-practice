using Unity.Entities;
using UnityEngine;

namespace Mob
{
    public class MobSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] MobAuthoring mobPrefab;
        [SerializeField] float timeBetweenSpawns = 1f;
        [SerializeField] bool isActive = true;

        class MobSpawnerAuthoringBaker : Baker<MobSpawnerAuthoring>
        {
            public override void Bake(MobSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var spawnerComponent = new MobSpawner()
                {
                    mobPrefabEntity = GetEntity(authoring.mobPrefab, TransformUsageFlags.None),
                    timeUntilNextSpawn = authoring.timeBetweenSpawns,
                    isActive = authoring.isActive
                };
                
                AddComponent(entity, spawnerComponent);
            }
        }
    }
}