using Unity.Netcode;
using UnityEngine;

public class PlayerInteractComponent : NetworkBehaviour
{
    Ray PointOnScreen => Camera.main.ScreenPointToRay(Input.mousePosition);
    [SerializeField] PlayerEnum ownerTag;
    [SerializeField] bool needToSelectACard;

    private void Start()
    {
        if (IsOwner)
            InvokeRepeating(nameof(OnHoverUpdate), 0.1f, 0.1f);
    }

    public void SetOwnerTag(PlayerEnum _value) => ownerTag = _value;

    public void SetSelectCard(bool _value) => needToSelectACard = _value;

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
                if (needToSelectACard)
                {
                    PlayerEnum _ownerType = GameManager.Instance.Board.GetOwnerOfCard(_boardCard);
                    int _cardID = GameManager.Instance.Board.GetSlotIndex(_boardCard, _ownerType);
                    SelectCardForEffect_ServerRpc(_cardID,_ownerType);
                    return;
                }
                if (_boardCard)
                {
                    SelectCardBoard_ServerRpc(GameManager.Instance.Board.GetSlotIndex(_boardCard, ownerTag), ownerTag);
                }
            }
        }
    }

    public void OnPlayerRelease()
    {
        if (Physics.Raycast(PointOnScreen, out RaycastHit _hit, 15.0f))
        {
            #region Owner Verification
            if (ownerTag != GameManager.Instance.PlayerTurnTag)
                OnReleaseCards_ServerRpc(ownerTag);
            #endregion

            #region Variables
            PlayerEntity _player = GameManager.Instance.GetPlayerFromTurn();
            HandCardComponent _selectedCard = _player.HandComponent.GetSelectedCard();
            #endregion

            #region if Spell selected
            if (_selectedCard && _selectedCard.Data is SpellCardData _spell)
            {
                LaunchEffect_ServerRpc(_selectedCard.ID,ownerTag);
                return;
            }
            #endregion

            #region Put card on board
            if (_hit.collider.gameObject.GetComponent<BoardSlotComponent>() is BoardSlotComponent _slot)
            {
                if (_slot.PlayerTag == ownerTag)
                {
                    if (_slot.IsEmpty)
                    {
                        if (_selectedCard && CardManager.Instance.IsSoldierID(_selectedCard.ID))
                        {
                            PutCardOnBoard_ServerRpc(ownerTag, _slot.GetSlotIndex);
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Attack Card
            if (_hit.collider.gameObject.GetComponent<BoardCardComponent>() is BoardCardComponent _boardCard)
            {
                if (GameManager.Instance.Board.GetOwnerOfCard(_boardCard) != ownerTag)
                {
                    BoardCardComponent _boardCardSelected = GameManager.Instance.Board.GetSelectedCard(ownerTag);
                    if (_boardCardSelected && _boardCardSelected.CanAttack)
                    {
                        AttackCardBoard_ServerRpc(_boardCard.ID, ownerTag);
                    }
                }
            }
            #endregion

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
    void SelectCardBoard_ServerRpc(int _id, PlayerEnum _tag)
    {
        GameManager.Instance.Board.SelectCard(_id, _tag);
    }

    [ServerRpc]
    void AttackCardBoard_ServerRpc(int _targetedCard, PlayerEnum _ownerTag)
    {
        GameManager.Instance.AttackCard(_targetedCard, _ownerTag);
    }

    [ServerRpc]
    void LaunchEffect_ServerRpc(int _cardID,PlayerEnum _ownerTag)
    {
        SpellManager.Instance.LaunchEffect(_cardID, _ownerTag);
    }

    [ServerRpc]
    void SelectCardForEffect_ServerRpc(int _targetedCard, PlayerEnum _owner)
    {

    }
    #endregion

}
