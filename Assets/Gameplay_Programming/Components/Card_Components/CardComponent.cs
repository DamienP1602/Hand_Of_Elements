using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CardMovementComponent), typeof(CardOverlayComponent))]
public class CardComponent : NetworkBehaviour
{
    public CardMovementComponent MovementComponent { get; private set; }
    public CardOverlayComponent OverlayComponent { get; private set; }

    [SerializeField] protected NetworkVariable<int> cardID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isHovered = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isSelected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] protected BaseCardData data;

    public int ID => cardID.Value;
    public bool IsHovered => isHovered.Value;
    public bool IsSelected => isSelected.Value;

    protected virtual void Awake()
    {
        MovementComponent = GetComponent<CardMovementComponent>();
        OverlayComponent = GetComponent<CardOverlayComponent>();

        cardID.OnValueChanged += (_oldVal, _newVal) => InitCard();
    }

    public void SetID(int _id)
    {
        cardID.Value = _id;
    }

    public void SetIsHovered(bool _value)
    {
        isHovered.Value = _value;
    }

    public void SetIsSelected(bool _value)
    {
        isSelected.Value = _value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitCard()
    {
        data = CardManager.Instance.GetCard(ID);
        OverlayComponent.SetData(data);
    }
}
