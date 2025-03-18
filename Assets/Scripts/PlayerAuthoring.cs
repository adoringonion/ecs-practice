using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private int hp;
        
        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerComponent {hp = authoring.hp});
                AddComponent(entity, new PlayerInputComponent());
            }
        }
    }
}