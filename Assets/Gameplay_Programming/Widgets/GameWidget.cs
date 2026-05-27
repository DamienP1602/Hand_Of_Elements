using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameWidget : MonoBehaviour
{
    [SerializeField] Button endTurnButton;
    [SerializeField] CardOverlayComponent visualCard;

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

    public void ShowVisualCard(CardComponent _card)
    {
        GameManager.Instance.debugWidget.SetDebugText(_card.Data ? "data" : "no Data");
        visualCard.SetData(_card.Data,true);
    }

    #endregion
}
