using UnityEngine;
using Leopotam.EcsLite;
using Unity.VisualScripting;

public class HealthSystem : IEcsRunSystem
{
    private NetworkConnection _networkConnection;
    private NetworkGameManager _networkManager;
    public async void Run(IEcsSystems systems)
    {
        if (_networkConnection.IsUnityNull())
        {
            var networkObject = GameObject.Find("NetCodeService");
            _networkConnection = networkObject.GetComponent<NetworkConnection>();
            _networkManager = networkObject.GetComponent<NetworkGameManager>();
        }
        var world = systems.GetWorld();
        var healthPool = world.GetPool<HealthComponent>();
        var enemyPool = world.GetPool<EnemyComponent>();
        var playerPool = world.GetPool<PlayerComponent>();
        var weaponPool = world.GetPool<WeaponComponent>();
        var playerUIPool = world.GetPool<PlayerUIComponent>();

        foreach (var entity in world.Filter<HealthComponent>().End())
        {
            var healthComponent = healthPool.Get(entity);
            if (healthComponent.Health <= 0 )
            {
                if (enemyPool.Has(entity))
                {
                    var enemyComponent = enemyPool.Get(entity);
                    var state = System.Text.Encoding.UTF8.GetBytes(enemyComponent.EnemyIndex.ToString());
                    await _networkConnection._socket.SendMatchStateAsync(_networkManager._match.Id, 2, state);
                    GameObject.Destroy(enemyComponent.EnemyObject);
                    enemyPool.Del(entity);
                }
                if (playerPool.Has(entity))
                {
                    var weaponComponent = weaponPool.Get(entity);
                    weaponComponent.CurrentShotCooldown = 5f;
                    healthComponent.Health = 100;
                }
            }
            if (playerPool.Has(entity))
            {
                var playerUIComponent = playerUIPool.Get(entity);
                playerUIComponent.HealthSlider.value = healthComponent.Health;
            }
        } 
    }
}
