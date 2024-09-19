using UnityEngine;
using UnityEngine.UI;

public class GameObjectPrefabsComponent : MonoBehaviour
{
    [field: SerializeField] public GameObject _playerPrefab {get; private set;}
    [field: SerializeField] public float _playerMoveSpeed {get; private set;}
    [field: SerializeField] public float _playerRotateSpeed {get; private set;}
    [field: SerializeField] public GameObject _onlinePlayerPrefab {get; private set;}
    [field: SerializeField] public GameObject[] _enemyPrefab {get; private set;} = new GameObject[2];
    [field: SerializeField] public Text _scoreText {get; private set;}
    [field: SerializeField] public Text _ammoText {get; private set;}
    [field: SerializeField] public Slider _healthBar {get; private set;}
}
