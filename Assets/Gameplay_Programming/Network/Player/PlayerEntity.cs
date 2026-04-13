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
[RequireComponent(typeof(PlayerBoardComponent))]
public class PlayerEntity : NetworkBehaviour
{
    public PlayerInputComponent InputsComponent { get; private set; }
    public PlayerInteractComponent InteractComponent { get; private set; }
    public PlayerHandComponent HandComponent { get; private set; }
    public PlayerBoardComponent BoardComponent { get; private set; }

    [Header("Player Parameters")]
    [SerializeField] PlayerEnum player;

    public PlayerEnum PlayerTag => player;

    private void Awake()
    {
        InputsComponent = GetComponent<PlayerInputComponent>();
        InteractComponent = GetComponent<PlayerInteractComponent>();
        HandComponent = GetComponent<PlayerHandComponent>();
        BoardComponent = GetComponent<PlayerBoardComponent>();
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
        HandComponent.Init();
        GameManager.Instance.debugWidget.SetDebugText($"Init for {player.ToString()} is done");

        SpawnBoard_ServerRpc();
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

    [ServerRpc]
    void SpawnBoard_ServerRpc()
    {
        BoardComponent.CreateBoard(player);
    }
}