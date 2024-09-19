using UnityEngine;
using Leopotam.EcsLite;
using UnityEngine.UIElements;
public class ECSSystemController : MonoBehaviour
{
    EcsWorld _world;
    EcsSystems _systems;

    void Start() {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        _systems
            .Add(new EntitySpawnSystem())
            .Add(new InputSystem())
            .Add(new CameraControllSystem())
            .Add(new ShootingSystem())
            .Add(new HealthSystem())
            .Add(new NPCAISystem())
            //.Add(new NetworkSystem())
            //.Add(new PlayerSynchronizationSystem())
            .Init();
    }

    void Update() {
        _systems?.Run();
    }

    void OnDestroy() {
        _systems?.Destroy();
        _world?.Destroy();
    }
    public EcsSystems GetEcsSystems()
    {
        return _systems;
    }
}
