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
        Invoke(nameof(DrawCard),0.5f);
        Invoke(nameof(DrawCard),1.0f);
        Invoke(nameof(DrawCard),1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedCard)
            UpdateSelectedCard();

        UpdateCardPosition();
        HoveredUpdate();
    }

    #region Card Hovered

    void HoveredUpdate()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (_card.IsSelected) continue;

            if (_card.IsHovered)
            {
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    [ClientRpc]
    public void SetHoveredCard_ClientRpc(int _id)
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];

            // Card is Hovered if _id == _i
            // _id = position in list
            _card.IsHovered = _i == _id;
        }
    }

    [ClientRpc]
    public void UnhoverCard_ClientRpc()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            _card.IsHovered = false;
        }
    }

    #endregion

    #region Card Selected

    void UpdateSelectedCard()
    {
        if (IsOwner)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f);

            foreach (RaycastHit _hit in _hits)
            {
                Vector3 _point = _hit.point;
                selectedCard.transform.position = new Vector3(_point.x, 2.0f, _point.z);
            }
        }
        else
        {
            selectedCard.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }

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

    [ClientRpc]
    public void SetSelectedCard_ClientRpc(int _id)
    {
        selectedCard = cardsInHand[_id];
        selectedCard.GetComponent<BoxCollider>().enabled = false;
        selectedCard.IsSelected = true;
    }

    [ClientRpc]
    public void ReleaseCard_ClientRpc()
    {
        if (!selectedCard) return;

        selectedCard.IsSelected = false;
        selectedCard.GetComponent<BoxCollider>().enabled = true;
        selectedCard = null;
    }

    #endregion

    #region Card Draw

    public void DrawCard()
    {
        SpawnCard_ServerRpc();
    }

    [ServerRpc]
    void SpawnCard_ServerRpc()
    {
        CardComponent _card = Instantiate(CardManager.Instance.handCardPrefab, transform);
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

    #endregion

    public HandCardComponent GetSelectedCard()
    {
        foreach (HandCardComponent _card in cardsInHand)
        {
            if (_card.IsSelected)
                return _card;
        }

        return null;
    }

    public void RemoveSelectedCard()
    {
        if (IsServer)
            Debug.Log("je passe ici par server");

        cardsInHand.Remove(selectedCard);

        ReleaseCard_ClientRpc();
        SetCardInHand_ClientRpc();
    }
}
