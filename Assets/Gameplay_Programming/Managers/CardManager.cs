using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    [field: SerializeField] public HandCardComponent handCardPrefab { get; private set; }
    [field: SerializeField] public BoardCardComponent boardCardPrefab { get; private set; }

    [field: SerializeField] public Vector3 cardShowPositon { get; private set; }

    [Header("Card Lists")]
    [SerializeField] List<BaseCardData> allCards;

    #region Getters

    public BaseCardData GetCard(int _id)
    {
        if (_id < 0 || _id >= allCards.Count)
        {
            return null;
        }

        return FindCardAt(_id);
    }

    public bool IsSoldierID(int _id)
    {
        if (_id < 0 || _id >= allCards.Count) return false;

        return FindCardAt(_id) is SoldierCardData;
    }

    BaseCardData FindCardAt(int _id)
    {
        foreach (BaseCardData _data in allCards)
        {
            if (_data.cardID == _id)
                return _data;
        }
        return null;
    }

    #endregion

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

    #region Menu

    [ContextMenu("Put All Card")]
    public void PutCardInList()
    {
        object[] _cards = Resources.FindObjectsOfTypeAll(typeof(BaseCardData));

        allCards.Clear();
        foreach (object _card in _cards)
        {
            BaseCardData _castedCard = _card as BaseCardData;
            allCards.Add(_castedCard);
        }
    }

    #endregion
}
