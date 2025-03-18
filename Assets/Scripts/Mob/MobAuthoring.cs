using DefaultNamespace.Mob;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Mob
{
    public class MobAuthoring : MonoBehaviour
    {
        class MobAuthoringBaker : Baker<MobAuthoring>
        {
            public override void Bake(MobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var mobComponent = new MobComponent()
                {
                    state = MobState.Normal,
                    timeUntilNextDirectionChange = 0
                };
                
                AddComponent(entity, mobComponent);
            }
        }
    }
}