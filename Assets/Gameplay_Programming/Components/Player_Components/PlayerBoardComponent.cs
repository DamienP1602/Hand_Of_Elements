using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerBoardComponent : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] Vector3 boardStartPos;
    [SerializeField] BoardSlotComponent slotPrefab;
    [SerializeField] List<BoardSlotComponent> slots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InvertBoardPosition()
    {
        boardStartPos = new Vector3(boardStartPos.x, boardStartPos.y, boardStartPos.z * -1.0f);
    }

    public void CreateBoard(PlayerEnum _playerTag)
    {
        for (int _i = 0; _i < 5; _i++)
        {
            int _indexOffset = _i - (5 / 2);
            BoardSlotComponent _board = Instantiate(slotPrefab);
            _board.NetworkObject.Spawn();

            Vector3 _pos = boardStartPos + (Vector3.right * (2.5f * _indexOffset));
            _board.transform.position = _pos;
            _board.NetworkObject.TrySetParent(gameObject, true);

            _board.Init(_playerTag, _i);

            slots.Add(_board);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(boardStartPos, 1.0f);
    }
}
