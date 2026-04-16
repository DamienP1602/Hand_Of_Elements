using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CardMovementComponent))]
public class CardComponent : NetworkBehaviour
{
    public CardMovementComponent MovementComponent { get; private set; }

    [field:SerializeField] public bool IsHovered {  get; set; }

    private void Awake()
    {
        MovementComponent = GetComponent<CardMovementComponent>();
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
