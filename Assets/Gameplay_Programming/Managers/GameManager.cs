using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Player Data")]
    [SerializeField] Vector3 firstPlayerPosition;
    [SerializeField] Vector3 secondPlayerPosition;
    [SerializeField] List<PlayerEntity> players;


    public List<PlayerEntity> GetAllPlayers => players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitPlayers();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitPlayers()
    {
        players = FindObjectsByType<PlayerEntity>(FindObjectsSortMode.None).ToList();

        foreach (PlayerEntity _player in players)
        {
            _player.Init_ClientRpc();

            bool _isFirstPlayer = _player.PlayerTag == PlayerEnum.Player_One;

            Vector3 _position = _isFirstPlayer ? firstPlayerPosition : secondPlayerPosition;
            _player.transform.position = _position;

            if (!_isFirstPlayer)
                _player.RotateCamera_ClientRpc();
        }
    }

    public PlayerEntity GetOtherPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag == _type);
    }

    public PlayerEntity GetPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag != _type);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(firstPlayerPosition,1.0f);
        Gizmos.DrawWireSphere(secondPlayerPosition,1.0f);
    }
}
