using UnityEngine;

public class PlayerInterfaceComponent : MonoBehaviour
{
    [SerializeField] CardOverlayComponent visualCard;

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
}
