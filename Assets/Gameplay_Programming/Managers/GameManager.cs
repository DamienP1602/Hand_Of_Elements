using System;
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
    }

    // Update is called once per frame
    void Update()
    {

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

    public PlayerEnum GetOtherPlayerTag(PlayerEnum _value) => _value == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;

    #endregion

    public void ChangeTurn()
    {
        PlayerEnum _newTurn = playerTurn.Value == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;
        playerTurn.Value = _newTurn;

        PlayerEntity _player = GetPlayerFromTurn();
        if (_player.DeckComponent.CardCount > 0)
        {
            _player.HandComponent.DrawCard(1);
            _player.HandComponent.SetCardInHand_ClientRpc();
        }

        widget.SetButtonIsVisible(false);
        Invoke(nameof(CheckButtonInteractable_ClientRpc), 0.1f);

        board.SetCardCanAttack(playerTurn.Value);
    }

    [ClientRpc]
    void CheckButtonInteractable_ClientRpc()
    {
        NetworkObject _obj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (_obj.GetComponent<PlayerEntity>() is PlayerEntity _player)
        {
            SetButtonVisibleFromPlayerTurn(_player.PlayerTag);
        }
    }

    public void SetButtonVisibleFromPlayerTurn(PlayerEnum _player)
    {
        widget.SetButtonIsVisible(_player == playerTurn.Value);
    }

    public void PutCardOnBoard(PlayerEnum _type, int _boardSlotIndex)
    {
        PlayerEntity _player = GetPlayer(_type);
        HandCardComponent _card = _player.HandComponent.GetSelectedCard();
        BoardSlotComponent _slot = board.GetSlot(_type, _boardSlotIndex);

        if (_card && _slot)
        {
            _slot.PutCardInSlot(_card.transform.position, _card.ID);
            _player.HandComponent.RemoveSelectedCard();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(firstPlayerPosition, 1.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(secondPlayerPosition, 1.0f);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void AttackCard(int _targetedCardID, PlayerEnum _ownerTag)
    {
        BoardCardComponent _selectedCard = board.GetSelectedCard(_ownerTag);
        BoardSlotComponent _targetedCard = board.GetCardFromID(GetOtherPlayerTag(_ownerTag), _targetedCardID);

        _selectedCard.AttackCard(_targetedCard.Card);
    }

    [ServerRpc]
    public void DestroyCard_ServerRpc(int _cardToDestroyID)
    {
        PlayerEnum _cardOwner = board.GetOwnerOfCard(_cardToDestroyID);
        BoardSlotComponent _slot = board.GetCardFromID(_cardOwner, _cardToDestroyID);
        _slot.Card.NetworkObject.Despawn(true);
    }
}
