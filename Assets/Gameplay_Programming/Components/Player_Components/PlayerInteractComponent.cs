using Unity.Netcode;
using UnityEngine;

public class PlayerInteractComponent : NetworkBehaviour
{
    Ray PointOnScreen => Camera.main.ScreenPointToRay(Input.mousePosition);
    [SerializeField] PlayerEnum ownerTag;

    private void Start()
    {
        if (IsOwner)
            InvokeRepeating(nameof(OnHoverUpdate), 0.1f, 0.1f);
    }

    public void SetOwnerTag(PlayerEnum _value) => ownerTag = _value;

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
                    {
                        if (_card != _hand.GetSelectedCard())
                            HoverCardHand_ServerRpc(_hand.GetIndexOf(_card));
                    }
                }
            }
            else
                UnhoverCardHand_ServerRpc();

            if (_hit.collider.gameObject.GetComponent<BoardCardComponent>() is BoardCardComponent _boardCard)
            {
                HoverCardBoard_ServerRpc(GameManager.Instance.Board.GetSlotIndex(_boardCard, ownerTag), ownerTag);
            }
            else
                UnhoverCardBoard_ServerRpc(ownerTag);
        }
    }

    public void OnPlayerClick()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (_hit.collider.gameObject.GetComponent<HandCardComponent>() is HandCardComponent _handCard)
            {
                PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
                if (_hand)
                {
                    OnSelectCardHand_ServerRpc(_hand.GetIndexOf(_handCard));
                }
            }
            if (_hit.collider.gameObject.GetComponent<BoardCardComponent>() is BoardCardComponent _boardCard)
            {
                if (_boardCard)
                {
                    OnSelectCardBoard_ServerRpc(GameManager.Instance.Board.GetSlotIndex(_boardCard, ownerTag), ownerTag);
                }
            }
        }
    }

    public void OnPlayerRelease()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            if (ownerTag != GameManager.Instance.PlayerTurnTag)
                OnReleaseCards_ServerRpc(ownerTag);

            if (_hit.collider.gameObject.GetComponent<BoardSlotComponent>() is BoardSlotComponent _slot)
            {
                if (_slot.PlayerTag == ownerTag)
                {
                    if (_slot.IsEmpty)
                    {
                        PlayerEntity _player = GameManager.Instance.GetPlayerFromTurn();
                        HandCardComponent _card = _player.HandComponent.GetSelectedCard();

                        if (_card && CardManager.Instance.IsSoldierID(_card.ID))
                        {
                            PutCardOnBoard_ServerRpc(ownerTag, _slot.GetSlotIndex);
                            return;
                        }
                    }
                }
            }
            if (_hit.collider.gameObject.GetComponent<BoardCardComponent>() is BoardCardComponent _boardCard)
            {
                if (GameManager.Instance.Board.GetOwnerOfCard(_boardCard) != ownerTag)
                {
                    BoardCardComponent _selectedCard = GameManager.Instance.Board.GetSelectedCard(ownerTag);
                    if (_selectedCard && !_selectedCard.CanAttack)
                    {
                        GameManager.Instance.debugWidget.SetDebugText("Attack");
                    }
                }
            }
            OnReleaseCards_ServerRpc(ownerTag);
        }
    }

    #region Hand
    [ServerRpc]
    void HoverCardHand_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SetHoveredCard(_id);
    }

    [ServerRpc]
    void UnhoverCardHand_ServerRpc()
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.UnhoverCard();
    }

    [ServerRpc]
    void OnSelectCardHand_ServerRpc(int _id)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.SelectCard(_id);
    }

    [ServerRpc]
    void OnReleaseCards_ServerRpc(PlayerEnum _playerTag)
    {
        PlayerHandComponent _hand = GetComponent<PlayerHandComponent>();
        _hand.ReleaseCard();

        GameManager.Instance.Board.ReleaseCards(_playerTag);
    }
    #endregion

    #region Board
    [ServerRpc]
    void PutCardOnBoard_ServerRpc(PlayerEnum _ownerType, int _index)
    {
        GameManager.Instance.PutCardOnBoard(_ownerType, _index);
    }

    [ServerRpc]
    void HoverCardBoard_ServerRpc(int _id, PlayerEnum _tag)
    {
        GameManager.Instance.Board.HoverCard(_id, _tag);
    }

    [ServerRpc]
    void UnhoverCardBoard_ServerRpc(PlayerEnum _tag)
    {
        GameManager.Instance.Board.UnhoverCards(_tag);
    }

    [ServerRpc]
    void OnSelectCardBoard_ServerRpc(int _id, PlayerEnum _tag)
    {
        GameManager.Instance.Board.SelectCard(_id, _tag);
    }
    #endregion

}
