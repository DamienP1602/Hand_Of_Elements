using System.Collections.Generic;
using UnityEngine;

public class BoardComponent : MonoBehaviour
{
    [Header("Player One")]
    [SerializeField] List<BoardSlotComponent> playerOneSlots;

    [Header("Player Two")]
    [SerializeField] List<BoardSlotComponent> playerTwoSlots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init(playerOneSlots);
        Init(playerTwoSlots);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init(List<BoardSlotComponent> _board)
    {
        int _size = _board.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            BoardSlotComponent _slot = _board[_i];
            _slot.SlotIndex = _i;
        }
    }

    public BoardSlotComponent GetSlot(PlayerEnum _playerTag, int _index)
    {
        return _playerTag == PlayerEnum.Player_One ? playerOneSlots[_index] : playerTwoSlots[_index];
    }
}
