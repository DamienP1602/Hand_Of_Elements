using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardMovementComponent : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool drawDebug;

    [Header("Movement Parameters")]
    [SerializeField,Tooltip("Initial speed given when giving new destination")] float maxMovementSpeed;
    [SerializeField,Tooltip("Slowest speed possible for the card")] float minMovementSpeed;
    [SerializeField] float currentMovementSpeed;
    [SerializeField,Tooltip("Reduce the current movement speed by this value each seconds")] float slowFactor;
    [SerializeField] bool canMove;

    [Header("Parameters")]
    [SerializeField] Vector3 destination;
    [SerializeField] Vector3 initialPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
            MoveTo();
    }

    public void SetDestination(Vector3 _destination)
    {
        initialPosition = transform.position;
        destination = _destination;

        currentMovementSpeed = maxMovementSpeed;
        canMove = true;
    }

    public void MoveTo()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, currentMovementSpeed * Time.deltaTime);

        currentMovementSpeed -= slowFactor * Time.deltaTime;
        currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, minMovementSpeed, maxMovementSpeed);

        if (transform.position == destination)
            canMove = false;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 0.5f);
    }
}
