using System;
using System.Collections.Generic;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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

    public Vector3 GetDeckPosition(PlayerEnum _playerTag)
    {
        return _playerTag == PlayerEnum.Player_One ? playerOneDeck.position : playerTwoDeck.position;
    }
}
