using System.Collections;
using System.Collections.Generic;
using Leopotam.EcsLite;
using Unity.VisualScripting;
using UnityEngine;

public class EntitySpawnSystem : IEcsInitSystem, IEcsRunSystem
{
    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        GameObjectPrefabsComponent prefabsComponent = GameObject.Find("ECSService").GetComponent<GameObjectPrefabsComponent>();

        var transformPool = world.GetPool<TransformComponent>();
        var rigidbodyPool = world.GetPool<RigidbodyComponent>();

        SpawnPlayer(world, prefabsComponent, transformPool, rigidbodyPool);
    }

    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var playerSyncPool = world.GetPool<PlayerSyncComponent>();
        var transformPool = world.GetPool<TransformComponent>();
        var rigidbodyPool = world.GetPool<RigidbodyComponent>();
        var enemyPool = world.GetPool<EnemyComponent>();
        GameObjectPrefabsComponent prefabsComponent = GameObject.Find("ECSService").GetComponent<GameObjectPrefabsComponent>();
        SpawnSyncPlayer(prefabsComponent, world, playerSyncPool, transformPool, rigidbodyPool);
        SpawnEnemy(prefabsComponent, world, enemyPool, transformPool);
    }

    private void SpawnEnemy(GameObjectPrefabsComponent prefabsComponent, EcsWorld world, 
    EcsPool<EnemyComponent> enemyPool, EcsPool<TransformComponent> trasformPool)
    {
        foreach (var entity in world.Filter<EnemyComponent>().End())
        {
            ref var enemyComponent = ref enemyPool.Get(entity);
            if (enemyComponent.EnemyObject.IsUnityNull())
            {
                //Debug.Log("Enemy Object created");
                //Debug.Log(prefabsComponent._enemyPrefab[enemyComponent.EnemyType].name);
                var newEnemyObject = Object.Instantiate(prefabsComponent._enemyPrefab[enemyComponent.EnemyType], enemyComponent.Position, Quaternion.identity);
                enemyComponent.EnemyObject = newEnemyObject;
                ref var transformComponent = ref trasformPool.Add(entity);
                transformComponent.Transform = newEnemyObject.transform;
            }
        }
    }
    private void SpawnSyncPlayer(GameObjectPrefabsComponent prefabsComponent, EcsWorld world, 
    EcsPool<PlayerSyncComponent> playerSyncPool, EcsPool<TransformComponent> transformPool, EcsPool<RigidbodyComponent> rigidbodyPool)
    {
        foreach (var entity in world.Filter<PlayerSyncComponent>().End())
        {
            ref var playerSyncComponent = ref playerSyncPool.Get(entity);
            if (playerSyncComponent.SyncGameObject.IsUnityNull())
            {
                ref var TransformComponent = ref transformPool.Add(entity);    
                ref var rigiddodyComponent = ref rigidbodyPool.Add(entity);

                var syncPlayerObject = Object.Instantiate(prefabsComponent._onlinePlayerPrefab, Vector3.zero, Quaternion.identity);
                playerSyncComponent.SyncGameObject = syncPlayerObject;
                rigiddodyComponent.Rigidbody = syncPlayerObject.GetComponent<Rigidbody>();
                TransformComponent.Transform = syncPlayerObject.transform;
            }
        }
    }

     private void SpawnPlayer(EcsWorld world, GameObjectPrefabsComponent prefabsComponent, 
     EcsPool<TransformComponent> transformPool, EcsPool<RigidbodyComponent> rigidbodyPool) 
     {
        var entity = world.NewEntity();

        var playerPool = world.GetPool<PlayerComponent>();
        var cameraPool = world.GetPool<CameraComponent>();
        var weaponPool = world.GetPool<WeaponComponent>();
        var playerUIPool = world.GetPool<PlayerUIComponent>();
        var healthPool = world.GetPool<HealthComponent>();
       
        ref var playerComponent = ref playerPool.Add(entity);
        ref var CameraComponent = ref cameraPool.Add(entity);
        ref var WeaponComponent = ref weaponPool.Add(entity);
        ref var playerUIComponent = ref playerUIPool.Add(entity);
        ref var healthComponent = ref healthPool.Add(entity);

        //Можно вынести в функцию
        ref var TransformComponent = ref transformPool.Add(entity);    
        ref var rigiddodyComponent = ref rigidbodyPool.Add(entity);

        var playerObject = Object.Instantiate(prefabsComponent._playerPrefab, Vector3.zero, Quaternion.identity);
        //Можно вынести в функцию
        rigiddodyComponent.Rigidbody = playerObject.GetComponent<Rigidbody>();
        TransformComponent.Transform = playerObject.transform;

        playerComponent.MoveSpeed = prefabsComponent._playerMoveSpeed;
        playerComponent.RotateSpeed = prefabsComponent._playerRotateSpeed;
        playerComponent.Id = -1;

        CameraComponent.MainCamera = Camera.main;
        CameraComponent.VerticalRotation = 0f;
        CameraComponent.HorizontalRotation = 0f;

        WeaponComponent.Damage = 25f;
        WeaponComponent.CurrrentAmmo = 7;
        WeaponComponent.MaxAmmo = 7;
        WeaponComponent.ShotCooldown = 2f;

        playerUIComponent.ScoreText = prefabsComponent._scoreText;
        playerUIComponent.AmmoText = prefabsComponent._ammoText;
        playerUIComponent.HealthSlider = prefabsComponent._healthBar;

        healthComponent.Health = 100;
    }
}
