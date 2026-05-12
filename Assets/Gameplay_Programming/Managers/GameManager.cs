using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
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
    [SerializeField] NetworkVariable<int> turnAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Board Data")]
    [SerializeField] BoardComponent board;

    [Header("Widget Data")]
    [SerializeField] GameWidget widget;

    [Header("Debug Data")]
    public DebugWidget debugWidget;

    #region Getters

    public BoardComponent Board => board;
    public List<PlayerEntity> GetAllPlayers => players;
    public GameWidget PlayerWidget => playerWidgetPrefab;

    public PlayerEnum PlayerTurnTag => playerTurn.Value;

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

    public PlayerEntity GetLocalPlayer()
    {
        NetworkObject _obj = NetworkManager.Singleton.LocalClient.PlayerObject;
        return _obj.GetComponent<PlayerEntity>();
    }

    public PlayerEnum GetOtherPlayerTag(PlayerEnum _value) => _value == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;

    #endregion

    void Start()
    {
        Invoke(nameof(InitPlayers_ClientRPC), Time.deltaTime);
    }

    void Update()
    {

    }

    #region ClientRpc

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

    [ClientRpc]
    void CheckButtonInteractable_ClientRpc()
    {
        PlayerEntity _player = GetLocalPlayer();
        SetButtonVisibleFromPlayerTurn(_player.PlayerTag);
    }

    #endregion

    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void ChangeTurn()
    {
        PlayerEnum _newTurn = playerTurn.Value == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;
        playerTurn.Value = _newTurn;

        if (_newTurn == PlayerEnum.Player_One)
            turnAmount.Value++;

        PlayerEntity _player = GetPlayerFromTurn();
        if (_player.DeckComponent.CardCount > 0)
        {
            _player.HandComponent.DrawCard(1);
            _player.HandComponent.SetCardInHand_ClientRpc();
        }
        _player.SetArcaneAmount(Mathf.Clamp(turnAmount.Value, 0, 10));

        widget.SetButtonIsVisible(false);
        Invoke(nameof(CheckButtonInteractable_ClientRpc), 0.1f);

        board.SetCardCanAttack(playerTurn.Value);

        foreach (PlayerEntity _entity in players)
        {
            _entity.InteractComponent.SetSelectCard(false);
            _entity.HandComponent.ReleaseCard();
            _entity.HandComponent.UnselectCardVisual_ClientRpc();
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void PutCardOnBoard(PlayerEnum _type, int _boardSlotIndex)
    {
        PlayerEntity _player = GetPlayer(_type);
        HandCardComponent _card = _player.HandComponent.GetSelectedCard();
        BoardSlotComponent _slot = board.GetSlot(_type, _boardSlotIndex);

        if (_player.ArcaneAmount < _card.Data.cardCost)
        {
            _player.InteractComponent.SetSelectCard(false);
            _player.HandComponent.ReleaseCard();
            _player.HandComponent.UnselectCardVisual_ClientRpc();
            return;
        }

        if (_card && _slot)
        {
            _player.RemoveArcane(_card.Data.cardCost);
            _slot.PutCardInSlot(_card.transform.position, _card.ID);
            _player.HandComponent.RemoveSelectedCard();
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void AttackCard(int _targetedCardID, PlayerEnum _ownerTag)
    {
        BoardCardComponent _selectedCard = board.GetSelectedCard(_ownerTag);
        BoardSlotComponent _targetedCard = board.GetCardFromCardID(GetOtherPlayerTag(_ownerTag), _targetedCardID);

        _selectedCard.AttackCard(_targetedCard.Card);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void DestroyCard(int _cardToDestroySlotID, PlayerEnum _ownerTag)
    {
        BoardSlotComponent _slot = board.GetSlot(_ownerTag, _cardToDestroySlotID);
        _slot.Card.NetworkObject.Despawn(true);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void InitCard(int _cardToInit)
    {
        PlayerEnum _cardOwner = board.GetOwnerOfCard(_cardToInit);
        BoardSlotComponent _slot = board.GetCardFromCardID(_cardOwner, _cardToInit);
        _slot.Card.InitStats();
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void DrawFirstCards(PlayerEntity _player)
    {
        _player.HandComponent.DrawCard(3);

        foreach (PlayerEntity _entity in players)
        {
            _entity.HandComponent.SetCardInHand_ClientRpc();
        }
    }

    #endregion

    #region Functions

    public void SetButtonVisibleFromPlayerTurn(PlayerEnum _player)
    {
        widget.SetButtonIsVisible(_player == playerTurn.Value);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(firstPlayerPosition, 1.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(secondPlayerPosition, 1.0f);
    }
}
