using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Player Data")]
    [SerializeField] Vector3 firstPlayerPosition;
    [SerializeField] Vector3 secondPlayerPosition;
    [SerializeField] List<PlayerEntity> players;
    [SerializeField] GameWidget playerWidgetPrefab;

    [Header("Turn Data")]
    [SerializeField] PlayerEnum playerTurn;

    [Header("Board Data")]
    [SerializeField] BoardComponent board;

    [Header("Widget Data")]
    [SerializeField] GameWidget widget;

    public BoardComponent Board => board;
    public List<PlayerEntity> GetAllPlayers => players;
    public GameWidget PlayerWidget => playerWidgetPrefab;

    public PlayerEnum PlayerTurnTag => playerTurn;

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

    #region GetPlayer

    public PlayerEntity GetOtherPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag == _type);
    }

    public PlayerEntity GetPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag != _type);
    }

    public PlayerEntity GetPlayerFromTurn()
    {
        return players.Find(_player => _player.PlayerTag == playerTurn);
    }

    #endregion

    [ClientRpc]
    public void ChangeTurn_ClientRpc(PlayerEnum _enum) => playerTurn = _enum;

    public void ChangeTurn(PlayerEnum _enum) => playerTurn = _enum;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(firstPlayerPosition,1.0f);
        Gizmos.DrawWireSphere(secondPlayerPosition,1.0f);
    }
}
