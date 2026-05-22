using Unity.Netcode;
using UnityEngine;

public class BoardCardComponent : CardComponent
{
    [Header("Board Card Network Parameters")]
    [SerializeField] NetworkVariable<bool> canAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> attackAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> healthAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    SoldierCardData castedData;

    #region Getters

    public bool CanAttack => canAttack.Value;

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
        _localPlayer.InitCard(ID);
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

    #endregion

    #region Functions

    void UpdateHealthAmount(int _newAmount)
    {
        OverlayComponent.UpdateHealth(_newAmount);
        if (_newAmount == 0)
        {
            PlayerEntity _localPlayer = GameManager.Instance.GetLocalPlayer();
            PlayerEnum _ownerTag = GameManager.Instance.Board.GetOwnerOfCard(this);
            _localPlayer.DestroyCard(GameManager.Instance.Board.GetSlotIndex(this, _ownerTag),_ownerTag);
        }
    }

    #endregion
}
