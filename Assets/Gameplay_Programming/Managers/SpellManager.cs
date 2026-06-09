using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellManager : Singleton<SpellManager>
{
    [Serializable]
    public struct TargetedData : INetworkSerializable
    {
        public int slotID;
        public PlayerEnum cardOwnerTag;

        public TargetedData(int _id, PlayerEnum _tag)
        {
            slotID = _id;
            cardOwnerTag = _tag;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref slotID);
            serializer.SerializeValue(ref cardOwnerTag);
        }
    }

    [Header("Parameters")]
    [SerializeField] BaseCardData data;
    [SerializeField] CardComponent card;
    [SerializeField] PlayerEntity playerOwner;
    [SerializeField] VisualSpellEffectComponent emptyVisualEffect;
    [SerializeField] int slotIndex;
    [SerializeField] bool elementaryCombo = false;
    [SerializeField] int vfxIndexes = 0;

    [Header("Targets")]
    [SerializeField] List<TargetedData> targets = new();
    [SerializeField] PlayerEntity playerTarget;

    // Methods
    Dictionary<CardEffectData.CardEffectSelectionMode, Func<bool>> selectionDic = new();
    Dictionary<CardEffectData.CardEffectMode, Action<CardEffectData, int>> effectDic = new();
    Dictionary<CardEffectData.KeyEffect, Action<CardEffectData>> keyEffectDic = new();
    Dictionary<CardEffectData.CardEffectSelectionMode, Func<bool>> canLaunchDic = new();

    private void Start()
    {
        Init();
    }

    #region Init

    void Init()
    {
        // Selection
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.NoTarget, () => true);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, SetCardSelection);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Self, SelectSelf);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, SelectOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, SelectRandomOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, SelectAllOpponentSoldiers);

        // Effect
        effectDic.Add(CardEffectData.CardEffectMode.NONE, null);
        effectDic.Add(CardEffectData.CardEffectMode.Summon, SummonCard);
        effectDic.Add(CardEffectData.CardEffectMode.Heal, RestaureHealth);
        effectDic.Add(CardEffectData.CardEffectMode.InstantDamage, DealDamages);
        effectDic.Add(CardEffectData.CardEffectMode.Debuff, AddDebuff);
        effectDic.Add(CardEffectData.CardEffectMode.Draw, DrawCard);

        // Can Lauch
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.NoTarget, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, IsTargetStillAlive);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Self, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, OpponentHasSoldierLeft);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, OpponentHasSoldierLeft);

        // Key Effect
        keyEffectDic.Add(CardEffectData.KeyEffect.Overload, DiscardCard);
    }

    #endregion

    #region Server Functions

    #region Launch Effect

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffect(int _cardID, PlayerEnum _ownerType, bool _canInHand)
    {
        // Reset Targets
        targets.Clear();
        playerTarget = null;

        // Set Parameters
        playerOwner = GameManager.Instance.GetPlayer(_ownerType);
        if (_canInHand)
            card = playerOwner.HandComponent.GetCard(_cardID);
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_ownerType, _cardID);
            card = _slot.Card;
            slotIndex = _slot.GetSlotIndex;
        }
        elementaryCombo = false;
        data = card.Data;

        // Check for selection mode
        // If return true, it means we need to select something on board
        if (CheckSelectionMode(data.effect))
            return;

        int _targetNumber = targets.Count;
        if (_targetNumber > 0)
        {
            for (int _i = 0; _i < _targetNumber; _i++)
            {
                // Launch Effect
                CreateVisualEffect(_i);
            }
        }
        if (playerTarget || data.hasKeyEffect)
        {
            // Launch Effect
            CreateVisualEffect(0);
        }

        if (_canInHand)
        {
            playerOwner.RemoveArcane(data.cardCost);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffectSelection(int _selectedSlotID, PlayerEnum _ownerType)
    {
        // Set Targets
        targets.Add(new TargetedData(_selectedSlotID, _ownerType));

        // Launch Effect
        CreateVisualEffect(0);

        playerOwner.RemoveArcane(data.cardCost);

        // Stop Card Selection
        playerOwner.InteractComponent.SetSelectCard(false);
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Server Function
    /// </summary>
    void CreateVisualEffect(int _number)
    {
        vfxIndexes++;

        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();
        VisualSpellEffectComponent _visual = Instantiate(emptyVisualEffect, _entity.transform);
        _visual.NetworkObject.Spawn();
        _visual.NetworkObject.TrySetParent(_entity.transform, true);
        _visual.SetVfxIndex(_number);
        _entity.AddNewVfxIndex(vfxIndexes);
    }

    #endregion



    #region Effect

    /// <summary>
    /// Server Function
    /// </summary>
    public void ChooseEffectToCast(int _index)
    {
        CardEffectData _effect = elementaryCombo ? data.elementaryComboEffect : data.effect;
        CastEffect(_effect, _index);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    void CastEffect(CardEffectData _effect, int _index)
    {
        effectDic[_effect.effectMode]?.Invoke(_effect, _index);

        if (_effect.keyEffect != CardEffectData.KeyEffect.NONE)
        {
            keyEffectDic[_effect.keyEffect].Invoke(_effect);
        }

        if (data.hasElementaryCombo && !elementaryCombo)
        {
            if (playerOwner.LastElementPlayed == data.cardElement)
            {
                if (CanLaunchEffect(_effect))
                {
                    elementaryCombo = true;
                    CheckSelectionMode(data.elementaryComboEffect);

                    int _targetNumber = targets.Count;
                    if (_targetNumber > 0)
                    {
                        for (int _i = 0; _i < _targetNumber; _i++)
                        {
                            CreateVisualEffect(_i);
                        }
                    }
                    if (playerTarget)
                    {
                        CreateVisualEffect(0);
                    }
                    return;
                }
            }
        }

        if (data is SpellCardData)
        {
            playerOwner.SetElementCardPlayed(data.cardElement);
            DiscardCardAfterUse(playerOwner.PlayerTag);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void DiscardCardAfterUse(PlayerEnum _ownerTag)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_ownerTag);
        HandCardComponent _card = _player.HandComponent.GetSelectedSpell();
        int _index = _player.HandComponent.GetIndexOf(_card);
        _card.SetIsInteractable(false);
        _player.HandComponent.PutCardInDiscardPile_ClientRpc(_index);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public bool CanLaunchEffect(CardEffectData _effect)
    {
        return canLaunchDic[_effect.selectionMode].Invoke();
    }

    bool OpponentHasSoldierLeft()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _opponentCards = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);
        return _opponentCards.Count > 0;
    }

    bool IsTargetStillAlive()
    {
        if (targets.Count == 0) return false;

        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(targets[0].cardOwnerTag, targets[0].slotID);
        return !_slot.Card.IsDead;
    }

    void DiscardCard(CardEffectData _effect)
    {
        int _amount = _effect.keyEffectValue;
        for (int _i = 0; _i < _amount; _i++)
        {
            playerOwner.HandComponent.DiscardRandomCard();
        }
    }

    #endregion

    #region Effect Functions

    void DealDamages(CardEffectData _effect, int _index)
    {
        TargetedData _target = targets[_index];
        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
        _slot.Card.RemoveHealth(_effect.amount);

        if (playerTarget)
            playerTarget.LoseHealth(_effect.amount);
    }

    void RestaureHealth(CardEffectData _effect, int _index)
    {
        TargetedData _target = targets[_index];
        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
        _slot.Card.RemoveHealth(_effect.amount);

        if (playerTarget)
            playerTarget.RestaureHealth(_effect.amount);
    }

    void SummonCard(CardEffectData _effect, int _index)
    {
        PlayerEnum _ownerTag = playerTarget.PlayerTag;
        BoardSlotComponent _slot = GameManager.Instance.Board.GetFirstEmptySlot(_ownerTag);
        if (!_slot) return;

        _slot.PutCardInSlot(_slot.transform.position, _effect.cardReference.cardID);
    }

    void AddDebuff(CardEffectData _effect, int _index)
    {
        TargetedData _target = targets[_index];
        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
        _slot.Card.AddDebuff(_effect.debuffType, _effect.amount);
    }

    void DrawCard(CardEffectData _effect, int _index)
    {
        playerTarget.HandComponent.DrawCard(_effect.amount, _effect.specificElement);
        playerTarget.HandComponent.SetCardInHand_ClientRpc();
    }

    #endregion

    #region Selection Functions

    bool CheckSelectionMode(CardEffectData _effect)
    {
        return selectionDic[_effect.selectionMode].Invoke();
    }

    bool SetCardSelection()
    {
        playerOwner.InteractComponent.SetSelectCard(true);
        return true;
    }

    bool SelectSelf()
    {
        playerTarget = GameManager.Instance.GetPlayer(playerOwner.PlayerTag);
        return false;
    }

    bool SelectOpponent()
    {
        playerTarget = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        return false;
    }

    bool SelectRandomOpponent()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        BoardSlotComponent _slot = GameManager.Instance.Board.GetRandomCardOnBoard(_opponent.PlayerTag);
        if (_slot)
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
        return false;
    }

    bool SelectAllOpponentSoldiers()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _slots = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
        }
        return false;
    }

    #endregion

    #endregion

    #region ServerRpc

    [ServerRpc]
    public void InitEffect_ServerRpc(PlayerEnum _ownerType, int _vfxIndex)
    {
        InitEffect_ClientRpc(_ownerType, _vfxIndex, data.cardID, slotIndex, elementaryCombo, targets.ToArray());
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    public void InitEffect_ClientRpc(PlayerEnum _ownerType, int _vfxIndex, int _cardID, int _slotIndex, bool _elementaryCombo, TargetedData[] _targets)
    {
        BaseCardData _data = CardManager.Instance.GetCard(_cardID);
        CardEffectData _effect = _elementaryCombo ? _data.elementaryComboEffect : _data.effect;
        PlayerEntity _entity = GameManager.Instance.GetPlayer(_ownerType);
        Vector3 _endPos = Vector3.zero;
        Vector3 _startPos = Vector3.zero;
        TargetedData _target = new TargetedData();
        if (_targets.Length > 0)
            _target = _targets[_vfxIndex];

        if (_data is SpellCardData)
        {
            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target) + Vector3.up * 0.5f;
            _startPos = _entity.HandComponent.SelectedPosition;
        }
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_ownerType, _slotIndex);
            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target) + Vector3.up * 0.5f;
            _startPos = _slot.Card.transform.position;
        }
        InitVisualEffect(_data, _endPos, _startPos, _vfxIndex, _elementaryCombo, _ownerType);
    }

    #endregion

    #region Functions

    void InitVisualEffect(BaseCardData _data, Vector3 _endPos, Vector3 _startPos, int _vfxIndex, bool _comboEffect, PlayerEnum _ownerType)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayer(_ownerType);
        VisualSpellEffectComponent[] _visuals = _entity.transform.GetComponentsInChildren<VisualSpellEffectComponent>(true);

        foreach (VisualSpellEffectComponent _visualEffect in _visuals)
        {
            if (_visualEffect.GetVfxIndex == _vfxIndex)
            {
                CardEffectData _effectData = _comboEffect ? _data.elementaryComboEffect : _data.effect;

                _visualEffect.SetVisualAsset(_effectData.effectAsset);

                if (_effectData.isInstantEffect)
                {
                    _visualEffect.transform.position = _endPos;
                    _visualEffect.SetTime(1.0f);
                    if (IsServer)
                        ChooseEffectToCast(_visualEffect.GetVfxIndex);

                    return;
                }
                else
                {
                    _visualEffect.transform.position = _startPos;
                    _visualEffect.SetDestination(_endPos);
                }

                if (IsServer)
                    _visualEffect.SetAction(ChooseEffectToCast);
            }
        }
    }

    #region Positions

    Vector3 GetEndPosFromEffect(CardEffectData _data, PlayerEnum _playerTag, TargetedData _target)
    {
        switch (_data.effectMode)
        {
            case CardEffectData.CardEffectMode.Summon:
                return GetPosFromSummon(_playerTag);
            case CardEffectData.CardEffectMode.Heal:
                return GetTargetPos(_target);
            case CardEffectData.CardEffectMode.InstantDamage:
                return GetTargetPos(_target);
            case CardEffectData.CardEffectMode.Debuff:
                return GetTargetPos(_target);
            case CardEffectData.CardEffectMode.Draw:
                return GetDeckPos(_playerTag);
        }

        return Vector3.zero;
    }

    Vector3 GetPosFromSummon(PlayerEnum _ownerTag)
    {
        BoardSlotComponent _slot = GameManager.Instance.Board.GetFirstEmptySlot(_ownerTag);
        return _slot.transform.position + _slot.CardPosition + Vector3.up * 0.25f;
    }

    Vector3 GetTargetPos(TargetedData _target)
    {
        BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_target.cardOwnerTag, _target.slotID);
        return _slot.transform.position + Vector3.up * 0.25f;
    }

    Vector3 GetDeckPos(PlayerEnum _ownerTag)
    {
        return GameManager.Instance.Board.GetDeckPosition(_ownerTag);
    }

    #endregion

    #endregion
}
