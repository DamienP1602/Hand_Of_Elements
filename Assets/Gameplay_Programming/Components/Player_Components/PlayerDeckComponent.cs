using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeckComponent : NetworkBehaviour
{
    [SerializeField] List<int> cardsInDeck = new List<int>();

    #region Getters

    public int CardCount => cardsInDeck.Count;

    public BaseCardData GetRandomCard()
    {
        if (cardsInDeck.Count == 0) return null;

        int _random = UnityEngine.Random.Range(0, cardsInDeck.Count);

        return CardManager.Instance.GetCard(cardsInDeck[_random]);
    }

    public BaseCardData GetRandomCardOfElement(CardElement _specificElement)
    {
        List<BaseCardData> _sortedList = new();

        List<BaseCardData> _allCards = CardManager.Instance.GetAllCards();
        foreach (BaseCardData _card in _allCards)
        {
            if (_card.cardElement == _specificElement && cardsInDeck.Contains(_card.cardID))
                _sortedList.Add(_card);
        }

        if (_sortedList.Count == 0)
            return null;

        int _random = UnityEngine.Random.Range(0, _sortedList.Count);

        return _sortedList[_random];
    }

    public BaseCardData GetRandomCardOfKey(CardEffectData.KeyEffect _specificKey)
    {
        List<BaseCardData> _sortedList = new();

        List<BaseCardData> _allCards = CardManager.Instance.GetAllCards();
        foreach (BaseCardData _card in _allCards)
        {
            if (_card.hasEffect)
                if (_card.keyEffect == _specificKey)
                    _sortedList.Add(_card);
        }

        if (_sortedList.Count == 0)
            return null;

        int _random = UnityEngine.Random.Range(0, _sortedList.Count);

        return _sortedList[_random];
    }

    #endregion

    void Start()
    {

    }

    void Update()
    {

    }

    #region Functions

    public void RemoveCard(int _id)
    {
        foreach (int _card in cardsInDeck)
        {
            if (_id == _card)
            {
                cardsInDeck.Remove(_card);
                return;
            }
        }
    }

    #endregion

    #region Server Function

    /// <summary>
    /// Server Function
    /// </summary>
    public void AddCardInDeck(PlayerEntity _owner, CardComponent _card, bool _inHand)
    {
        int _index = 0;
        if (_inHand)
            _index = _owner.HandComponent.GetIndexOf(_card as HandCardComponent);
        else
            _index = GameManager.Instance.Board.GetSlotIndex(_card as BoardCardComponent, _owner.PlayerTag);

        AddCardInDeck_ClientRpc(_owner.PlayerTag, _index, _inHand);
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    void AddCardInDeck_ClientRpc(PlayerEnum _ownerType, int _cardIndex, bool _inHand)
    {
        CardComponent _card = null;
        PlayerEntity _player = GameManager.Instance.GetPlayer(_ownerType);
        if (_inHand)
        {
            _card = _player.HandComponent.GetCard(_cardIndex);
        }
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_ownerType, _cardIndex);
            _card = _slot.Card;
        }

        cardsInDeck.Add(_card.Data.cardID);

        _card.MovementComponent.SetSpeed(6.0f);
        _card.MovementComponent.SetDestination(GameManager.Instance.Board.GetDeckPosition(_ownerType) + Vector3.up * 0.5f);
        _card.MovementComponent.OnDestinationReached += () => _card.FadeComponent.SetFade(CardFadeComponent.FadeStatus.FadeOut);
        if (IsServer)
            _card.FadeComponent.OnFadeFinish += () => _card.NetworkObject.Despawn(true);
    }

    #endregion
}
