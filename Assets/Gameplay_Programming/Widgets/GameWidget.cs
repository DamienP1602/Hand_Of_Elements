using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameWidget : MonoBehaviour
{
    [SerializeField] Button endTurnButton;

    void Start()
    {
        endTurnButton.onClick.AddListener(ChangeTurn);
    }

    void Update()
    {

    }

    #region Functions

    void ChangeTurn()
    {
        PlayerEntity _player = GameManager.Instance.GetLocalPlayer();
        _player.ChangeTurn();
    }

    public void SetButtonIsVisible(bool _value)
    {
        endTurnButton.gameObject.SetActive(_value);
    }

    #endregion
}
