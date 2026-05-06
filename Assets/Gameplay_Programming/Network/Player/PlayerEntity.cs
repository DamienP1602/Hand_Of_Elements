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

    #region Getters

    public PlayerEnum PlayerTag => player;

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

        if (!IsOwner) return;

        InteractComponent.SetOwnerTag(player);
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
    /// Called by a Card when it's destroyed and will call his owner
    /// </summary>
    public void DestroyCard(int _cardToDestroyID)
    {
        DestroyCard_ServerRpc(_cardToDestroyID);
    }

    #endregion

    #region ServerRpc

    [ServerRpc]
    void ChangeTurn_ServerRPC()
    {
        GameManager.Instance.ChangeTurn();
    }

    [ServerRpc]
    void DestroyCard_ServerRpc(int _cardToDestroyID)
    {
        GameManager.Instance.DestroyCard(_cardToDestroyID);
    }

    #endregion
}