using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public enum PlayerEnum
{
    Player_One,
    Player_Two
}

[RequireComponent(typeof(PlayerInputComponent), typeof(PlayerInteractComponent), typeof(PlayerHandComponent))]
public class PlayerEntity : NetworkBehaviour
{
    public PlayerInputComponent InputsComponent { get; private set; }
    public PlayerInteractComponent InteractComponent { get; private set; }
    public PlayerHandComponent HandComponent { get; private set; }

    [Header("Player Parameters")]
    [SerializeField] PlayerEnum player;

    public PlayerEnum PlayerTag => player;

    private void Awake()
    {
        InputsComponent = GetComponent<PlayerInputComponent>();
        InteractComponent = GetComponent<PlayerInteractComponent>();
        HandComponent = GetComponent<PlayerHandComponent>();
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

        InitInputs();
    }

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