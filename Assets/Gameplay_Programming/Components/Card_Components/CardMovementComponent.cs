using UnityEngine;

public class CardMovementComponent : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool drawDebug;

    [Header("Movement Parameters")]
    [SerializeField] float movementSpeed;
    [SerializeField] bool canMove;
    [SerializeField] bool canRotate;
    [SerializeField] float rotationSpeed = 100.0f;

    [Header("Parameters")]
    [SerializeField] Vector3 destination;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] Quaternion rotationDestination;

    void Start()
    {

    }

    void Update()
    {
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
            canMove = false;
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

        initialPosition = transform.position;
        destination = _destination;

        canMove = true;
    }

    public void SetRotationDestination(Quaternion _destination)
    {
        rotationDestination = _destination;
        canRotate = true;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 0.5f);
    }
}
