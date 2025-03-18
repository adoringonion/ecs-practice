using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DefaultNamespace.Mob
{
    public partial struct MobMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            foreach (var (mob, transform) in SystemAPI.Query<RefRW<MobComponent>, RefRW<LocalTransform>>())
            {
                if (mob.ValueRO.state == MobState.Normal)
                {
                    var direction = new float3(UnityEngine.Random.Range(-1, 1), 0, UnityEngine.Random.Range(-1, 1));
                    mob.ValueRW.timeUntilNextDirectionChange -= state.WorldUnmanaged.Time.DeltaTime;
                    if (mob.ValueRO.timeUntilNextDirectionChange <= 0)
                    {
                        mob.ValueRW.timeUntilNextDirectionChange = UnityEngine.Random.Range(0.5f, 2f); 
                        transform.ValueRW.Position += direction;
                    }
                }
                
            }
        }
        
        public void OnUpdate(ref SystemState state)
        {
            
        }
        
        public void OnDestroy(ref SystemState state)
        {
            
        }
        


    }
}