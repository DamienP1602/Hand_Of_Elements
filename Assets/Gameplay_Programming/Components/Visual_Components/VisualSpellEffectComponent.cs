using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class VisualSpellEffectComponent : NetworkBehaviour
{
    [field: SerializeField] public VisualEffect VisualEffect { get; private set; }

    [Header("Movement Parameters")]
    [SerializeField] Vector3 destination;
    [SerializeField] AnimationCurve curveMovement;
    [SerializeField] float currentMovementTime = 0.0f;
    //[SerializeField] float moveSpeed = 3.0f;
    [SerializeField] bool canMove = false;

    [Header("Time Parameters")]
    [SerializeField] float lifeTime = 0.0f;
    [SerializeField] bool isTimed = false;
    [SerializeField] float currentTime = 0.0f;

    Func<int, IEnumerator> actionToPlay;

    [Header("Network Variables")]
    [SerializeField] NetworkVariable<int> vfxIndex = new NetworkVariable<int>(-2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    #region Getters

    public int GetVfxIndex => vfxIndex.Value;

    #endregion

    #region Setters

    public void SetVisualAsset(VisualEffectAsset _asset) => VisualEffect.visualEffectAsset = _asset;
    public void SetAction(Func<int,IEnumerator> _action) => actionToPlay += _action;
    public void SetVfxIndex(int _index) => vfxIndex.Value = _index;

    public void SetDestination(Vector3 _destination)
    {
        destination = _destination;
        canMove = true;
    }

    public void SetTime(float _time)
    {
        lifeTime = _time;
        isTimed = true;
    }

    #endregion


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            vfxIndex.OnValueChanged += (_old, _new) => Invoke(nameof(InitEffect), 0.1f);
        }
    }

    #region Init

    void InitEffect()
    {
        PlayerEntity _owner = GetComponentInParent<PlayerEntity>();
        SpellManager.Instance.InitEffect_ServerRpc(_owner.PlayerTag, vfxIndex.Value);
    }

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
            MoveTo();

        if (isTimed)
            TimeTo();
    }

    #region Functions

    void MoveTo()
    {
        currentMovementTime += Time.deltaTime / 1.0f;
        float _value = curveMovement.Evaluate(currentMovementTime);

        transform.position = Vector3.Lerp(transform.position, destination, _value);

        if (transform.position == destination)
        {
            if (IsServer)
                OnEndBehaviour_ServerRpc();

            VisualEffect.Stop();
            canMove = false;
        }
    }

    void TimeTo()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= lifeTime)
        {
            if (IsServer)
                OnEndBehaviour_ServerRpc();

            VisualEffect.Stop();
            isTimed = false;
        }
    }

    #endregion

    #region ServerRpc

    [ServerRpc]
    void OnEndBehaviour_ServerRpc()
    {
        StartCoroutine(actionToPlay?.Invoke(vfxIndex.Value));

        Invoke(nameof(DestroyActor), 1.0f);
    }

    #endregion


    #region Server Function

    /// <summary>
    /// Server Function
    /// </summary>
    void DestroyActor()
    {
        NetworkObject.Despawn(this);
    }

    #endregion

}
