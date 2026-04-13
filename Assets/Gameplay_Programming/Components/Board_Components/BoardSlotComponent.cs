using Unity.Netcode;
using UnityEngine;

public class BoardSlotComponent : NetworkBehaviour
{
    [SerializeField] Transform cardTransform;
    [SerializeField] BoardCardComponent card;
    [SerializeField] NetworkVariable<PlayerEnum> playerTag = new NetworkVariable<PlayerEnum>(PlayerEnum.Player_One, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> slotIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public PlayerEnum PlayerTag => playerTag.Value;
    public int GetSlotIndex => slotIndex.Value;

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

    public void PutCardInSlot()
    {
        card = Instantiate(CardManager.Instance.boardCardPrefab, cardTransform);
        card.NetworkObject.Spawn();

        PutCardInSlot_ClientRpc();
    }

    [ClientRpc]
    void PutCardInSlot_ClientRpc()
    {
        card.NetworkObject.TrySetParent(cardTransform, true);
        card.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
    }
}
