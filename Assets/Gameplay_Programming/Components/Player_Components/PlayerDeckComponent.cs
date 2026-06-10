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
                if (_card.effect.keyEffect == _specificKey)
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

}
