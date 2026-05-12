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

    [Header("Player Parameters")]
    [SerializeField] PlayerEnum player;

    [Header("Network Variables")]
    [SerializeField] NetworkVariable<int> arcaneAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    #region Getters

    public PlayerEnum PlayerTag => player;
    public int ArcaneAmount => arcaneAmount.Value;

    #endregion

    private void Awake()
    {
        InputsComponent = GetComponent<PlayerInputComponent>();
        InteractComponent = GetComponent<PlayerInteractComponent>();
        HandComponent = GetComponent<PlayerHandComponent>();
        DeckComponent = GetComponent<PlayerDeckComponent>();
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

        if (!IsOwner) return;

        DrawInitCards_ServerRpc();

        InitInputs();
        GameManager.Instance.SetButtonVisibleFromPlayerTurn(player);
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
    public void InitCard(int _ID)
    {
        InitCard_ServerRpc(_ID);
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
    void InitCard_ServerRpc(int _cardToInit)
    {
        GameManager.Instance.InitCard(_cardToInit);
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

    #endregion
}