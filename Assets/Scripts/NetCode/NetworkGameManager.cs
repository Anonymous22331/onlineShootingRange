using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leopotam.EcsLite;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour
{
    [field: SerializeField] public NetworkConnection _networkConnection{get; private set;}
    public IMatch _match{get; private set;}
    struct MatchIdDeserialize
{
    public string match_id;
}
    [field: SerializeField] public ECSSystemController _systemsController{get; private set;}
    private IEcsSystems _systems;
    private bool _isSystemSet = false;
    private List<int> _playerIds = new List<int>();
    private Queue<System.Action> TODO = new Queue<System.Action>();

        /// <summary>
        /// Op.Codes: 0 for players, 1 for enemies creation, 2 for enemies update 10 for id update
        /// var response = await _networkConnection._socket.RpcAsync("enemyAdd_rpc");
        /// Debug.Log(response);
        ///
        /// response = await _networkConnection._socket.RpcAsync("enemyRequest_rpc");
        /// Debug.Log(response);
        ///
        /// response = await _networkConnection._socket.RpcAsync("enemyRemove_rpc","1");
        /// Debug.Log(response);
        ///
        /// response = await _networkConnection._socket.RpcAsync("enemyRequest_rpc");
        /// Debug.Log(response);
        /// <summary>

    async void Start()
    {
        await _networkConnection.ConnectAsync();
        Debug.Log("Connected to server");
        _networkConnection._socket.ReceivedMatchState += OnReceivedMatchState;

        var respond = await _networkConnection._socket.RpcAsync("create_match");
        var match_id = JsonConvert.DeserializeObject<MatchIdDeserialize>(respond.Payload).match_id;
        
        _match = await _networkConnection._socket.JoinMatchAsync(match_id);


        Debug.Log(match_id);

        if(_networkConnection._socket != null && _match != null)
        {
            InvokeRepeating("SendPlayerData", 0f, 0.1f);
        }
    }

    private void Update()
    {

        if (!_isSystemSet)
        {
            _systems = _systemsController.GetEcsSystems();
            _isSystemSet = true;
        }

        while( TODO.Count > 0)
        {
            TODO.Dequeue()();
        }
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        //Debug.Log("Received");
        var state = matchState.State.Length > 0 ? System.Text.Encoding.UTF8.GetString(matchState.State) : null;
        switch (matchState.OpCode)
        {
        case 0:
            //Debug.Log("EventCalled");
            var playerData = JsonConvert.DeserializeObject<List<NetworkData>>(state);
            UpdateSyncPlayer(playerData);
            break;
        case 1:
            var enemiesData = JsonConvert.DeserializeObject<List<EnemyNetworkData>>(state);
            CreateEnemies(enemiesData);
            break;
        case 2:
            Debug.Log("Received");
            RemoveEnemy(state);
            break;
        case 10:
        try
        {
            var newPlayerData = JsonConvert.DeserializeObject<NetworkData>(state);
            var world = _systems.GetWorld();
            var playerPool = world.GetPool<PlayerComponent>();
            foreach(var player in world.Filter<PlayerComponent>().End())
            {
                ref var playerComponent = ref playerPool.Get(player);
                if (playerComponent.Id != -1)
                {
                    break;
                }
                else 
                {
                    playerComponent.Id = newPlayerData.Id;
                }
            }
        }
        catch
        {
           _isSystemSet = false;
        }
            break;
        default:
            Debug.Log(state);
            break;
        }
    }

    private void SendPlayerData()
    {
        var world = _systems.GetWorld();
        var playerPool = world.GetPool<PlayerComponent>();
        var transformPool = world.GetPool<TransformComponent>();
        foreach (var entity in world.Filter<PlayerComponent>().End())
        {
            ref var playerComponent = ref playerPool.Get(entity);
            ref var transformComponent = ref transformPool.Get(entity);

                if (_networkConnection._socket.IsConnected)
                {
                    var data = new NetworkData
                    {
                    Id = playerComponent.Id ,
                    Position = new System.Numerics.Vector3 (transformComponent.Transform.position.x, transformComponent.Transform.position.y, transformComponent.Transform.position.z ),
                    Rotation = new System.Numerics.Quaternion( transformComponent.Transform.rotation.x, transformComponent.Transform.rotation.y, transformComponent.Transform.rotation.z, transformComponent.Transform.rotation.w)
                    };
                    string dataJson = JsonConvert.SerializeObject(data);
                    SendPlayerDataAsync(_networkConnection._socket, _match.Id, dataJson);
                }
            }
        }

    private async void SendPlayerDataAsync(ISocket socket, string matchId, string data)
    {
        //Debug.Log("Data send");
        await Task.Delay(TimeSpan.FromSeconds(0.25));
        await socket.SendMatchStateAsync(matchId, 0, data);
    }

    private void UpdateSyncPlayer(List<NetworkData> playersData)
    {
        var world = _systems.GetWorld();
        var transformPool = world.GetPool<TransformComponent>();

        //Debug.Log("DataRecieved");

        var playerSyncPool = world.GetPool<PlayerSyncComponent>();
        var playerPool = world.GetPool<PlayerComponent>();
        foreach(var playerData in playersData)
        {
            foreach(var player in world.Filter<PlayerComponent>().End())
            {
                ref var playerComponent = ref playerPool.Get(player);
                if (playerComponent.Id == playerData.Id) 
                {
                    continue;
                }
                if (_playerIds.Contains(playerData.Id))
                {
                    //Debug.Log(playerData.Position + " " + playerData.Rotation);
                    foreach (var syncPlayer in world.Filter<PlayerSyncComponent>().End())
                    {

                        ref var playerSyncComponent = ref playerSyncPool.Get(syncPlayer);
                        if (transformPool.Has(syncPlayer)){
                            var transformComponent = transformPool.Get(syncPlayer);

                            if (playerSyncComponent.Id == playerData.Id)
                            {
                                lock(TODO)
                                {
                                    TODO.Enqueue( () => 
                                    {
                                        //Debug.Log("Data set");
                                        transformComponent.Transform.SetPositionAndRotation(new Vector3(playerData.Position.X,playerData.Position.Y,playerData.Position.Z), new Quaternion(playerData.Rotation.X, playerData.Rotation.Y, playerData.Rotation.Z, playerData.Rotation.W));
                                    });
                                }
                            }
                        }
                        else 
                        {
                            Debug.Log("Synchronization failed");
                        }
                    }
                }
                else 
                {
                    Debug.Log("NewPlayerAdded");
                    var newPlayer = world.NewEntity();
                    ref var playerSyncComponent = ref playerSyncPool.Add(newPlayer);
                    playerSyncComponent.Id = playerData.Id;
                    _playerIds.Add(playerData.Id);
                }
            }
    }
    }
    private void CreateEnemies(List<EnemyNetworkData> enemiesData)
    {
        var world = _systems.GetWorld();
        var enemyPool = world.GetPool<EnemyComponent>();
        var healthPool = world.GetPool<HealthComponent>();
        var weaponPool = world.GetPool<WeaponComponent>();
        foreach(var enemyData in enemiesData)
        {
            var newEnemy = world.NewEntity();
            ref var enemyComponent = ref enemyPool.Add(newEnemy);
            ref var healthComponent = ref healthPool.Add(newEnemy);
            ref var weaponComponent = ref weaponPool.Add(newEnemy);

            enemyComponent.EnemyType = enemyData.Type;
            enemyComponent.EnemyIndex = enemyData.Id;
            enemyComponent.EnemyObject = null;
            healthComponent.Health = enemyData.Health;
            weaponComponent.Damage = 25;
            weaponComponent.CurrentShotCooldown = 0;
            weaponComponent.ShotCooldown = 5;
            enemyComponent.Position = new Vector3(enemyData.Position.X, enemyData.Position.Y,enemyData.Position.Z);

        }
    }
    private void RemoveEnemy(string enemyIndex)
    {
        var world = _systems.GetWorld();
        var enemyPool = world.GetPool<EnemyComponent>();
        Debug.Log("Enemy event");
        foreach (var enemy in world.Filter<EnemyComponent>().End())
        {
            var enemyComponent = enemyPool.Get(enemy);
            if (enemyComponent.EnemyIndex == Convert.ToInt32(enemyIndex))
            {
                Debug.Log("Enemy deleted");
                 TODO.Enqueue( () => 
                {
                    GameObject.Destroy(enemyComponent.EnemyObject);
                });
                enemyPool.Del(enemy);
            }
        }
    }
}
