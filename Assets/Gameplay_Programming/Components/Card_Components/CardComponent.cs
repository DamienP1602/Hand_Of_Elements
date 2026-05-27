using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CardMovementComponent), typeof(CardOverlayComponent))]
public class CardComponent : NetworkBehaviour
{
    public CardMovementComponent MovementComponent { get; private set; }
    public CardOverlayComponent OverlayComponent { get; private set; }

    [Header("Card Network Parameters")]
    [SerializeField] protected NetworkVariable<int> cardID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isHovered = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isSelected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Card Parameters")]
    [SerializeField] protected BaseCardData data;

    public int ID => cardID.Value;
    public bool IsHovered => isHovered.Value;
    public bool IsSelected => isSelected.Value;
    public BaseCardData Data => data;

    #region Setters

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

    #endregion

    protected virtual void Awake()
    {
        MovementComponent = GetComponent<CardMovementComponent>();
        OverlayComponent = GetComponent<CardOverlayComponent>();

        cardID.OnValueChanged += (_oldVal, _newVal) => InitCard();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    #region Inits

    public virtual void InitCard()
    {
        data = CardManager.Instance.GetCard(ID);
        OverlayComponent.SetData(data);
        OverlayComponent.SetColorFromType();
    }

    #endregion
}
