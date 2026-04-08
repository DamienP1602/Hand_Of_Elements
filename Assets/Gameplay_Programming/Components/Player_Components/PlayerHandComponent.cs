using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [SerializeField] List<HandCardComponent> cardsInHand;
    [SerializeField] HandCardComponent selectedCard;

    public List<HandCardComponent> Cards => cardsInHand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init()
    {
        DrawCard();
        DrawCard();
        DrawCard();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedCard)
            UpdateSelectedCard();

        UpdateCardPosition();
    }

    void UpdateCardPosition()
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];

            if (!_card.IsSelected)
                _card.transform.localPosition = new Vector3(_i * 3.0f, 0.0f, 0.0f);
        }
    }

    void UpdateSelectedCard()
    {
        if (IsOwner)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f);

            bool _hasHit = false;
            foreach (RaycastHit _hit in _hits)
            {
                if (_hit.collider.GetComponent<BoardComponent>())
                {
                    Vector3 _point = _hit.point;
                    selectedCard.transform.position = new Vector3(_point.x, 2.0f, _point.z);
                    _hasHit = true;
                }
            }
            Debug.DrawRay(_ray.origin, _ray.direction * 20.0f, _hasHit ? Color.green : Color.red);
        }
        else
        {
            selectedCard.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }

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

    public void DrawCard()
    {
        SpawnCard_ServerRpc();
    }

    [ServerRpc]
    void SpawnCard_ServerRpc()
    {
        HandCardComponent _card = Instantiate(CardManager.Instance.Prefab, transform);
        _card.NetworkObject.Spawn();
        _card.NetworkObject.TrySetParent(gameObject, true);

        SetCardInHand_ClientRpc();
    }

    [ClientRpc]
    void SetCardInHand_ClientRpc()
    {
        GameManager _manager = GameManager.Instance;
        if (!_manager) return;

        List<PlayerEntity> _players = _manager.GetAllPlayers;
        foreach (PlayerEntity _player in _players)
        {
            if (_player)
            {
                HandCardComponent[] _cards = _player.GetComponentsInChildren<HandCardComponent>();
                _player.HandComponent.cardsInHand = _cards.ToList();
            }
        }
    }

    [ClientRpc]
    public void SetSelectedCard_ClientRpc(int _id)
    {
        selectedCard = cardsInHand[_id];
        selectedCard.IsSelected = true;
    }

    [ClientRpc]
    public void ReleaseCard_ClientRpc()
    {
        if (!selectedCard) return;

        selectedCard.IsSelected = false;
        selectedCard = null;
    }
}
