using NUnit.Framework.Internal;
using System;
using System.Collections;
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
    [SerializeField] List<BaseCardData> nextEffectToPlay = new();

    [Header("Time Parameters")]
    [SerializeField] float timeToWait = 0.0f;
    [SerializeField] bool needToWaitTime = false;

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

    private void Update()
    {

    }

    #region Init

    void Init()
    {
        // Selection
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.NoTarget, () => false);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, SetCardSelection);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Self, SelectSelf);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, SelectOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, SelectRandomOpponent);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, SelectAllOpponentSoldiers);
        selectionDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponents, SelectAllOpponents);

        // Effect
        effectDic.Add(CardEffectData.CardEffectMode.NONE, null);
        effectDic.Add(CardEffectData.CardEffectMode.Summon, SummonCard);
        effectDic.Add(CardEffectData.CardEffectMode.Heal, RestaureHealth);
        effectDic.Add(CardEffectData.CardEffectMode.InstantDamage, DealDamages);
        effectDic.Add(CardEffectData.CardEffectMode.Debuff, AddDebuff);
        effectDic.Add(CardEffectData.CardEffectMode.Draw, DrawCard);
        effectDic.Add(CardEffectData.CardEffectMode.RestaureArcane, RestaureArcane);
        effectDic.Add(CardEffectData.CardEffectMode.GainArcane, GainArcane);

        // Can Lauch
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.NoTarget, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.SingleTarget, IsTargetStillAlive);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Self, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.Opponent, () => true);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.RandomOpponent, OpponentHasSoldierLeft);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponentSoldier, OpponentHasSoldierLeft);
        canLaunchDic.Add(CardEffectData.CardEffectSelectionMode.AllOpponents, () => true);

        // Key Effect
        keyEffectDic.Add(CardEffectData.KeyEffect.Overload, DiscardCard);
        keyEffectDic.Add(CardEffectData.KeyEffect.Etherial, null);
    }

    #endregion

    #region Server Functions

    #region Launch Effect

    /// <summary>
    /// Server Function
    /// </summary>
    public IEnumerator LaunchEffect(int _cardID, PlayerEnum _ownerType, bool _canInHand)
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

        // Play Keyword Effect
        if (data.keyEffect != CardEffectData.KeyEffect.NONE)
            keyEffectDic[data.keyEffect]?.Invoke(data.effect);
        // If you need to wait after he keyword effect => wait for X seconds
        if (needToWaitTime)
        {
            yield return new WaitForSeconds(timeToWait);
            needToWaitTime = false;
        }

        // Check for selection mode
        // If return true, it means we need to select something on board
        if (CheckSelectionMode(data.effect))
            yield break;

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
            CreateVisualEffect(-1);
        }

        if (_canInHand)
        {
            playerOwner.RemoveCurrentArcane(data.cardCost);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public IEnumerator LaunchEffectSelection(int _selectedSlotID, PlayerEnum _ownerType)
    {
        // Set Targets
        targets.Add(new TargetedData(_selectedSlotID, _ownerType));

        // Launch Effect
        CreateVisualEffect(0);

        playerOwner.RemoveCurrentArcane(data.cardCost);

        // Stop Card Selection
        playerOwner.InteractComponent.SetSelectCard(false);

        // Play Keyword Effect
        if (data.keyEffect != CardEffectData.KeyEffect.NONE)
            keyEffectDic[data.keyEffect].Invoke(data.effect);
        // If you need to wait after he keyword effect => wait for X seconds
        if (needToWaitTime)
        {
            yield return new WaitForSeconds(timeToWait);
            needToWaitTime = false;
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Server Function
    /// </summary>
    void CreateVisualEffect(int _number)
    {
        PlayerEntity _entity = GameManager.Instance.GetPlayerFromTurn();
        VisualSpellEffectComponent _visual = Instantiate(emptyVisualEffect, _entity.transform);
        _visual.NetworkObject.Spawn();
        _visual.NetworkObject.TrySetParent(_entity.transform, true);
        _visual.SetVfxIndex(_number);
    }

    #endregion

    #region Effect

    /// <summary>
    /// Server Function
    /// </summary>
    IEnumerator ChooseEffectToCast(int _index)
    {
        CardEffectData _effect = elementaryCombo ? data.elementaryComboEffect : data.effect;
        yield return StartCoroutine(CastEffect(_effect, _index));
    }

    IEnumerator CastEffect(CardEffectData _effect, int _index)
    {
        // If there's a unique effect, trigger it before any executions
        if (data.hasUniqueEffect)
            data.uniqueEffectData.ExecuteEffect(card);

        // Play Effect
        effectDic[_effect.effectMode]?.Invoke(_effect, _index);
        // If you need to wait after the effect => wait for X seconds
        if (needToWaitTime)
        {
            yield return new WaitForSeconds(timeToWait);
            needToWaitTime = false;
        }

        // If the card has an elementary combo AND we're not during the elementary combo
        if (data.hasElementaryCombo && !elementaryCombo)
        {
            // If the last played card is the same element as this card
            if (playerOwner.LastElementPlayed == data.cardElement)
            {
                // Check if we can still launch the effect
                if (CanLaunchEffect(_effect))
                {
                    // Activate elementary combo
                    elementaryCombo = true;
                    // Set the new Target(s)
                    CheckSelectionMode(data.elementaryComboEffect);

                    // Launch visual effect for each targets
                    int _targetNumber = targets.Count;
                    if (_targetNumber > 0)
                    {
                        for (int _i = 0; _i < _targetNumber; _i++)
                        {
                            CreateVisualEffect(_i);
                        }
                    }
                    // Launch single visual effect if the target is a player
                    if (playerTarget)
                    {
                        CreateVisualEffect(-1);
                    }
                    yield break;
                }
            }
        }

        // If the card played is a spell, set the last played element and discard the played card
        if (data is SpellCardData)
        {
            playerOwner.SetElementCardPlayed(data.cardElement);
            DiscardCardAfterUse(playerOwner.PlayerTag);
        }

        if (nextEffectToPlay.Count > 0)
            TriggerNextEffect();

        yield break;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void DiscardCardAfterUse(PlayerEnum _ownerTag)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_ownerTag);
        HandCardComponent _card = _player.HandComponent.GetSelectedSpell();
        if (_card.Data.keyEffect == CardEffectData.KeyEffect.Etherial)
        {
            _player.DeckComponent.AddCardInDeck(playerOwner,_card,true);
        }
        else
        {
            int _index = _player.HandComponent.GetIndexOf(_card);
            _card.SetIsInteractable(false);
            _player.HandComponent.PutCardInDiscardPile_ClientRpc(_index);
        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public bool CanLaunchEffect(CardEffectData _effect)
    {
        return canLaunchDic[_effect.selectionMode].Invoke();
    }

    /// <summary>
    /// Server Function
    /// </summary>
    bool OpponentHasSoldierLeft()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _opponentCards = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);
        return _opponentCards.Count > 0;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    bool IsTargetStillAlive()
    {
        if (targets.Count == 0) return false;

        BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(targets[0].cardOwnerTag, targets[0].slotID);
        return !_slot.Card.IsDead;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    void SetWaitTime(float _time)
    {
        needToWaitTime = true;
        timeToWait = _time;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void SetNextEffect(BaseCardData _data)
    {
        nextEffectToPlay.Add(_data);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    void TriggerNextEffect()
    {
        BaseCardData _next = nextEffectToPlay[0];
        nextEffectToPlay.RemoveAt(0);

        data = _next;
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
            CreateVisualEffect(-1);
        }
    }

    #endregion

    #region Effect Functions

    void DealDamages(CardEffectData _effect, int _index)
    {
        if (_index >= 0)
        {
            TargetedData _target = targets[_index];
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
            _slot.Card.RemoveHealth(_effect.amount);
            
        }
        else if (playerTarget)
            playerTarget.LoseHealth(_effect.amount);

        SetWaitTime(0.2f);
    }

    void RestaureHealth(CardEffectData _effect, int _index)
    {
        if (_index >= 0)
        {
            TargetedData _target = targets[_index];
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
            _slot.Card.RemoveHealth(_effect.amount);
        }
        else if (playerTarget)
            playerTarget.RestaureHealth(_effect.amount);

        SetWaitTime(0.5f);
    }

    void SummonCard(CardEffectData _effect, int _index)
    {
        PlayerEnum _ownerTag = playerTarget.PlayerTag;
        BoardSlotComponent _slot = GameManager.Instance.Board.GetFirstEmptySlot(_ownerTag);
        if (!_slot) return;

        _slot.PutCardInSlot(_slot.transform.position, _effect.cardReference.cardID);

        SetWaitTime(1.0f);
    }

    void AddDebuff(CardEffectData _effect, int _index)
    {
        if (_index >= 0)
        {
            TargetedData _target = targets[_index];
            BoardSlotComponent _slot = GameManager.Instance.Board.GetSlot(_target.cardOwnerTag, _target.slotID);
            _slot.Card.AddDebuff(_effect.debuffType, _effect.amount);
        }

        SetWaitTime(0.5f);
    }

    void DrawCard(CardEffectData _effect, int _index)
    {
        playerTarget.HandComponent.DrawCard(_effect.amount, _effect.specificElement, _effect.specificKeyEffect);
        playerTarget.HandComponent.SetCardInHand_ClientRpc();

        SetWaitTime(0.5f);
    }

    void RestaureArcane(CardEffectData _effect,int _index)
    {
        playerTarget.AddArcaneForThisTurn(_effect.amount);

        SetWaitTime(0.2f);
    }

    void GainArcane(CardEffectData _effect, int _index)
    {
        playerTarget.AddArcaneAmount(_effect.amount);

        SetWaitTime(0.2f);
    }

    void DiscardCard(CardEffectData _effect)
    {
        int _amount = data.keyEffectValue;
        for (int _i = 0; _i < _amount; _i++)
        {
            playerOwner.HandComponent.DiscardRandomCard();
        }
        playerOwner.AddToOverloadAmount(_amount);
        SetWaitTime(1.0f);
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

        return _slot == null;
    }

    bool SelectAllOpponentSoldiers()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _slots = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
        }
        return _slots.Count == 0;
    }

    bool SelectAllOpponents()
    {
        PlayerEntity _opponent = GameManager.Instance.GetOtherPlayer(playerOwner.PlayerTag);
        List<BoardSlotComponent> _slots = GameManager.Instance.Board.GetAllSlotCards(_opponent.PlayerTag);

        foreach (BoardSlotComponent _slot in _slots)
        {
            targets.Add(new TargetedData(_slot.GetSlotIndex, _opponent.PlayerTag));
        }
        playerTarget = _opponent;

        return false;
    }

    #endregion

    #endregion

    #region ServerRpc

    [ServerRpc]
    public void InitEffect_ServerRpc(PlayerEnum _ownerType, int _vfxIndex)
    {
        InitEffect_ClientRpc(_ownerType, _vfxIndex, data.cardID, slotIndex, elementaryCombo, targets.ToArray(), playerTarget ? playerTarget.PlayerTag : PlayerEnum.Player_NONE);
    }

    #endregion

    #region ClientRpc

    [ClientRpc]
    public void InitEffect_ClientRpc(PlayerEnum _ownerType, int _vfxIndex, int _cardID, int _slotIndex, bool _elementaryCombo, TargetedData[] _targets, PlayerEnum _playerTarget)
    {
        BaseCardData _data = CardManager.Instance.GetCard(_cardID);
        CardEffectData _effect = _elementaryCombo ? _data.elementaryComboEffect : _data.effect;
        PlayerEntity _entity = GameManager.Instance.GetPlayer(_ownerType);
        Vector3 _endPos = Vector3.zero;
        Vector3 _startPos = Vector3.zero;
        TargetedData? _target = null;
        if (_vfxIndex >= 0)
            _target = _targets[_vfxIndex];

        if (_data is SpellCardData)
        {
            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target, _playerTarget);
            _startPos = _entity.HandComponent.SelectedPosition;
        }
        else
        {
            BoardSlotComponent _slot = GameManager.Instance.Board.GetCardFromCardID(_ownerType, _slotIndex);
            _endPos = GetEndPosFromEffect(_effect, _ownerType, _target, _playerTarget);
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
                    _visualEffect.SetTime(_effectData.effectTime);
                }
                else
                {
                    _visualEffect.transform.position = _startPos;
                    _visualEffect.SetDestination(_endPos + Vector3.up * 0.5f);
                }

                if (IsServer)
                    _visualEffect.SetAction(ChooseEffectToCast);
            }
        }
    }

    #region Positions

    Vector3 GetEndPosFromEffect(CardEffectData _data, PlayerEnum _playerTag, TargetedData? _target, PlayerEnum _playerTarget)
    {
        switch (_data.effectMode)
        {
            case CardEffectData.CardEffectMode.Summon:
                return GetPosFromSummon(_playerTag);
            case CardEffectData.CardEffectMode.Heal:
                return GetTargetPos(_target.Value);
            case CardEffectData.CardEffectMode.InstantDamage:
                {
                    if (_target == null)
                        return GetPlayerPos(_playerTarget);
                    else
                        return GetTargetPos(_target.Value);
                }
            case CardEffectData.CardEffectMode.Debuff:
                return GetTargetPos(_target.Value);
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

    Vector3 GetPlayerPos(PlayerEnum _playerTarget)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_playerTarget);
        return _player.PortraitComponent.transform.position;
    }

    #endregion

    #endregion
}
