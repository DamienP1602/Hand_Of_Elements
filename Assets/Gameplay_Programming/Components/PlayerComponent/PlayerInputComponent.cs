using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : MonoBehaviour
{
    IAA_Player inputs;

    public InputAction Click { get; private set; }

    private void Awake()
    {
        inputs = new IAA_Player();

        Click = inputs.Player.Click;
    }

    private void OnEnable()
    {
        Click.Enable();
    }

    private void OnDisable()
    {
        Click.Disable();
    }

}
