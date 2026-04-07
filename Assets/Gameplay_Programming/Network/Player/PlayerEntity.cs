using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerEnum
{
    Player_One,
    Player_Two
}

[RequireComponent(typeof(PlayerInputComponent), typeof(PlayerInteractComponent),typeof(PlayerHandComponent))]
public class PlayerEntity : NetworkBehaviour
{
    public PlayerInputComponent InputsComponent { get; private set; }
    public PlayerInteractComponent InteractComponent { get; private set; }
    public PlayerHandComponent HandComponent { get; private set; }

    [Header("Parameters")]
    [SerializeField] Camera playerCamera;
    [SerializeField] PlayerEnum player;

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
    }

    public void Init()
    {
        if (!IsOwner) return;

        InitCamera();
        InitInputs();

        HandComponent.Init();
    }

    void InitCamera()
    {
        playerCamera = Camera.main;
    }
}
