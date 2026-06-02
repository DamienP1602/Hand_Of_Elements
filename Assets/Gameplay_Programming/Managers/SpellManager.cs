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
    [SerializeField] int slotIndex;
    [SerializeField] bool elementaryCombo = false;
    [SerializeField] int vfxIndexes = 0;

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
        elementaryCombo = false;

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

        // Create visual Effect
        CreateVisualEffect();

        // todo move after 
        // Remove card from Hand
        if (card.Data is SpellCardData)
        {
            playerOwner.HandComponent.RemoveSelectedCard();
            playerOwner.RemoveArcane(card.Data.cardCost);
        }

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

        // Create visual Effect
        CreateVisualEffect();

        // todo move after 
        // Remove card from Hand if it's a Spell
        if (card.Data is SpellCardData)
        {
            playerOwner.HandComponent.RemoveSelectedCard();
            playerOwner.RemoveArcane(card.Data.cardCost);
        }
    }

    public void LaunchSoldierEffect(int _slotID,PlayerEnum _ownerType)
    {
        // Reset Targets
        targets.Clear();
        playerTarget = null;

        // Set Parameters
        playerOwner = GameManager.Instance.GetPlayer(_ownerType);
        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_ownerType, _slotID);
        card = _slot.Card;
        slotIndex = _slot.GetSlotIndex;
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

        // If it has a visual effect, it will create and launch it to the target(s)
        if (currentEffect.effectAsset != null)
            LaunchProjectile();
        // If not, it will cast the spell instantly
        else
            CastEffect();
    }


    /// <summary>
    /// Server Function
    /// </summary>
    void CreateVisualEffect()
    {
        vfxIndexes++;
        PlayerEntity _player = GameManager.Instance.GetPlayer(card.OwnerTag);
        
        VisualSpellEffectComponent _visualEffect = Instantiate(emptyVisualEffect);
        _visualEffect.NetworkObject.Spawn();
        _visualEffect.NetworkObject.TrySetParent(_player.transform);
        _player.AddNewVfxIndex(vfxIndexes);
    }



    /// <summary>
    /// Server Function
    /// </summary>
    void CastEffect()
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
                elementaryCombo = true;
                LaunchProjectile();
            }
        }
        playerOwner.SetElementCardPlayed(card.Data.cardElement);
    }

    void CastComboEffect()
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

    /// <summary>
    /// Server Function
    /// </summary>
    public bool CanLaunchEffect(CardEffectData _effect)
    {
        switch (_effect.selectionMode)
        {
            case CardEffectData.CardEffectSelectionMode.NoTarget:
                return true;
            case CardEffectData.CardEffectSelectionMode.SingleTarget:
                return true;
            case CardEffectData.CardEffectSelectionMode.Self:
                return true;
            case CardEffectData.CardEffectSelectionMode.Opponent:
                return true;
        }

        return false;
    }

    void LaunchProjectile()
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();
        VisualSpellEffectComponent _visual = Instantiate(emptyVisualEffect, _entity.transform);
        _visual.NetworkObject.Spawn();
        _visual.NetworkObject.TrySetParent(_entity.transform, true);
        Invoke(nameof(InitProjectileEffect), 0.1f);
    }

    void InitProjectileEffect()
    {
        int _slotIndex = card.Data is SoldierCardData ? slotIndex : 0;
        InitEffect_ClientRpc(card.Data.cardID, targets.ToArray(), _slotIndex);
    }

    void CreateStandingEffect(Vector3 _pos)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();
        VisualSpellEffectComponent _visual = Instantiate(emptyVisualEffect, _entity.transform);
        _visual.NetworkObject.Spawn();
        _visual.NetworkObject.TrySetParent(_entity.transform, true);

        InitStandingEffect_ClientRpc(card.Data.cardID, _pos);
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

        //CreateStandingEffect();
        VisualSpellEffectComponent _visualEffect = Instantiate(emptyVisualEffect, playerOwner.transform);
        _visualEffect.NetworkObject.Spawn();
        _visualEffect.NetworkObject.TrySetParent(playerOwner.transform,true);
        _visualEffect.SetVisualAsset(_effect.effectAsset);
        _visualEffect.transform.position = _slot.Card.transform.position;
        Destroy(_visualEffect, 1.0f);
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

    #region ServerRpc



    #endregion

    #region ClientRpc

    [ClientRpc]
    void InitEffect_ClientRpc(int _cardID, TargetedData[] _targets, int _slotOfCard)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();

        VisualSpellEffectComponent[] _visuals = _entity.transform.GetComponentsInChildren<VisualSpellEffectComponent>(true);
        if (_visuals.Length > 0)
        {
            BaseCardData _data = CardManager.Instance.GetCard(_cardID);
            CardEffectData _effect = elementaryCombo ? _data.elementaryComboEffect : _data.effect;

            int _size = _visuals.Length;
            for (int _i = 0; _i < _size; _i++)
            {
                VisualSpellEffectComponent _visual = _visuals[_i];
                if (!_visual) continue;

                _visual.SetVisualAsset(_effect.effectAsset);
                if (!_effect.isInstantEffect)
                {
                    if (_data is SpellCardData)
                        _visual.transform.position = _entity.transform.position;
                    else
                    {
                        BoardSlotComponent _cardSlot = GameManager.Instance.Board.GetSlot(_entity.PlayerTag, _slotOfCard);
                        _visual.transform.position = _cardSlot.Card.transform.position;
                    }

                    if (_targets.Length < _i)
                        return;

                    BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_targets[_i].cardOwnerTag, _targets[_i].cardID);
                    Vector3 _destination = _slot.IsEmpty ? _slot.transform.position : _slot.Card.transform.position;
                    _visual.SetDestination(_destination);

                    _visual.SetAction(elementaryCombo ? CastComboEffect : CastEffect);
                }
            }
        }


    }

    [ClientRpc]
    void InitStandingEffect_ClientRpc(int _cardID,Vector3 _position)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();

        VisualSpellEffectComponent[] _visuals = _entity.transform.GetComponentsInChildren<VisualSpellEffectComponent>(true);
    }

    #endregion
}
