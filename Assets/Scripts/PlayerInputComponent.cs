using Unity.Entities;

namespace DefaultNamespace
{
    public struct PlayerInputComponent : IComponentData
    {
        public float horizontal;
        public float vertical;
    }
}