using System;
using UnityEngine;

public class CardMovementComponent : MonoBehaviour
{
    public event Action OnDestinationReached;

    [Header("Debug")]
    [SerializeField] bool drawDebug;

    [Header("Movement Parameters")]
    [SerializeField] float movementSpeed;
    [SerializeField] bool canMove;
    [SerializeField] bool canRotate;
    [SerializeField] bool lockUpdate;
    [SerializeField] float rotationSpeed = 100.0f;

    [Header("Parameters")]
    [SerializeField] Vector3 destination;
    [SerializeField] Quaternion rotationDestination;

    public void SetLockUpdate(bool _value) => lockUpdate = _value;

    void Start()
    {

    }

    void Update()
    {
        if (lockUpdate) return;

        if (canMove)
            MoveTo();

        if (canRotate)
            RotateTo();
    }

    #region Update

    public void MoveTo()
    {
        transform.position = Vector3.Lerp(transform.position, destination, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position,destination) < 0.01f)
        {
            canMove = false;
            OnDestinationReached?.Invoke();
        }
    }

    public void RotateTo()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation,rotationDestination,Time.deltaTime * rotationSpeed);

        if (transform.rotation == rotationDestination)
            canRotate = false;
    }

    #endregion

    #region Functions

    public void SetDestination(Vector3 _destination)
    {
        if (_destination == transform.position) return;

        destination = _destination;
        canMove = true;
    }

    public void SetRotationDestination(Quaternion _destination)
    {
        rotationDestination = _destination;
        canRotate = true;
    }

    public void SetSpeed(float _value) => movementSpeed = _value;

    #endregion

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 0.5f);
    }
}
