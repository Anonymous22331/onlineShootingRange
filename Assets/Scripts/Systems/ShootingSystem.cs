using Leopotam.EcsLite;
using UnityEngine;

public class ShootingSystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        var weaponPool = world.GetPool<WeaponComponent>();
        var transformPool = world.GetPool<TransformComponent>();
        var cameraPool = world.GetPool<CameraComponent>();
        var playerUIPool = world.GetPool<PlayerUIComponent>();
        var playerPool = world.GetPool<PlayerComponent>();

        foreach (var entity in world.Filter<PlayerComponent>().End())
        {
            ref var weaponComponent = ref weaponPool.Get(entity);
            ref var transformComponent = ref transformPool.Get(entity);
            ref var cameraComponent = ref cameraPool.Get(entity);
            ref var playerUIComponent = ref playerUIPool.Get(entity);
            ref var playerComponent = ref playerPool.Get(entity);

            if (weaponComponent.CurrentShotCooldown < 0 && weaponComponent.CurrentShotCooldown < 0 && weaponComponent.CurrrentAmmo > 0) 
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {   
                    Shot(ref weaponComponent, ref cameraComponent, ref world, ref playerUIComponent, ref playerComponent);
                }
            }
            else 
            {
                weaponComponent.CurrentShotCooldown -= Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.R) && weaponComponent.CurrrentAmmo != weaponComponent.MaxAmmo)
            {
                Reload(ref weaponComponent, ref playerUIComponent);
            }
        }
    }

    private void Shot(ref WeaponComponent weaponComponent, ref CameraComponent cameraComponent, ref EcsWorld world, ref PlayerUIComponent playerUIComponent, ref PlayerComponent playerComponent)
    {
        weaponComponent.CurrentShotCooldown = weaponComponent.ShotCooldown;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = cameraComponent.MainCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f)) 
        {
            var enemyPool = world.GetPool<EnemyComponent>();
            var healthPool = world.GetPool<HealthComponent>();

            foreach (var targetEntity in world.Filter<EnemyComponent>().End()) 
            {
                ref var enemyComponent = ref enemyPool.Get(targetEntity);
                ref var HealthComponent = ref healthPool.Get(targetEntity);

                if (hitInfo.collider.gameObject == enemyPool.Get(targetEntity).EnemyObject.gameObject) 
                {
                    HealthComponent.Health -= weaponComponent.Damage;
                    if(HealthComponent.Health <= 0)
                    {
                        playerComponent.Score += 1;
                        playerUIComponent.ScoreText.text = "Score: " + playerComponent.Score; 
                    }
                    //hitInfo.rigidbody.AddForce(Vector3.forward * 10);
                    break;
                }
            }
        }
        weaponComponent.CurrrentAmmo -= 1;
        playerUIComponent.AmmoText.text = weaponComponent.CurrrentAmmo + "/" + weaponComponent.MaxAmmo;     
    }

    private void Reload(ref WeaponComponent weaponComponent, ref PlayerUIComponent playerUIComponent)
    {
        weaponComponent.CurrentShotCooldown = 5f;
        weaponComponent.CurrrentAmmo = weaponComponent.MaxAmmo;
        playerUIComponent.AmmoText.text = weaponComponent.CurrrentAmmo + "/" + weaponComponent.MaxAmmo;
    }
}
