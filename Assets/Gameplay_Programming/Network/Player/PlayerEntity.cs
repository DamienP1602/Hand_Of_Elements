using Unity.Netcode;
using UnityEngine;

public class PlayerEntity : NetworkBehaviour
{
    public PlayerInputComponent InputsComponent { get; private set; }
    public PlayerClickComponent ClickComponent { get; private set; }

    [Header("Parameters")]
    [SerializeField] Camera playerCamera;

    private void Awake()
    {
        InputsComponent = GetComponent<PlayerInputComponent>();
        ClickComponent = GetComponent<PlayerClickComponent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitInputs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitCamera(Camera _camera,Vector3 _spawnPosition, bool _shouldRotate)
    {
        if (IsOwner)
        {
            playerCamera = Instantiate(_camera, transform);
            playerCamera.transform.position = _spawnPosition;
            if (_shouldRotate)
                playerCamera.transform.eulerAngles += Vector3.forward * 180.0f;
        }
    }

    void InitInputs()
    {
        InputsComponent.Click.started += (_context) => ClickComponent.OnPlayerClick();
    }
}
