using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class VisualSpellEffectComponent : NetworkBehaviour
{
    [field:SerializeField] public VisualEffect VisualEffect { get; private set; }

    [SerializeField] Vector3 destination;
    [SerializeField] float moveSpeed = 3.0f;

    public void SetVisualAsset(VisualEffectAsset _asset) => VisualEffect.visualEffectAsset = _asset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveTo();
    }

    public void SetDestination(Vector3 _destination)
    {
        destination = _destination;
    }

    void MoveTo()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);

        if (transform.position == destination)
        {
            OnDestinationReached_ServerRpc();
        }
    }

    [ServerRpc]
    void OnDestinationReached_ServerRpc()
    {
        SpellManager.Instance.CastEffect();
        NetworkObject.Despawn(this);
    }
}
