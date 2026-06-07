using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerEnum
{
    Player_One,
    Player_Two,
    Player_NONE
}

[RequireComponent(typeof(PlayerInputComponent), typeof(PlayerInteractComponent), typeof(PlayerHandComponent))]
[RequireComponent(typeof(PlayerDeckComponent))]
public class PlayerEntity : NetworkBehaviour
{
    public PlayerInputComponent InputsComponent { get; private set; }
    public PlayerInteractComponent InteractComponent { get; private set; }
    public PlayerHandComponent HandComponent { get; private set; }
    public PlayerDeckComponent DeckComponent { get; private set; }
    public PlayerInterfaceComponent InterfaceComponent { get; private set; }
    public PlayerPortraitComponent PortraitComponent { get; private set; }

    [Header("Player Parameters")]
    [SerializeField] PlayerEnum player;

    [Header("Network Variables")]
    [SerializeField] NetworkVariable<int> arcaneAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<CardElement> lastElementPlayed = new NetworkVariable<CardElement>(CardElement.NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<List<int>> vfxIndexCreated = new NetworkVariable<List<int>>(new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] NetworkVariable<int> healthAmount = new NetworkVariable<int>(400, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    #region Getters

    public PlayerEnum PlayerTag => player;
    public int ArcaneAmount => arcaneAmount.Value;
    public CardElement LastElementPlayed => lastElementPlayed.Value;

    #endregion

    private void Awake()
    {
        InputsComponent = GetComponent<PlayerInputComponent>();
        InteractComponent = GetComponent<PlayerInteractComponent>();
        HandComponent = GetComponent<PlayerHandComponent>();
        DeckComponent = GetComponent<PlayerDeckComponent>();
        InterfaceComponent = GetComponentInChildren<PlayerInterfaceComponent>(true);
        PortraitComponent = GetComponentInChildren<PlayerPortraitComponent>();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    #region Inits

    public void Init()
    {
        player = OwnerClientId == 0 ? PlayerEnum.Player_One : PlayerEnum.Player_Two;
        InteractComponent.SetOwnerTag(player);
        healthAmount.OnValueChanged += (_old, _new) => PortraitComponent.SetHealthAmount(_new);

        if (!IsOwner) return;

        DrawInitCards_ServerRpc();

        InteractComponent.SetOwner(this);
        InterfaceComponent.gameObject.SetActive(true);
        arcaneAmount.OnValueChanged += (_old, _new) => InterfaceComponent.SetArcaneText(_new);

        InitInputs();
        GameManager.Instance.SetButtonVisibleFromPlayerTurn(player);

        if (player == PlayerEnum.Player_One)
            ChangeTurn_ServerRPC();
    }

    void InitInputs()
    {
        InputsComponent.Click.started += (_context) => InteractComponent.OnPlayerClick();
        InputsComponent.Click.canceled += (_context) => InteractComponent.OnPlayerRelease();
    }

    #endregion

    #region ByPass Functions

    /// <summary>
    /// Called by the UI "End Turn Button"
    /// </summary>
    public void ChangeTurn()
    {
        ChangeTurn_ServerRPC();
    }

    /// <summary>
    /// Called by a Card when it's destroyed
    /// </summary>
    public void DestroyCard(int _cardToDestroyID,PlayerEnum _ownerTag)
    {
        DestroyCard_ServerRpc(_cardToDestroyID, _ownerTag);
    }

    /// <summary>
    /// Called by a Card when it's been played
    /// </summary>
    public void InitCard(int _ID, PlayerEnum _owner)
    {
        //GameManager.Instance.debugWidget.SetDebugText("Set card :" + _ID.ToString() + " to : " + _owner.ToString());
        InitCard_ServerRpc(_ID, _owner);
    }

    #endregion

    #region ServerRpc

    [ServerRpc]
    void ChangeTurn_ServerRPC()
    {
        GameManager.Instance.ChangeTurn();
    }

    [ServerRpc]
    void DestroyCard_ServerRpc(int _cardToDestroyID, PlayerEnum _ownerTag)
    {
        GameManager.Instance.DestroyCard(_cardToDestroyID,_ownerTag);
    }

    [ServerRpc]
    void InitCard_ServerRpc(int _cardToInit,PlayerEnum _owner)
    {
        GameManager.Instance.InitCard(_cardToInit,_owner);
    }

    [ServerRpc]
    void DrawInitCards_ServerRpc()
    {
        GameManager.Instance.DrawFirstCards(this);
    }

    #endregion

    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void AddArcane(int _amount) => arcaneAmount.Value += _amount;

    /// <summary>
    /// Server Function
    /// </summary>
    public void RemoveArcane(int _amount) => arcaneAmount.Value -= _amount;

    /// <summary>
    /// Server Function
    /// </summary>
    public void SetArcaneAmount(int _amount) => arcaneAmount.Value = _amount;

    /// <summary>
    /// Server Function
    /// </summary>
    public void SetElementCardPlayed(CardElement _element) => lastElementPlayed.Value = _element;

    /// <summary>
    /// Server Function
    /// </summary>
    public void AddNewVfxIndex(int _index) => vfxIndexCreated.Value.Add(_index);

    /// <summary>
    /// Server Function
    /// </summary>
    public void LoseHealth(int _amount)
    {
        int _newAmount = healthAmount.Value - _amount;
        _newAmount = Mathf.Clamp(_newAmount, 0, 400);
        healthAmount.Value = _newAmount;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void RestaureHealth(int _amount)
    {
        int _newAmount = healthAmount.Value + _amount;
        _newAmount = Mathf.Clamp(_newAmount, 0, 400);
        healthAmount.Value = _newAmount;
    }

    #endregion
}