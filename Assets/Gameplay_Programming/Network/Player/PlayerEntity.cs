using Unity.Netcode;
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

    [ClientRpc]
    public void Init_ClientRpc()
    {
        player = OwnerClientId == 0 ? PlayerEnum.Player_One : PlayerEnum.Player_Two;

        if (!IsOwner) return;
        InitInputs();

        HandComponent.Init();
    }

    [ClientRpc]
    public void RotateCamera_ClientRpc()
    {
        Camera _camera = Camera.main;
        _camera.transform.rotation = Quaternion.Euler(90.0f, 180.0f, 0.0f);
    }
}
