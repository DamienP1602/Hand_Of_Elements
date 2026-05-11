using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellManager : Singleton<SpellManager>
{
    [Serializable]
    struct TargetedData : INetworkSerializable
    {
        public int cardID;
        public PlayerEnum cardOwnerTag;

        public TargetedData(int _id, PlayerEnum _tag)
        {
            cardID = _id;
            cardOwnerTag = _tag;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardID);
            serializer.SerializeValue(ref cardOwnerTag);
        }
    }

    [Header("Parameters")]
    [SerializeField] CardEffectData currentEffect;
    [SerializeField] CardComponent card;
    [SerializeField] PlayerEntity playerOwner;

    [Header("Targets")]
    [SerializeField] List<TargetedData> targets = new();
    [SerializeField] PlayerEntity playerTarget;


    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffect(int _cardID, PlayerEnum _ownerType)
    {
        // Reset Targets
        targets.Clear();
        playerTarget = null;

        // Set Parameters
        playerOwner = GameManager.Instance.GetPlayer(_ownerType);
        card = playerOwner.HandComponent.GetSelectedCard();
        currentEffect = card.Data.effect;
        
        // Check for selection mode
        switch (currentEffect.selectionMode)
        {
            case CardEffectData.CardEffectSelectionMode.SingleTarget:
                SetCardSelection(playerOwner);
                return;
            case CardEffectData.CardEffectSelectionMode.Self:
                SelectSelf(_ownerType);
                break;
            case CardEffectData.CardEffectSelectionMode.Opponent:
                SelectOpponent(_ownerType);
                break;
        }
        
        // If can play the effect, play it
        CastEffect();

        // Remove card from Hand
        playerOwner.HandComponent.RemoveSelectedCard();
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffectSelection(int _selectedCardID, PlayerEnum _ownerType)
    {
        // Stop Card Selection
        playerOwner.InteractComponent.SetSelectCard(false);

        // Set Targets
        targets.Add(new TargetedData(_selectedCardID, _ownerType));

        // Play the effect
        CastEffect();

        // Remove card from Hand
        playerOwner.HandComponent.RemoveSelectedCard();
    }

    /// <summary>
    /// Server Function
    /// </summary>
    void CastEffect()
    {
        switch (currentEffect.effectMode)
        {
            case CardEffectData.CardEffectMode.Summon:
                SummonCard();
                break;
            case CardEffectData.CardEffectMode.Heal:
                RestaureHealth();
                break;
            case CardEffectData.CardEffectMode.InstantDamage:
                DealDamages();
                break;
            case CardEffectData.CardEffectMode.Debuff:
                break;
        }
    }

    #region Effect Functions

    void DealDamages()
    {
        foreach (TargetedData _target in targets)
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.cardID);
            _slot.Card.RemoveHealth(currentEffect.amount);
        }
    }

    void RestaureHealth()
    {
        foreach (TargetedData _target in targets)
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.cardID);
            _slot.Card.RestaureHealth(currentEffect.amount);
        }
    }

    void SummonCard()
    {
        PlayerEnum _ownerTag = playerTarget.PlayerTag;
        BoardSlotComponent _slot = GameManager.Instance.Board.GetFirstEmptySlot(_ownerTag);
        if (!_slot) return;

        _slot.PutCardInSlot(_slot.transform.position,currentEffect.cardReference.cardID);
    }

    #endregion

    #region Selection Functions

    void SetCardSelection(PlayerEntity _player)
    {
        _player.InteractComponent.SetSelectCard(true);
    }

    void SelectSelf(PlayerEnum _tag)
    {
        playerTarget = GameManager.Instance.GetPlayer(_tag);
    }

    void SelectOpponent(PlayerEnum _tag)
    {
        playerTarget = GameManager.Instance.GetOtherPlayer(_tag);
    }

    #endregion

    #endregion
}
