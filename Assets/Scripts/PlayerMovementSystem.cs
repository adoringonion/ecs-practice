using DefaultNamespace;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        // RotationとMovementを別々のジョブで処理
        state.Dependency = new PlayerRotationJob
        {
            DeltaTime = deltaTime
        }.Schedule(state.Dependency);

        state.Dependency = new PlayerMovementJob
        {
            DeltaTime = deltaTime
        }.Schedule(state.Dependency);

        state.Dependency.Complete();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}

[BurstCompile]
public partial struct PlayerRotationJob : IJobEntity
{
    public float DeltaTime;

    void Execute(RefRW<LocalTransform> transform, RefRW<PlayerComponent> player, [ReadOnly] in PlayerInputComponent input)
    {
        if (math.abs(input.horizontal) <= 0.1f)
        {
            return;
        }

        var rotationAmount = input.horizontal * player.ValueRO.RotationSpeed * DeltaTime;
        var rotation = quaternion.RotateY(math.radians(rotationAmount));
        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rotation);
        player.ValueRW.Forward = math.rotate(transform.ValueRO.Rotation, new float3(0, 0, 1));
    }
}

[BurstCompile]
public partial struct PlayerMovementJob : IJobEntity
{
    public float DeltaTime;

    void Execute(RefRW<LocalTransform> transform, RefRW<PlayerComponent> player, [ReadOnly] in PlayerInputComponent input)
    {
        UpdateSpeed(ref player, input.vertical);
        ApplyMovement(ref transform, player);
    }

    private void UpdateSpeed(ref RefRW<PlayerComponent> player, float verticalInput)
    {
        if (verticalInput > 0.1f)
        {
            // Accelerate
            player.ValueRW.CurrentSpeed = math.min(
                player.ValueRO.CurrentSpeed + player.ValueRO.Acceleration * DeltaTime,
                player.ValueRO.MaxSpeed
            );
        }
        else if (verticalInput < -0.1f)
        {
            // Reverse
            player.ValueRW.CurrentSpeed = math.max(
                player.ValueRO.CurrentSpeed - player.ValueRO.Acceleration * DeltaTime,
                -player.ValueRO.MaxSpeed * 0.5f // Reverse speed is half of forward speed
            );
        }
        else
        {
            // Decelerate
            player.ValueRW.CurrentSpeed = math.lerp(
                player.ValueRO.CurrentSpeed,
                0,
                player.ValueRO.Deceleration * DeltaTime
            );
        }
    }

    private void ApplyMovement(ref RefRW<LocalTransform> transform, RefRW<PlayerComponent> player)
    {
        if (math.abs(player.ValueRO.CurrentSpeed) <= 0.01f)
        {
            return;
        }

        transform.ValueRW.Position += player.ValueRO.Forward * player.ValueRO.CurrentSpeed * DeltaTime;
    }
}