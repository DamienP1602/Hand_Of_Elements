using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class CardDictionary
{
    [Serializable]
    public struct Value
    {
        public int key;
        public BaseCardData card;

        public Value(int _key, BaseCardData _card)
        {
            key = _key;
            card = _card;
        }
    }
    [SerializeField] List<Value> values;

    public int Count => values.Count;

    public void Add(int _key, BaseCardData _value)
    {
        values.Add(new Value(_key, _value));
    }

    public bool Contains(int _key)
    {
        foreach (Value _pair in values)
        {
            if (_pair.key == _key)
                return true;
        }
        return false;
    }

    public BaseCardData this[int _key]
    {
        get 
        {
            foreach (Value _pair in values)
            {
                if (_pair.key == _key)
                    return _pair.card;
            }
            return null;
        }
    }
}

public class CardManager : Singleton<CardManager>
{
    [field: SerializeField] public HandCardComponent handCardPrefab { get; private set; }
    [field: SerializeField] public BoardCardComponent boardCardPrefab { get; private set; }

    [field: SerializeField] public Vector3 cardShowPositon { get; private set; }

    [Header("Card Lists")]
    [SerializeField] MasterCardData masterCardFile;

    [SerializeField] CardDictionary allCards;

    #region Getters

    public BaseCardData GetCard(int _id)
    {
        if (!allCards.Contains(_id))
        {
            return null;
        }

        return allCards[_id];
    }

    public bool IsSoldierID(int _id)
    {
        if (!allCards.Contains(_id))
        {
            return false;
        }

        return allCards[_id] is SoldierCardData;
    }

    public List<BaseCardData> GetAllCards()
    {
        List<BaseCardData> _result = new();

        int _size = allCards.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            _result.Add(allCards[_i]);
        }

        return _result;
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        Dictionary<int, BaseCardData> _cards = masterCardFile.GetAllCards();

        foreach (KeyValuePair<int,BaseCardData> _card in _cards)
        {
            allCards.Add(_card.Key, _card.Value);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cardShowPositon, Vector3.one);
    }

}
