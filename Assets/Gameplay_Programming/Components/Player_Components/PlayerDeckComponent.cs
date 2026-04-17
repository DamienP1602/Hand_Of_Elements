using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckComponent : MonoBehaviour
{
    [SerializeField] List<int> cardsInDeck;


    #region Getters

    public int CardCount => cardsInDeck.Count;

    public BaseCardData GetRandomCard()
    {
        if (cardsInDeck.Count == 0) return null;

        int _random = UnityEngine.Random.Range(0, cardsInDeck.Count);
        return CardManager.Instance.GetCard(_random);        
    }

    #endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
