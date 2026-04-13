using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> cardSelectedID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] List<HandCardComponent> cardsInHand;

    public List<HandCardComponent> Cards => cardsInHand;
    public HandCardComponent GetSelectedCard => cardsInHand[cardSelectedID.Value];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init()
    {
        Invoke(nameof(DrawCard), 0.5f);
        Invoke(nameof(DrawCard), 1.0f);
        Invoke(nameof(DrawCard), 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (cardSelectedID.Value > -1)
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
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
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
        HandCardComponent _card = cardsInHand[cardSelectedID.Value];
        if (IsOwner)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f);

            foreach (RaycastHit _hit in _hits)
            {
                Vector3 _point = _hit.point;
                _card.transform.position = new Vector3(_point.x, 2.0f, _point.z);
            }
        }
        else
        {
            _card.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        }

    }

    void UpdateCardPosition()
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];

            if (_i != cardSelectedID.Value)
                _card.transform.localPosition = new Vector3(_i * 3.0f, 0.0f, 0.0f);
        }
    }

    [ClientRpc]
    public void SelectCard_ClientRpc(int _id)
    {
        cardSelectedID.Value = _id;
        HandCardComponent _card = cardsInHand[_id];
        _card.GetComponent<BoxCollider>().enabled = false;
        _card.IsSelected = true;
    }

    [ClientRpc]
    public void ReleaseCard_ClientRpc()
    {
        if (cardSelectedID.Value == -1) return;

        HandCardComponent _card = cardsInHand[cardSelectedID.Value];
        _card.IsSelected = false;
        _card.GetComponent<BoxCollider>().enabled = true;
        cardSelectedID.Value = -1;
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
                _player.HandComponent.cardsInHand.Clear();
                HandCardComponent[] _cards = _player.GetComponentsInChildren<HandCardComponent>();
                foreach (HandCardComponent _card in _cards)
                {
                    if (_card)
                        _player.HandComponent.cardsInHand.Add(_card);
                }
            }
        }
    }

    #endregion

    public void RemoveSelectedCard()
    {
        PlayerEntity _player = GameManager.Instance.GetPlayerFromTurn();
        HandCardComponent _card = _player.HandComponent.GetSelectedCard;
        _player.HandComponent.cardsInHand.Remove(_card);
        _card.NetworkObject.Despawn(true);
        _card.NetworkObject.OnDeferredDespawnComplete += () => 
            {
                SetCardInHand_ClientRpc();
                return true;
            };

        _player.HandComponent.ReleaseCard_ClientRpc();
    }
}
