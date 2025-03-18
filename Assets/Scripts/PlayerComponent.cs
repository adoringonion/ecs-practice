using Unity.Entities;

namespace DefaultNamespace
{
    public struct PlayerComponent : IComponentData
    {
        public int hp;
        public Entity prefab;
    }
}