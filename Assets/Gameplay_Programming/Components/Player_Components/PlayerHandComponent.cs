using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [SerializeField] List<HandCardComponent> cardsInHand;

    #region Getter

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
        HoveredUpdate();
        UpdateSelectedCard();
    }

    #region Update

    /// <summary>
    /// Will draw in blue the hovered Card
    /// </summary>
    void HoveredUpdate()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (!_card) continue;

            if (_card.IsHovered)
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            else
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }

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
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f);
            if (Physics.Raycast(_ray, out RaycastHit _hit, 20.0f))
            {
                Vector3 _point = _hit.point;
                _card.transform.position = new Vector3(_point.x, 2.0f, _point.z);
            }
        }
        else
            _card.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }

    #endregion

    #region Card Hovered

    /// <summary>
    /// Server Function
    /// </summary>
    public void SetHoveredCard(int _id)
    {
        HandCardComponent _cardToHover = GetCard(_id);
        if (!_cardToHover || _cardToHover.IsHovered) return;

        foreach (HandCardComponent _card in cardsInHand)
        {
            _card.SetIsHovered(_card == _cardToHover);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void UnhoverCard()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (_card.IsHovered)
            {
                _card.SetIsHovered(false);
            }
        }
    }

    #endregion

    #region Card Selected

    public void SelectCard(int _id)
    {
        HandCardComponent _card = GetCard(_id);
        if (!_card) return;

        SelectCard_ClientRpc(_id);
        _card.SetIsSelected(true);
    }

    [ClientRpc]
    void SelectCard_ClientRpc(int _index)
    {
        HandCardComponent _card = GetCard(_index);
        if (!_card) return;

        _card.GetComponent<BoxCollider>().enabled = false;
    }

    public void ReleaseCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        ReleaseCard_ClientRpc();
        _card.SetIsSelected(false);
    }

    [ClientRpc]
    void ReleaseCard_ClientRpc()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        _card.GetComponent<BoxCollider>().enabled = true;
        UpdateCardPosition();
    }

    public void RemoveSelectedCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return; 

        _card.NetworkObject.Despawn(true);
        Invoke(nameof(SetCardInHand_ClientRpc),0.1f);
    }

    #endregion

    #region Card Draw

    public void DrawCard(int _amount)
    {
        for (int _i = 0; _i < _amount; _i++)
        {
            HandCardComponent _card = Instantiate(CardManager.Instance.handCardPrefab, GameManager.Instance.Board.GetDeckPosition(GetComponent<PlayerEntity>().PlayerTag), Quaternion.identity);
            _card.NetworkObject.Spawn();
            _card.NetworkObject.TrySetParent(gameObject, true);

            PlayerDeckComponent _deck = GetComponent<PlayerDeckComponent>();
            int _random = UnityEngine.Random.Range(0, _deck.CardCount);
            _card.SetID(_random);
        }
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

    /// <summary>
    /// Will put the card in the right position;
    /// </summary>
    void UpdateCardPosition()
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];
            if (!_card) continue;

            float _indexOffset = _i - (_size / 2) + (_size % 2 != 0 ? 0.0f : 0.5f);
            _card.MovementComponent.SetDestination(transform.position + new Vector3(_indexOffset * 3.0f, 0.0f, 0.0f));
        }
    }
    #endregion
}
