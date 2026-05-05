using Unity.Netcode;
using UnityEngine;

public class BoardSlotComponent : NetworkBehaviour
{
    [SerializeField] Vector3 cardPosition;
    [SerializeField] BoardCardComponent card;
    [SerializeField] NetworkVariable<PlayerEnum> playerTag = new NetworkVariable<PlayerEnum>(PlayerEnum.Player_One, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> slotIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public PlayerEnum PlayerTag => playerTag.Value;
    public int GetSlotIndex => slotIndex.Value;
    public BoardCardComponent Card => card;

    public bool IsEmpty => card == null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(PlayerEnum _playerTag, int _index)
    {
        playerTag.Value = _playerTag;
        slotIndex.Value  = _index;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void PutCardInSlot(Vector3 _startingPos,int _cardID)
    {
        card = Instantiate(CardManager.Instance.boardCardPrefab, _startingPos,Quaternion.identity);
        card.NetworkObject.Spawn();
        card.NetworkObject.TrySetParent(transform, true);

        card.SetID(_cardID);

        PutCardInSlot_ClientRpc();
    }

    [ClientRpc]
    void PutCardInSlot_ClientRpc()
    {
        BoardCardComponent _card = GetComponentInChildren<BoardCardComponent>();
        if (_card)
        {
            card = _card;
            card.transform.position = cardPosition + transform.position;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + cardPosition, 0.25f);
    }
}
