using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeckComponent : NetworkBehaviour
{
    //[SerializeField] NetworkVariable<List<int>> cardsInDeck = new NetworkVariable<List<int>>(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] List<int> cardsInDeck = new List<int>();

    #region Getters

    public int CardCount => cardsInDeck.Count;

    public BaseCardData GetRandomCard()
    {
        if (cardsInDeck.Count == 0) return null;

        int _random = UnityEngine.Random.Range(0, cardsInDeck.Count);

        return CardManager.Instance.GetCard(cardsInDeck[_random]);
    }

    #endregion

    void Start()
    {

    }

    void Update()
    {

    }

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
}
