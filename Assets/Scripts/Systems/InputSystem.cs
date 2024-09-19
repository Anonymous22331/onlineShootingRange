using UnityEngine;
using Leopotam.EcsLite;

public class InputSystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        
        var playerPool = world.GetPool<PlayerComponent>();
        var transformPool = world.GetPool<TransformComponent>();
        var cameraPool = world.GetPool<CameraComponent>();

        foreach (var entity in world.Filter<PlayerComponent>().End()) {
            ref var moveSpeed = ref playerPool.Get(entity).MoveSpeed;
            ref var playerTransform = ref transformPool.Get(entity).Transform;
            ref var CameraData = ref cameraPool.Get(entity);   

            var inputX = Input.GetAxis("Horizontal");
            var inputZ = Input.GetAxis("Vertical");
            playerTransform.rotation = Quaternion.Euler(0,CameraData.MainCamera.transform.eulerAngles.y,0);

            Vector3 direction = playerTransform.transform.forward * inputZ + playerTransform.transform.right * inputX;
            playerTransform.position += moveSpeed * Time.deltaTime * direction;
            
        }
    }

}
