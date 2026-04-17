using Unity.Netcode;
using UnityEngine;

public class HandCardComponent : CardComponent
{
    [field:SerializeField] public bool IsSelected { get; set; }
    [SerializeField] NetworkVariable<int> cardID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void SetID(int _id)
    {
        cardID.Value = _id;
        data = CardManager.Instance.GetCard(_id);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
