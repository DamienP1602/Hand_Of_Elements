using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDiscardPileComponent : NetworkBehaviour
{
    [SerializeField] List<int> discardedCardIDs;
    [SerializeField] Transform discardedCardTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Functions

    public void AddHandCard(CardComponent _card,int _cardHandIndex)
    {
        _card.SetIsInteractable(false);
        _card.MovementComponent.SetSpeed(6.0f);
        _card.MovementComponent.SetDestination(discardedCardTransform.position);
        _card.MovementComponent.SetRotationDestination(discardedCardTransform.rotation);

        _card.MovementComponent.OnDestinationReached += () => _card.FadeComponent.SetFade(CardFadeComponent.FadeStatus.FadeOut);
        if (IsOwner)
            _card.FadeComponent.OnFadeFinish += () => RemoveCard_ServerRpc(_cardHandIndex);

        discardedCardIDs.Add(_card.Data.cardID);
    }

    public void AddBoardCard(CardComponent _card)
    {
        _card.MovementComponent.SetSpeed(6.0f);
        _card.MovementComponent.SetDestination(discardedCardTransform.position);
        _card.MovementComponent.SetRotationDestination(discardedCardTransform.rotation);

        _card.MovementComponent.OnDestinationReached += () => _card.FadeComponent.SetFade(CardFadeComponent.FadeStatus.FadeOut);
        if (IsServer)
            _card.FadeComponent.OnFadeFinish += () => _card.NetworkObject.Despawn(true);

        discardedCardIDs.Add(_card.Data.cardID);
    }

    #endregion

    #region ServerRpc

    [ServerRpc]
    void RemoveCard_ServerRpc(int _handIndex)
    {
        PlayerEntity _player = GetComponentInParent<PlayerEntity>();
        _player.HandComponent.RemoveCard(_handIndex);
    }

    #endregion
}
