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
    Dictionary<CardEffectData.CardEffectSelectionMode, Action> selectionDic = new();
    Dictionary<CardEffectData.CardEffectMode, Action<CardEffectData,int>> effectDic = new();
    Dictionary<CardEffectData.CardEffectSelectionMode, Func<bool>> canLaunchDic = new();

    private void Start()
    {
        Init();
    }

    #region Init

    void Init()
    {
        // Selection
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, SetCardSelection);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Self, SelectSelf);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, SelectOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, SelectRandomOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, SelectAllOpponentSoldiers);

        // Effect
        effectDic.Add(CardEffectData.CardEffectMode.Summon, SummonCard);
        effectDic.Add(CardEffectData.CardEffectMode.Heal, RestaureHealth);
        effectDic.Add(CardEffectData.CardEffectMode.InstantDamage, DealDamages);
        effectDic.Add(CardEffectData.CardEffectMode.Debuff, AddDebuff);
        effectDic.Add(CardEffectData.CardEffectMode.Draw, DrawCard);

        // Can Lauch
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.NoTarget, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Self, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, OpponentHasSoldierLeft);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, OpponentHasSoldierLeft);
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
        if (_canInHand)
            card = playerOwner.HandComponent.GetSelectedCard();
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_ownerType, _cardID);
            card = _slot.Card;
            slotIndex = _slot.GetSlotIndex;
        }
        playerOwner = GameManager.Instance.GetPlayer(_ownerType);
        elementaryCombo = false;
        data = card.Data;


        // Check for selection mode
        CheckSelectionMode(data.effect);

        int _targetNumber = targets.Count;
        if (_targetNumber > 0)
        {
            for (int _i = 0; _i < _targetNumber; _i++)
            {
                // Launch Effect
                CreateVisualEffect(_i);
            }
        }
        if (playerTarget)
        {
            // Launch Effect
            CreateVisualEffect(0);
        }

        if (_canInHand)
            playerOwner.HandComponent.RemoveSelectedCard();
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffectSelection(int _selectedSlotID, PlayerEnum _ownerType)
    {
        // Stop Card Selection
        playerOwner.InteractComponent.SetSelectCard(false);

        // Set Targets
        targets.Add(new TargetedData(_selectedSlotID, _ownerType));

        // Launch Effect
        CreateVisualEffect(0);
        playerOwner.HandComponent.RemoveSelectedCard();
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
        CastEffect(_effect,_index);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    void CastEffect(CardEffectData _effect,int _index)
    {
        effectDic[_effect.effectMode]?.Invoke(_effect, _index);

        if (data.hasElementaryCombo && !elementaryCombo)
        {
            if (playerOwner.LastElementPlayed == data.cardElement)
            {
                if (CanLaunchEffect(_effect))
                {
                    elementaryCombo = true;
                    CheckSelectionMode(data.elementaryComboEffect);

                    int _targetNumber = targets.Count;
                    if (_targetNumber > 0 || playerTarget)
                    {
                        for (int _i = 0; _i < _targetNumber; _i++)
                        {
                            // Launch Effect
                            CreateVisualEffect(_i);
                        }
                    }
                    return;
                }
            }
        }

        if (data is SpellCardData)
        {
            playerOwner.RemoveArcane(data.cardCost);
        }
        playerOwner.SetElementCardPlayed(data.cardElement);
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

    #endregion

    #region Effect Functions

    void DealDamages(CardEffectData _effect,int _index)
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

    void SummonCard(CardEffectData _effect,int _index)
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

    void CheckSelectionMode(CardEffectData _effect)
    {
        selectionDic[_effect.selectionMode]?.Invoke();
    }

    void SetCardSelection()
    {
        playerOwner.InteractComponent.SetSelectCard(true);
    }

    void SelectSelf()
    {
        playerTarget = GameManager.Instance.GetPlayer(playerOwner.PlayerTag);
    }

    void SelectOpponent()
    {
        playerTarget = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
    }

    void SelectRandomOpponent()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        BoardSlotComponent _slot = GameManager.Instance.Board.GetRandomCardOnBoard(_opponent.PlayerTag);
        if (_slot)
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
    }

    void SelectAllOpponentSoldiers()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _slots = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
        }
    }

    #endregion

    #endregion

    #region ServerRpc

    [ServerRpc]
    public void InitEffect_ServerRpc(PlayerEnum _ownerType, int _vfxIndex)
    {
        InitEffect_ClientRpc(_ownerType, _vfxIndex, data.cardID, targets.ToArray());
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    public void InitEffect_ClientRpc(PlayerEnum _ownerType, int _vfxIndex, int _cardID, TargetedData[] _targets)
    {
        BaseCardData _data = CardManager.Instance.GetCard(_cardID);
        CardEffectData _effect = elementaryCombo ? _data.elementaryComboEffect : _data.effect;
        PlayerEntity _entity = GameManager.Instance.GetPlayer(_ownerType);
        bool _isOnBoard = false;
        Vector3 _endPos = Vector3.zero;
        Vector3 _startPos = Vector3.zero;
        TargetedData _target = new TargetedData();
        if (_targets.Length > 0)
            _target = _targets[_vfxIndex];

        if (_data is SpellCardData)
        {
            _isOnBoard = false;
            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target) + Vector3.up * 0.5f;
            _startPos = _entity.transform.position;
        }
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_ownerType, slotIndex);
            _isOnBoard = true;

            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target) + Vector3.up * 0.5f;
            _startPos = _slot.Card.transform.position;
        }
        InitVisualEffect(_data, _isOnBoard, _endPos, _startPos, _vfxIndex, elementaryCombo, _ownerType);
    }

    #endregion

    #region Functions

    void InitVisualEffect(BaseCardData _data, bool _isOnBoard, Vector3 _endPos, Vector3 _startPos, int _vfxIndex, bool _comboEffect, PlayerEnum _ownerType)
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
        return _slot.transform.position + _slot.CardPosition + Vector3.up * 0.1f;
    }

    Vector3 GetTargetPos(TargetedData _target)
    {
        BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_target.cardOwnerTag, _target.slotID);
        return _slot.transform.position + Vector3.up * 0.1f;
    }

    Vector3 GetDeckPos(PlayerEnum _ownerTag)
    {        
        return GameManager.Instance.Board.GetDeckPosition(_ownerTag) + Vector3.back * 1.5f;
    }

    #endregion

    #endregion
}
