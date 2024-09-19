using Leopotam.EcsLite;
using UnityEngine;

public class NPCAISystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var enemyPool = world.GetPool<EnemyComponent>();
        var weaponPool = world.GetPool<WeaponComponent>();
        foreach (var enemy in world.Filter<EnemyComponent>().End())
        {
            ref var enemyComponent = ref enemyPool.Get(enemy);
            if (enemyComponent.EnemyType == 1)
            {
                ref var weaponComponent = ref weaponPool.Get(enemy);
                int closestPlayerEntity = -1;
                float closestDistance = Mathf.Infinity;
                closestPlayerEntity = FindClosestPlayer(world, enemy, closestPlayerEntity, closestDistance);

                if (closestPlayerEntity != -1)
                {
                    weaponComponent.CurrentShotCooldown += Time.deltaTime;

                    if (weaponComponent.CurrentShotCooldown >= weaponComponent.ShotCooldown)
                    {
                        var healthPool = world.GetPool<HealthComponent>();
                        ref var healthComponent = ref healthPool.Get(closestPlayerEntity);
                        healthComponent.Health -= weaponComponent.Damage;
                        weaponComponent.CurrentShotCooldown = 0;
                    }
                }
            }
        }
    }

    private int FindClosestPlayer(EcsWorld world, int enemy, int closestPlayerEntity, float closestDistance)
    {
        int localClosestPlayerEntity = closestPlayerEntity;
        var transformPool = world.GetPool<TransformComponent>();
        foreach (var player in world.Filter<PlayerComponent>().End())
        {       
            ref var playerTransformComponent = ref transformPool.Get(player);
            ref var enemyTransformComponent = ref transformPool.Get(enemy);
            float distance = Vector3.Distance(playerTransformComponent.Transform.position, enemyTransformComponent.Transform.position);

            if (distance < closestDistance)
            {
                localClosestPlayerEntity = player;
                closestDistance = distance;
            }
        }
        foreach (var playerSync in world.Filter<PlayerSyncComponent>().End())
        {
                ref var playerSyncTransformComponent = ref transformPool.Get(playerSync);
                ref var enemyTransformComponent = ref transformPool.Get(enemy);
                
                float distance = Vector3.Distance(playerSyncTransformComponent.Transform.position, enemyTransformComponent.Transform.position);

                if (distance < closestDistance)
                {
                    localClosestPlayerEntity = playerSync;
                    closestDistance = distance;
                }
        }
        return localClosestPlayerEntity;
    }
}
