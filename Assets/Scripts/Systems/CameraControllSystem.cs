using Leopotam.EcsLite;
using UnityEngine;

public class CameraControllSystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        var playerComponent = world.GetPool<PlayerComponent>();
        var transformComponent = world.GetPool<TransformComponent>();
        var CameraComponent = world.GetPool<CameraComponent>();

        foreach (var entity in world.Filter<PlayerComponent>().End()) {
            
            ref var playerTransform = ref transformComponent.Get(entity).Transform;
            ref var rotateSpeed = ref playerComponent.Get(entity).RotateSpeed;
            ref var CameraData = ref CameraComponent.Get(entity);

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            CameraData.MainCamera.transform.position = playerTransform.transform.position + Vector3.up/4;

            CameraData.VerticalRotation -= mouseY * rotateSpeed;
            CameraData.VerticalRotation = Mathf.Clamp(CameraData.VerticalRotation, -25f, 45f);
            CameraData.HorizontalRotation += mouseX * rotateSpeed;

            CameraData.MainCamera.transform.rotation = Quaternion.Euler(CameraData.VerticalRotation, CameraData.HorizontalRotation, 0);
        } 
    }
}
