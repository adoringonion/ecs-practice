using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

public class PlayerSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] PlayerAuthoring playerObject;
        
    private class PlayerSpawnerAuthoringBaker : Baker<PlayerSpawnerAuthoring>
    {
            
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSpawnerComponent {playerPrefab = GetEntity(authoring.playerObject.gameObject, TransformUsageFlags.Dynamic)});
        }
    }
}