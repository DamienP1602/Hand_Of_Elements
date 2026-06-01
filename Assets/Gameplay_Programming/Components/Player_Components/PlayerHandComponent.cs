using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [Header("Debug")]
    public bool showDebug;

    [Header("Parameters")]
    [SerializeField] List<HandCardComponent> cardsInHand;
    [SerializeField] Vector3 selectedCardPosition;
    [SerializeField] LayerMask boardLayer;
    [SerializeField] HandCardComponent selectedSpell;
    [Header("Card In Hand Parameters")]
    [SerializeField] float cardDistanceOffset = 1.9f;
    [SerializeField] float cardsRotationOffset = 1.0f;
    [SerializeField] float cardHoveredScale = 1.5f;
    [SerializeField] float cardHoveredForwardOffset = 1.5f;

    #region Getter

    Vector3 SelectedPosition => selectedCardPosition + transform.position;

    public HandCardComponent GetSelectedCard()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (_card.IsSelected)
                return _card;
        }
        return null;
    }

    public HandCardComponent GetCard(int _index)
    {
        if (_index < 0 || _index >= cardsInHand.Count)
            return null;
        return cardsInHand[_index];

    }

    public int GetIndexOf(HandCardComponent _card)
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            if (_card == cardsInHand[_i])
                return _i;
        }
        return -1;
    }

    public bool Contains(HandCardComponent _toCheck)
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (_card == _toCheck)
                return true;
        }
        return false;
    }

    #endregion

    void Start()
    {

    }

    void Update()
    {
        UpdateSelectedCard();
    }

    #region Update

    /// <summary>
    /// Will move the selected Card to the mouse Position
    /// </summary>
    void UpdateSelectedCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        if (IsOwner)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f, boardLayer);
            if (_hits.Length > 0)
            {
                RaycastHit _hit = _hits[0];
                Vector3 _point = _hit.point;
                _card.transform.position = _point;
            }
        }
        else
            _card.MovementComponent.SetDestination(SelectedPosition);
    }

    #endregion

    #region Server Functions

    /// <summary>
    /// Server Function
    /// Also will put new position for the new hovered card and put in the default position for the old one
    /// </summary>
    public void SetHoveredCard(int _id)
    {
        HandCardComponent _cardToHover = GetCard(_id);
        if (!_cardToHover || _cardToHover.IsHovered) return;

        int _handSize = cardsInHand.Count;
        for (int _i = 0; _i < _handSize; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];
            bool _isHovered = _card == _cardToHover;

            _card.SetIsHovered(_isHovered);

            HoverCardVisual_ClientRpc(_i, _isHovered);
        }
    }

    /// <summary>
    /// Server Function
    /// Also will put the last hovered card to its default position in hand
    /// </summary>
    public void UnhoverCard()
    {
        int _handSize = cardsInHand.Count;
        for (int _i = 0; _i < _handSize; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];
            if (_card.IsHovered)
            {
                _card.SetIsHovered(false);

                HoverCardVisual_ClientRpc(_i, false);
            }
        }
    }

    /// <summary>
    /// Server Functions
    /// </summary>
    public void SelectCard(int _id)
    {
        HandCardComponent _card = GetCard(_id);
        if (!_card) return;

        SelectCard_ClientRpc(_id);
        _card.SetIsSelected(true);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void ReleaseCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        ReleaseCard_ClientRpc();
        _card.SetIsSelected(false);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void RemoveSelectedCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        _card.NetworkObject.Despawn(true);
        Invoke(nameof(SetCardInHand_ClientRpc), 0.1f);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void DrawCard(int _amount)
    {
        PlayerEntity _owner = GetComponent<PlayerEntity>();
        for (int _i = 0; _i < _amount; _i++)
        {
            PlayerDeckComponent _deck = GetComponent<PlayerDeckComponent>();
            if (_deck.CardCount == 0)
                return;

            HandCardComponent _card = Instantiate(CardManager.Instance.handCardPrefab, GameManager.Instance.Board.GetDeckPosition(_owner.PlayerTag), Quaternion.identity);
            _card.NetworkObject.Spawn();
            _card.NetworkObject.TrySetParent(gameObject, true);

            BaseCardData _data = _deck.GetRandomCard();
            _card.Set(_data.cardID, _owner.PlayerTag);

            RemoveCardInDeck_ClientRpc(_data.cardID);
        }
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    void SelectCard_ClientRpc(int _index)
    {
        HandCardComponent _card = GetCard(_index);
        if (!_card) return;

        _card.GetComponent<BoxCollider>().enabled = false;
    }

    [ClientRpc]
    void ReleaseCard_ClientRpc()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        _card.GetComponent<BoxCollider>().enabled = true;
        UpdateCardPosition();
    }

    [ClientRpc]
    void RemoveCardInDeck_ClientRpc(int _id)
    {
        PlayerDeckComponent _deck = GetComponent<PlayerDeckComponent>();
        _deck.RemoveCard(_id);
    }

    [ClientRpc]
    public void SetCardInHand_ClientRpc()
    {
        cardsInHand.Clear();
        HandCardComponent[] _cards = GetComponentsInChildren<HandCardComponent>();
        foreach (HandCardComponent _card in _cards)
        {
            if (_card)
            {
                cardsInHand.Add(_card);
            }
        }
        UpdateCardPosition();
    }


    [ClientRpc]
    public void SelectCardVisual_ClientRpc()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        if (IsOwner)
        {
            selectedSpell = _card;
            Canvas _canva = _card.GetComponentInChildren<Canvas>();
            _canva.enabled = false;
        }
        else
        {
            _card.MovementComponent.SetDestination(SelectedPosition);
        }

    }

    [ClientRpc]
    public void UnselectCardVisual_ClientRpc()
    {
        if (IsOwner)
        {
            if (!selectedSpell) return;

            Canvas _canva = selectedSpell.GetComponentInChildren<Canvas>();
            _canva.enabled = true;
            selectedSpell = null;
        }
        else
        {
            UpdateCardPosition();
        }
    }

    [ClientRpc]
    void HoverCardVisual_ClientRpc(int _id, bool _isHovered)
    {
        HandCardComponent _cardToHover = GetCard(_id);

        if (IsOwner)
        {
            Vector3 _initialPos = GetCardPosition(_id);
            Vector3 _offset = _isHovered ? Vector3.forward * cardHoveredForwardOffset : Vector3.zero;
            _cardToHover.MovementComponent.SetDestination(_initialPos + _offset);
            _cardToHover.OverlayComponent.SetScaleTarget(_isHovered ? cardHoveredScale : 1.0f);
        }
        else
        {
            _cardToHover.OverlayComponent.SetScaleTarget(_isHovered ? 1.1f : 1.0f);
        }
    }

    #endregion

    #region Functions

    [ContextMenu("Update Card Position")]
    void UpdateCardPosition()
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];
            if (!_card) continue;

            float _xOffset = _i - (_size / 2) + (_size % 2 != 0 ? 0.0f : 0.5f);

            _card.MovementComponent.SetDestination(transform.position + new Vector3(_xOffset * cardDistanceOffset, 0.0f,0.0f));

            float _angle = (_xOffset * cardsRotationOffset) * (IsOwner ? 1.0f : -1.0f);
            _card.MovementComponent.SetRotationDestination(Quaternion.AngleAxis(_angle, transform.up));
        }
    }

    Vector3 GetCardPosition(int _index)
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            if (_index != _i) continue;

            float _xOffset = _i - (_size / 2) + (_size % 2 != 0 ? 0.0f : 0.5f);

            return transform.position + new Vector3(_xOffset * cardDistanceOffset, 0.0f, 0.0f);
        }
        return Vector3.zero;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(SelectedPosition, 0.5f);
    }
}
