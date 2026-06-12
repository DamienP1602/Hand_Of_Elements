using TMPro;
using UnityEngine;

public class PlayerInterfaceComponent : MonoBehaviour
{
    [SerializeField] CardOverlayComponent visualCard;
    [SerializeField] TMP_Text arcaneAmount;

    public void ShowVisualCard(CardComponent _card)
    {
        visualCard.gameObject.SetActive(true);
        visualCard.SetData(_card.Data, true);

        Vector3 _cardPos = Camera.main.ViewportToScreenPoint(_card.transform.position) / 4.0f;
        visualCard.transform.position = _cardPos;
    }

    public void HideVisual()
    {
        visualCard.gameObject.SetActive(false);
    }

    public void SetArcaneText(int _amount, int _maxAmount)
    {
        int _turnAmount = GameManager.Instance.PlayerTurnCount;
        _turnAmount = Mathf.Clamp(_turnAmount, 0, 10);
        arcaneAmount.text = _amount.ToString() + "/" + _turnAmount.ToString();
    }
}
