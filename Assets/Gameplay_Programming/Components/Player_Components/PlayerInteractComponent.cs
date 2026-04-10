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
        PlayerEnum _playerTurn = GameManager.Instance.PlayerTurnTag;
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (_hit.collider.gameObject.GetComponent<BoardSlotComponent>() is BoardSlotComponent _slot)
            {
                if (_slot.PlayerTag == _playerTurn && GetComponent<PlayerEntity>().PlayerTag == _playerTurn)
                {
                    if (_slot.IsEmpty)
                    {
                        PutCardOnBoard_ServerRpc(_playerTurn, _slot.SlotIndex);
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

    [ServerRpc]
    void PutCardOnBoard_ServerRpc(PlayerEnum _ownerTag, int _index)
    {
        BoardComponent _board = GameManager.Instance.Board;
        BoardSlotComponent _slot = _board.GetSlot(_ownerTag, _index);
        _slot.PutCardInSlot();

        PlayerEntity _player = GameManager.Instance.GetPlayer(_ownerTag);
        _player.HandComponent.RemoveSelectedCard();
    }
}
