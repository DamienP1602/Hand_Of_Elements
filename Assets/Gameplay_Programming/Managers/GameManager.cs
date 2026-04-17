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
    [SerializeField] NetworkVariable<PlayerEnum> playerTurn = new NetworkVariable<PlayerEnum>(PlayerEnum.Player_One, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Board Data")]
    [SerializeField] BoardComponent board;

    [Header("Widget Data")]
    [SerializeField] GameWidget widget;

    [Header("Debug Data")]
    public DebugWidget debugWidget;

    public BoardComponent Board => board;
    public List<PlayerEntity> GetAllPlayers => players;
    public GameWidget PlayerWidget => playerWidgetPrefab;

    public PlayerEnum PlayerTurnTag => playerTurn.Value;

    void Start()
    {
        Invoke(nameof(InitPlayers_ClientRPC), Time.deltaTime);
        playerTurn.OnValueChanged += CallChangeTurn;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CallChangeTurn(PlayerEnum _oldVal, PlayerEnum _newVal)
    {
        debugWidget.SetDebugText($"Change turn to {_newVal.ToString()}");
    }

    [ClientRpc]
    void InitPlayers_ClientRPC()
    {
        players = FindObjectsByType<PlayerEntity>(FindObjectsSortMode.None).ToList();

        foreach (PlayerEntity _player in players)
        {
            if (_player.IsOwner)
            {
                _player.transform.position = firstPlayerPosition;
            }
            else
            {
                _player.transform.position = secondPlayerPosition;
            }

            _player.Init();
            board.SetPlayerBoardSide(_player);
        }
    }

    #region GetPlayer

    public PlayerEntity GetOtherPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag != _type);
    }

    public PlayerEntity GetPlayer(PlayerEnum _type)
    {
        return players.Find(_player => _player.PlayerTag == _type);
    }

    public PlayerEntity GetPlayerFromTurn()
    {
        return players.Find(_player => _player.PlayerTag == playerTurn.Value);
    }

    #endregion

    public void ChangeTurn()
    {
        PlayerEnum _newTurn = playerTurn.Value == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;
        playerTurn.Value = _newTurn;

        PlayerEntity _player = GetPlayerFromTurn();
        if (_player.DeckComponent.CardCount == 0) return;

        _player.HandComponent.DrawCard(1);
        _player.HandComponent.SetCardInHand_ClientRpc();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(firstPlayerPosition, 1.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(secondPlayerPosition, 1.0f);
    }
}
