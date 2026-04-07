using Unity.Netcode;
using UnityEngine;

public class HandCardComponent : NetworkBehaviour
{
    [SerializeField] Card data;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Card _card)
    {
        data = _card;
    }
}
