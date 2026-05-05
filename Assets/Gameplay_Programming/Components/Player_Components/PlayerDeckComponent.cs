using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeckComponent : NetworkBehaviour
{
    [SerializeField] NetworkVariable<List<int>> cardsInDeck = new NetworkVariable<List<int>>(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    #region Getters

    public int CardCount => cardsInDeck.Value.Count;

    public BaseCardData GetRandomCard()
    {
        if (cardsInDeck.Value.Count == 0) return null;

        int _random = UnityEngine.Random.Range(0, cardsInDeck.Value.Count);
        cardsInDeck.Value.Remove(_random);

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
