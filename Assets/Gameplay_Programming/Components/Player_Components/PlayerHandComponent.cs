using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHandComponent : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> cardSelectedID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> cardHoveredID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] List<HandCardComponent> cardsInHand;

    public List<HandCardComponent> Cards => cardsInHand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void Init()
    {
        Invoke(nameof(InitDraw), Time.deltaTime);
    }

    void InitDraw()
    {
        DrawCard(3);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCardPosition();
        HoveredUpdate();
        UpdateSelectedCard();
    }

    #region Update

    /// <summary>
    /// Will draw in blue the hovered Card
    /// </summary>
    void HoveredUpdate()
    {
        int _handSize = cardsInHand.Count;
        for (int _i = 0; _i < _handSize; _i++)
        {
            HandCardComponent _card = cardsInHand[_i];
            if (!_card) continue;

            if (_i == cardHoveredID.Value)
            {
                if (IsOwner)
                    _card.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                else
                    _card.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }
            else
                _card.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }

    /// <summary>
    /// Will move the selected Card to the mouse Position
    /// </summary>
    void UpdateSelectedCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        if (IsOwner)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] _hits = Physics.RaycastAll(_ray, 20.0f);
            if (Physics.Raycast(_ray, out RaycastHit _hit, 20.0f))
            {
                Vector3 _point = _hit.point;
                _card.transform.position = new Vector3(_point.x, 2.0f, _point.z);
            }
        }
        else
            _card.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }

    /// <summary>
    /// Will put the card in the right position;
    /// </summary>
    void UpdateCardPosition()
    {
        int _size = cardsInHand.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            int _indexOffset = _i - (_size / 2);
            HandCardComponent _card = cardsInHand[_i];
            if (!_card) continue;

            if (_i != cardSelectedID.Value)
                _card.transform.localPosition = new Vector3(_indexOffset * 3.0f, 0.0f, 0.0f);
        }
    }

    #endregion

    #region Card Hovered

    public void SetHoveredCard(int _id)
    {
        if (cardHoveredID.Value == _id) return;
        cardHoveredID.Value = _id;
    }

    public void UnhoverCard()
    {
        if (cardHoveredID.Value == -1) return;
        cardHoveredID.Value = -1;
    }

    #endregion

    #region Card Selected

    public void SelectCard(int _id)
    {
        if (cardSelectedID.Value == _id) return;
        cardSelectedID.Value = _id;

        Invoke(nameof(SelectCard_ClientRpc), Time.deltaTime);
    }

    [ClientRpc]
    void SelectCard_ClientRpc()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card)
        {
            GameManager.Instance.debugWidget.SetDebugText("Can't get card");
            return;
        }

        _card.GetComponent<BoxCollider>().enabled = false;
        _card.IsSelected = true;
    }

    public void ReleaseCard()
    {
        ReleaseCard_ClientRpc();
        cardSelectedID.Value = -1;
    }

    [ClientRpc]
    void ReleaseCard_ClientRpc()
    {
        if (cardSelectedID.Value == -1) return;

        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        _card.IsSelected = false;
        _card.GetComponent<BoxCollider>().enabled = true;
    }

    #endregion

    #region Card Draw

    public void DrawCard(int _amount)
    {
        for (int _i = 0; _i < _amount; _i++)
        {
            SpawnCard_ServerRpc(); //C'EST INVERSE
        }
    }

    [ServerRpc]
    void SpawnCard_ServerRpc()
    {
        HandCardComponent _card = Instantiate(CardManager.Instance.handCardPrefab);
        _card.NetworkObject.Spawn();
        _card.NetworkObject.TrySetParent(gameObject, true);

        SetCardInHand_ClientRpc();
    }

    [ClientRpc]
    void SetCardInHand_ClientRpc()
    {
        cardsInHand.Clear();
        HandCardComponent[] _cards = GetComponentsInChildren<HandCardComponent>();
        foreach (HandCardComponent _card in _cards)
        {
            if (_card)
                cardsInHand.Add(_card);
        }
    }

    #endregion

    public void RemoveSelectedCard()
    {
        HandCardComponent _card = GetSelectedCard();
        if (!_card) return;

        ReleaseCard();
        _card.NetworkObject.Despawn(true);
        Invoke(nameof(SetCardInHand_ClientRpc), Time.deltaTime);
    }

    public HandCardComponent GetSelectedCard()
    {
        if (cardSelectedID.Value < 0 || cardSelectedID.Value >= cardsInHand.Count)
            return null;

        return cardsInHand[cardSelectedID.Value];
    }
}
