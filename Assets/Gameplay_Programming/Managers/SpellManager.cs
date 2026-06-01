using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.GraphicsBuffer;

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
    [SerializeField] VisualSpellEffectComponent emptyVisualEffect;

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
        if (currentEffect.effectAsset != null)
            LaunchProjectile();
        else
            CastEffect();

        // Remove card from Hand
        playerOwner.HandComponent.RemoveSelectedCard();
        playerOwner.RemoveArcane(card.Data.cardCost);
        playerOwner.SetElementCardPlayed(card.Data.cardElement);
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
        if (currentEffect.effectAsset != null)
            LaunchProjectile();
        else
            CastEffect();

        // Remove card from Hand
        playerOwner.HandComponent.RemoveSelectedCard();
        playerOwner.RemoveArcane(card.Data.cardCost);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void CastEffect()
    {
        switch (currentEffect.effectMode)
        {
            case CardEffectData.CardEffectMode.Summon:
                SummonCard(currentEffect);
                break;
            case CardEffectData.CardEffectMode.Heal:
                RestaureHealth(currentEffect);
                break;
            case CardEffectData.CardEffectMode.InstantDamage:
                DealDamages(currentEffect);
                break;
            case CardEffectData.CardEffectMode.Debuff:
                AddDebuff(currentEffect);
                break;
        }

        if (card.Data.hasElementaryCombo)
        {
            if (playerOwner.LastElementPlayed == card.Data.cardElement)
            {
                CardEffectData _comboEffect = card.Data.elementaryComboEffect;
                switch (_comboEffect.effectMode)
                {
                    case CardEffectData.CardEffectMode.Summon:
                        SummonCard(_comboEffect);
                        break;
                    case CardEffectData.CardEffectMode.Heal:
                        RestaureHealth(_comboEffect);
                        break;
                    case CardEffectData.CardEffectMode.InstantDamage:
                        DealDamages(_comboEffect);
                        break;
                    case CardEffectData.CardEffectMode.Debuff:
                        AddDebuff(_comboEffect);
                        break;
                }
            }
        }
        playerOwner.SetElementCardPlayed(card.Data.cardElement);
    }

    void LaunchProjectile()
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();
        VisualSpellEffectComponent _visual = Instantiate(emptyVisualEffect, _entity.transform);
        _visual.NetworkObject.Spawn();
        _visual.NetworkObject.TrySetParent(_entity.transform, true);
        Invoke(nameof(InitEffect),0.1f);
    }

    void InitEffect()
    {
        InitEffect_ClientRpc(card.Data.cardID, targets.ToArray());
    }

    #region Effect Functions

    void DealDamages(CardEffectData _effect)
    {
        foreach (TargetedData _target in targets)
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.cardID);
            _slot.Card.RemoveHealth(_effect.amount);
        }
    }

    void RestaureHealth(CardEffectData _effect)
    {
        foreach (TargetedData _target in targets)
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.cardID);
            _slot.Card.RestaureHealth(_effect.amount);
        }
    }

    void SummonCard(CardEffectData _effect)
    {
        PlayerEnum _ownerTag = playerTarget.PlayerTag;
        BoardSlotComponent _slot = GameManager.Instance.Board.GetFirstEmptySlot(_ownerTag);
        if (!_slot) return;

        _slot.PutCardInSlot(_slot.transform.position, _effect.cardReference.cardID);
    }

    void AddDebuff(CardEffectData _effect)
    {
        foreach (TargetedData _target in targets)
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.cardID);
            _slot.Card.AddDebuff(_effect.debuffType, _effect.amount);
        }
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

    #region ClientRpc

    [ClientRpc]
    void InitEffect_ClientRpc(int _cardID, TargetedData[] _targets)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();

        VisualSpellEffectComponent[] _visuals = _entity.transform.GetComponentsInChildren<VisualSpellEffectComponent>(true);
        if (_visuals.Length > 0)
        {
            BaseCardData _data = CardManager.Instance.GetCard(_cardID);

            int _size = _visuals.Length;
            for (int _i = 0; _i < _size; _i++)
            {
                VisualSpellEffectComponent _visual = _visuals[_i];
                if (!_visual) continue;

                _visual.transform.position = _entity.transform.position;

                _visual.SetVisualAsset(_data.effect.effectAsset);

                if (_targets.Length < _i)
                    return;

                BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_targets[_i].cardOwnerTag, _targets[_i].cardID);
                _visual.SetDestination(_slot.transform.position);
            }
        }
    }

    #endregion
}
