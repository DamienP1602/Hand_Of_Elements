using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BoardComponent : MonoBehaviour
{
    [Header("Player One Parameters")]
    [SerializeField] Transform playerOneSide;
    [SerializeField] Transform playerOneDeck;
    [SerializeField] List<BoardSlotComponent> playerOneSlots;

    [Header("Player Two Parameters")]
    [SerializeField] Transform playerTwoSide;
    [SerializeField] Transform playerTwoDeck;
    [SerializeField] List<BoardSlotComponent> playerTwoSlots;

    #region Getters

    public Vector3 GetDeckPosition(PlayerEnum _playerTag)
    {
        return _playerTag == PlayerEnum.Player_One ? playerOneDeck.position : playerTwoDeck.position;
    }

    public BoardSlotComponent GetSlot(PlayerEnum _playerTag, int _index)
    {
        if (_index < 0 || _index >= playerOneSlots.Count) return null;

        switch (_playerTag)
        {
            case PlayerEnum.Player_One:
                return playerOneSlots[_index];
            case PlayerEnum.Player_Two:
                return playerTwoSlots[_index];
        }

        return null;
    }

    public int GetSlotIndex(BoardCardComponent _card, PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        int _size = _slots.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            BoardSlotComponent _slot = _slots[_i];
            if (_slot.Card == _card)
                return _i;
        }
        return -1;
    }

    public PlayerEnum GetOwnerOfCard(BoardCardComponent _boardCard)
    {
        List<BoardSlotComponent> _allSlots = GetAllSlots();

        foreach (BoardSlotComponent _slot in _allSlots)
        {
            if (_slot.IsEmpty) continue;

            if (_slot.Card == _boardCard)
                return _slot.PlayerTag;
        }

        return PlayerEnum.Player_NONE;
    }

    public PlayerEnum GetOwnerOfCard(int _cardID)
    {
        List<BoardSlotComponent> _allSlots = GetAllSlots();

        foreach (BoardSlotComponent _slot in _allSlots)
        {
            if (_slot.IsEmpty) continue;

            if (_slot.Card.ID == _cardID)
                return _slot.PlayerTag;
        }

        return PlayerEnum.Player_NONE;
    }

    public BoardCardComponent GetSelectedCard(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (_slot.IsEmpty) continue;

            if (_slot.Card.IsSelected)
                return _slot.Card;
        }
        return null;
    }

    public BoardSlotComponent GetCardFromCardID(PlayerEnum _playerTag, int _targetedCardID)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        int _size = _slots.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            BoardSlotComponent _slot = _slots[_i];
            if (!_slot) continue;

            if (_slot.IsEmpty) continue;

            if (_i == _targetedCardID)
                return _slot;
        }
        return null;
    }

    public List<BoardSlotComponent> GetSlotsFromTag(PlayerEnum _tag) => _tag == PlayerEnum.Player_One ? playerOneSlots : playerTwoSlots;

    List<BoardSlotComponent> GetAllSlots()
    {
        List<BoardSlotComponent> _allSlots = new List<BoardSlotComponent>();
        _allSlots.AddRange(playerOneSlots);
        _allSlots.AddRange(playerTwoSlots);

        return _allSlots;
    }

    public BoardSlotComponent GetRandomCardOnBoard(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        List<int> _availableSlots = new();

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (!_slot.IsEmpty)
                _availableSlots.Add(_slot.GetSlotIndex);
        }

        if (_availableSlots.Count > 0)
        {
            int _randomIndex = UnityEngine.Random.Range(0, _availableSlots.Count);
            int _slotIndex = _availableSlots[_randomIndex];
            return _slots[_slotIndex];
        }
        else
            return null;

    }

    public List<BoardSlotComponent> GetAllSlotCards(PlayerEnum _tag)
    {
        List<BoardSlotComponent> _result = new();

        List<BoardSlotComponent> _slots = GetSlotsFromTag(_tag);
        foreach (BoardSlotComponent _slot in _slots)
        {
            if (!_slot.IsEmpty)
                _result.Add(_slot);
        }

        return _result;
    }

    #endregion

    #region Setters

    public void SetCardCanAttack(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (_slot.IsEmpty) continue;

            _slot.Card.SetCanAttack(true);
        }
    }

    public void SetPlayerBoardSide(PlayerEntity _player)
    {
        Vector3 _offset = Vector3.forward * (_player.IsOwner ? 4.5f : -4.5f) + Vector3.down * 2.0f;
        Vector3 _newPos = _player.transform.position + _offset;

        if (_player.PlayerTag == PlayerEnum.Player_One)
            playerOneSide.position = _newPos;
        else
            playerTwoSide.position = _newPos;
    }

    #endregion

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void UnhoverCards(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (_slot.IsEmpty) continue;

            if (_slot.Card.IsHovered)
                _slot.Card.SetIsHovered(false);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void HoverCard(int _id, PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        int _size = _slots.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            BoardSlotComponent _slot = _slots[_i];
            if (_slot.IsEmpty) continue;

            _slot.Card.SetIsHovered(_i == _id);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void SelectCard(int _id, PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        int _size = _slots.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            BoardSlotComponent _slot = _slots[_i];
            if (_slot.IsEmpty) continue;

            _slot.Card.SetIsSelected(_i == _id);
        }
    }

    /// <summary>
    /// Server Fuction
    /// </summary>
    public void ReleaseCards(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (_slot.IsEmpty || !_slot.Card.IsSelected) continue;

            _slot.Card.SetIsSelected(false);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public BoardSlotComponent GetFirstEmptySlot(PlayerEnum _playerTag)
    {
        List<BoardSlotComponent> _slots = GetSlotsFromTag(_playerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            if (_slot.IsEmpty)
                return _slot;
        }
        return null;
    }

    #endregion

}
