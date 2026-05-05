using Unity.Netcode;
using UnityEngine;

public class BoardCardComponent : CardComponent
{
    [SerializeField] NetworkVariable<bool> canAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
}
