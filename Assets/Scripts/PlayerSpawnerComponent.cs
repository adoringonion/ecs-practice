using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public struct PlayerSpawnerComponent : IComponentData
    {
        public Entity playerPrefab;
    }
}