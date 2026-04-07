using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [SerializeField] List<HandCardComponent> cardsInHand;
    HandCardComponent lastCreatedCard;

    public List<HandCardComponent> Cards => cardsInHand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init()
    {
        AddCardToHand();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ClientRpc]
    public void SetHoveredCard_ClientRpc(int _id)
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            if (_i == _id)
            {
                cardsInHand[_i].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                cardsInHand[_i].GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
            }
        }
    }

    [ClientRpc]
    public void UnhoverCard_ClientRpc()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            _card.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }

    void AddCardToHand()
    {
        lastCreatedCard = Instantiate(CardManager.Instance.Prefab, transform);
        cardsInHand.Add(lastCreatedCard);

        SpawnCard_ServerRpc();
    }

    [ServerRpc]
    public void SpawnCard_ServerRpc()
    {
        lastCreatedCard.NetworkObject.Spawn();
        lastCreatedCard.NetworkObject.TrySetParent(transform);
    }
}
