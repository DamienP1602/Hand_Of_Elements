using Unity.Netcode;
using UnityEngine;

public class PlayerInteractComponent : NetworkBehaviour
{
    Ray PointOnScreen => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Start()
    {
        if (IsOwner)
            InvokeRepeating(nameof(OnHoverUpdate), 0.05f, 0.05f);
    }

    public void OnPlayerClick()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (_hit.collider.gameObject.GetComponent<HandCardComponent>() is HandCardComponent _card)
            {
                PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
                if (_hand)
                {
                    OnSelectCard_ServerRpc(_hand.Cards.IndexOf(_card));
                }
            }
        }
    }

    public void OnPlayerRelease()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
            if (_hand)
            {
                OnReleaseCard_ServerRpc();
            }
        }
    }

    void OnHoverUpdate()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (_hit.collider.gameObject.GetComponent<HandCardComponent>() is HandCardComponent _card)
            {
                PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
                if (_hand)
                {
                    if (_hand.Cards.Contains(_card))
                    {
                        HoverCard_ServerRpc(_hand.Cards.IndexOf(_card));
                    }
                }
            }
            else
            {
                UnhoverCard_ServerRpc();
            }
        }
    }

    [ServerRpc]
    void HoverCard_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SetHoveredCard_ClientRpc(_id);
    }

    [ServerRpc]
    void UnhoverCard_ServerRpc()
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.UnhoverCard_ClientRpc();
    }

    [ServerRpc]
    void OnSelectCard_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SetSelectedCard_ClientRpc(_id);
    }

    [ServerRpc]
    void OnReleaseCard_ServerRpc()
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.ReleaseCard_ClientRpc();
    }
}
