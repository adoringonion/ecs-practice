using Unity.Burst;
using Unity.Entities;

public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
            
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {   
        foreach (var player in SystemAPI.Query<RefRW<PlayerInputComponent>>())
        {
            player.ValueRW.horizontal = UnityEngine.Input.GetAxis("Horizontal");
            player.ValueRW.vertical = UnityEngine.Input.GetAxis("Vertical");
        }
            
            
            
            
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}