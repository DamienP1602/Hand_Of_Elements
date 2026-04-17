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
                    OnSelectCard_ServerRpc(_hand.GetIndexOf(_card));
                }
            }
        }
    }

    public void OnPlayerRelease()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (_hit.collider.gameObject.GetComponent<BoardSlotComponent>() is BoardSlotComponent _slot)
            {
                PlayerEntity _player = GetComponent<PlayerEntity>();

                if (_slot.PlayerTag == _player.PlayerTag && _player.PlayerTag == GameManager.Instance.PlayerTurnTag)
                {
                    if (_slot.IsEmpty)
                    {
                        PutCardOnBoard_ServerRpc(_slot.GetSlotIndex);
                        return;
                    }
                }
            }
            OnReleaseCard_ServerRpc();
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
                    if (_hand.Contains(_card))
                        HoverCard_ServerRpc(_hand.GetIndexOf(_card));
                }
            }
            else
                UnhoverCard_ServerRpc();
        }
    }

    [ServerRpc]
    void HoverCard_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SetHoveredCard(_id);
    }

    [ServerRpc]
    void UnhoverCard_ServerRpc()
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.UnhoverCard();
    }

    [ServerRpc]
    void OnSelectCard_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SelectCard(_id);
    }

    [ServerRpc]
    void OnReleaseCard_ServerRpc()
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.ReleaseCard();
    }

    [ServerRpc]
    void PutCardOnBoard_ServerRpc(int _index)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayerFromTurn();
        HandCardComponent _card = _player.HandComponent.GetSelectedCard();
        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_player.PlayerTag, _index);

        if (_card && _slot)
        {
            _slot.PutCardInSlot();
            _player.HandComponent.RemoveSelectedCard();
        }
    }
}
