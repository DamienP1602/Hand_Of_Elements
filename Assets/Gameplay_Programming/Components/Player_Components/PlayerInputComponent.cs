using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputComponent : MonoBehaviour
{
    IAA_Player inputs;

    #region Getters

    public InputAction Click { get; private set; }

    #endregion

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
