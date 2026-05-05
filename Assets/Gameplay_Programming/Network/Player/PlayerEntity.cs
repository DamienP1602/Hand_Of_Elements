using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
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

    public PlayerEnum PlayerTag => player;

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

    void InitInputs()
    {
        InputsComponent.Click.started += (_context) => InteractComponent.OnPlayerClick();
        InputsComponent.Click.canceled += (_context) => InteractComponent.OnPlayerRelease();
    }

    public void Init()
    {
        player = OwnerClientId == 0 ? PlayerEnum.Player_One : PlayerEnum.Player_Two;

        if (!IsOwner) return;

        InteractComponent.SetOwnerTag(player);
        InitInputs();
        GameManager.Instance.SetButtonVisibleFromPlayerTurn(player);
    }

    /// <summary>
    /// Called by the UI "End Turn Button"
    /// </summary>
    public void ChangeTurn()
    {
        ChangeTurn_ServerRPC();
    }

    [ServerRpc]
    void ChangeTurn_ServerRPC()
    {
        GameManager.Instance.ChangeTurn();
    }
}