using System;
using Unity.Netcode;
using UnityEngine;

public class BoardCardComponent : CardComponent
{
    [Header("Board Card Network Parameters")]
    [SerializeField] NetworkVariable<bool> canAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Board Card Parameters")]
    [SerializeField] int attackAmount;
    [SerializeField] int healthAmount;

    public bool CanAttack => canAttack.Value;

    public void SetCanAttack(bool _value) => canAttack.Value = _value;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void InitCard()
    {
        base.InitCard();

        SoldierCardData _soldier = data as SoldierCardData;

        attackAmount = _soldier.attackAmount;
        healthAmount = _soldier.healthAmount;
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void AttackCard(BoardCardComponent _target)
    {
        _target.RemoveHealth_ClientRpc(attackAmount);

        RemoveHealth_ClientRpc(_target.attackAmount);
        SetCanAttack(false);

        CheckDeath();
    }

    [ClientRpc]
    void RemoveHealth_ClientRpc(int _amount)
    {
        healthAmount -= _amount;
        healthAmount = Mathf.Clamp(healthAmount, 0, int.MaxValue);
        OverlayComponent.UpdateHealth(healthAmount);
    }

    void CheckDeath()
    {
        if (healthAmount == 0)
        {
            NetworkObject _obj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (_obj.GetComponent<PlayerEntity>() is PlayerEntity _player)
            {
                _player.DestroyCard(ID);
            }
        }
    }
}
