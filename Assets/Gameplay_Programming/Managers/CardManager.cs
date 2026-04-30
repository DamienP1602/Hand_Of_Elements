using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    [field:SerializeField] public HandCardComponent handCardPrefab { get; private set; }
    [field:SerializeField] public BoardCardComponent boardCardPrefab { get; private set; }

    [field:SerializeField] public Vector3 cardShowPositon { get; private set; }

    [Header("Card Lists")]
    [SerializeField] List<BaseCardData> allCards;
    Dictionary<int, BaseCardData> cardsDictionary = new Dictionary<int, BaseCardData>();

    #region Getters

    public BaseCardData GetCard(int _id)
    {
        if (_id < 0 || _id >= cardsDictionary.Count) return null;

        return cardsDictionary[_id];
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        foreach (BaseCardData _card in allCards)
        {
            cardsDictionary[_card.cardID] = _card;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cardShowPositon, Vector3.one);
    }
}
