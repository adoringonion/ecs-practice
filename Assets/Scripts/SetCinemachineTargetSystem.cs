using DefaultNamespace;
using Unity.Cinemachine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

public partial class SetCinemachineTargetSystem : SystemBase
{
    private CinemachineCamera virtualCamera;
    private PlayerCameraTarget playerCameraTarget;
    private EntityQuery playerQuery;
    private bool isCameraInitialized = false;

    protected override void OnCreate()
    {
        // PlayerComponentを持つエンティティのクエリを作成
        playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerComponent>(), ComponentType.ReadOnly<LocalTransform>());
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        // OnStartRunningはシーンが完全にロードされた後に呼ばれます
        FindCameraAndTarget();
    }

    private void FindCameraAndTarget()
    {
        // シーン内のVirtualCameraを検索
        virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineCameraが見つかりません。自動生成します。");
            CreateCinemachineCamera();
        }
        else
        {
            Debug.Log("CinemachineCamera見つかりました: " + virtualCamera.name);
        }
        
        playerCameraTarget = Object.FindFirstObjectByType<PlayerCameraTarget>();
        if (playerCameraTarget == null)
        {
            Debug.LogError("PlayerCameraTargetが見つかりません。カメラ追従用のターゲットを作成します。");
            // ターゲットオブジェクトを作成
            var targetObject = new GameObject("PlayerCameraTarget");
            playerCameraTarget = targetObject.AddComponent<PlayerCameraTarget>();
        }
        
        isCameraInitialized = true;
    }

    protected override void OnUpdate()
    {
        // カメラがまだ初期化されていない場合、再度試行
        if (!isCameraInitialized || virtualCamera == null)
        {
            FindCameraAndTarget();
            if (!isCameraInitialized) return;
        }
        
        // サブシーンを含めたすべてのPlayerエンティティから対象を見つける
        if (playerQuery.CalculateEntityCount() > 0)
        {
            // エンティティをNativeArrayとして取得
            NativeArray<Entity> playerEntities = playerQuery.ToEntityArray(Allocator.Temp);
            
            if (playerEntities.Length > 0)
            {
                // 最初に見つかったPlayerを対象とする（複数ある場合は選択ロジックを追加できます）
                Entity targetEntity = playerEntities[0];
                var targetPosition = SystemAPI.GetComponent<LocalTransform>(targetEntity).Position;
                
                // カメラ追従用のターゲット位置を更新
                playerCameraTarget.transform.position = targetPosition;
                
                // Cinemachineカメラのフォロー対象をセット
                if (virtualCamera.Follow != playerCameraTarget.transform)
                {
                    virtualCamera.Follow = playerCameraTarget.transform;
                }
            }
            
            // 一時的に確保したメモリを解放
            playerEntities.Dispose();
        }
    }
    
    // カメラが存在しない場合に自動生成するメソッド
    private void CreateCinemachineCamera()
    {
        var cameraObject = new GameObject("CM Virtual Camera");
        virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
        
        // カメラの基本設定
        var freeLook = cameraObject.AddComponent<CinemachineVirtualCamera>();
        freeLook.m_Lens.FieldOfView = 60f;
        
        // 必要に応じてカメラの設定を追加
        Debug.Log("CinemachineCameraを自動生成しました");
    }
}
