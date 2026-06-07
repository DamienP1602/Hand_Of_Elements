using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoardCardComponent : CardComponent
{
    [Serializable]
    public struct CardBoardEffect : INetworkSerializable
    {
        public DebuffType type;
        public int amount;

        public CardBoardEffect(DebuffType _type, int _amount)
        {
            type = _type;
            amount = _amount;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref amount);
        }
    }

    [Header("Board Card Network Parameters")]
    [SerializeField] NetworkVariable<bool> canAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> attackAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> healthAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<List<CardBoardEffect>> cardDebuffs = new NetworkVariable<List<CardBoardEffect>>(new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    SoldierCardData castedData;

    #region Getters

    public bool CanAttack => canAttack.Value;
    public bool HasDebuff(DebuffType _debuff)
    {
        foreach (CardBoardEffect _effect in cardDebuffs.Value)
        {
            if (_effect.type == _debuff)
                return true;
        }
        return false;
    }
    public bool IsDead => healthAmount.Value == 0;
    public int GetAttack => attackAmount.Value;


    #endregion

    #region Setters

    public void SetCanAttack(bool _value) => canAttack.Value = _value;

    #endregion

    void Start()
    {
        healthAmount.OnValueChanged += (_old, _new) => UpdateHealthAmount(_new);
        castedData = data as SoldierCardData;
    }

    void Update()
    {

    }

    #region Inits

    public override void InitCard()
    {
        base.InitCard();

        PlayerEntity _localPlayer = GameManager.Instance.GetLocalPlayer();
        int _slotIndex = GameManager.Instance.Board.GetSlotIndex(this, ownerTag.Value);
        _localPlayer.InitCard(_slotIndex, ownerTag.Value);
        FadeComponent.SetFade(CardFadeComponent.FadeStatus.FadeIn);
    }

    public void InitStats()
    {
        if (!castedData)
            castedData = data as SoldierCardData;

        attackAmount.Value = castedData.attackAmount;
        healthAmount.Value = castedData.healthAmount;
    }

    #endregion

    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void AttackCard(BoardCardComponent _target)
    {
        _target.RemoveHealth(attackAmount.Value);

        RemoveHealth(_target.attackAmount.Value);
        SetCanAttack(false);
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void RemoveHealth(int _amount)
    {
        int _newValue = healthAmount.Value - _amount;
        _newValue = Mathf.Clamp(_newValue, 0, castedData.healthAmount);
        healthAmount.Value = _newValue;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void RestaureHealth(int _amount)
    {
        int _newValue = healthAmount.Value + _amount;
        _newValue = Mathf.Clamp(_newValue, 0, castedData.healthAmount);
        healthAmount.Value = _newValue;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void AddDebuff(DebuffType _debuff, int _amount)
    {
        cardDebuffs.Value.Add(new CardBoardEffect(_debuff,_amount));
    }

    /// <summary>
    /// Server Fuction
    /// </summary>
    public void TakeDamageFromBurn()
    {
        foreach (CardBoardEffect _debuff in cardDebuffs.Value)
        {
            if (_debuff.type == DebuffType.BurnToken)
                RemoveHealth(_debuff.amount * 10);
        }
        cardDebuffs.Value.Clear();
    }

    #endregion

    #region Functions

    void UpdateHealthAmount(int _newAmount)
    {
        OverlayComponent.UpdateHealth(_newAmount);
        if (_newAmount == 0)
        {
            PlayerEntity _localPlayer = GameManager.Instance.GetLocalPlayer();
            PlayerEnum _ownerTag = GameManager.Instance.Board.GetOwnerOfCard(this);
            _localPlayer.DestroyCard(GameManager.Instance.Board.GetSlotIndex(this, _ownerTag), _ownerTag);
        }
    }

    #endregion
}
