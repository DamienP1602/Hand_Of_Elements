using Unity.Netcode;
using UnityEngine;

public class BoardSlotComponent : NetworkBehaviour
{
    [SerializeField] Transform cardTransform;
    [SerializeField] BoardCardComponent card;
    [SerializeField] PlayerEnum playerTag;
    [field:SerializeField] public int SlotIndex { get; set; }

    public PlayerEnum PlayerTag => playerTag;

    public bool IsEmpty => card == null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        PlayerEntity _player = GameManager.Instance.GetPlayer(playerTag);
        HandCardComponent _card = _player.HandComponent.GetSelectedCard();
        if (!_card) return;

        card.NetworkObject.TrySetParent(cardTransform, false);
        card.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
    }
}
