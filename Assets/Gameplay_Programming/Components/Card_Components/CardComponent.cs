using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CardMovementComponent), typeof(CardOverlayComponent), typeof(CardFadeComponent))]
public class CardComponent : NetworkBehaviour
{
    public CardMovementComponent MovementComponent { get; private set; }
    public CardOverlayComponent OverlayComponent { get; private set; }
    public CardFadeComponent FadeComponent { get; private set; }

    [Header("Card Network Parameters")]
    [SerializeField] protected NetworkVariable<int> cardID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isHovered = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<bool> isSelected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] protected NetworkVariable<PlayerEnum> ownerTag = new NetworkVariable<PlayerEnum>(PlayerEnum.Player_NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Card Parameters")]
    [SerializeField] protected BaseCardData data;
    [SerializeField] protected bool isInteractable = true;

    #region Getters

    public int ID => cardID.Value;
    public bool IsHovered => isHovered.Value;
    public bool IsSelected => isSelected.Value;
    public BaseCardData Data => data;
    public PlayerEnum OwnerTag => ownerTag.Value;
    public bool IsInteractable => isInteractable;

    #endregion

    #region Setters

    public void Set(int _id, PlayerEnum _owner)
    {
        cardID.Value = _id;
        ownerTag.Value = _owner;
    }

    public void SetIsHovered(bool _value)
    {
        isHovered.Value = _value;
    }

    public void SetIsSelected(bool _value)
    {
        isSelected.Value = _value;
    }

    public void SetIsInteractable(bool _value)
    {
        isInteractable = _value;
    }

    #endregion

    protected virtual void Awake()
    {
        MovementComponent = GetComponent<CardMovementComponent>();
        OverlayComponent = GetComponent<CardOverlayComponent>();
        FadeComponent = GetComponent<CardFadeComponent>();
        ownerTag.OnValueChanged += (_oldVal, _newVal) => InitCard();
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
    }

    #endregion
}
