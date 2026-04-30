using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardMovementComponent : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool drawDebug;

    [Header("Parameters")]
    [SerializeField] AnimationCurve movementBasedOnCurve;
    [SerializeField] float movementSpeed;
    [SerializeField] Vector3 destination;
    [SerializeField] Vector3 initialPosition;

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
        initialPosition = transform.position;
        destination = _destination;
    }

    public void MoveTo()
    {
        float _value = Mathf.InverseLerp(initialPosition.magnitude, destination.magnitude, transform.position.magnitude);
        float _step = Time.deltaTime * movementBasedOnCurve.Evaluate(_value) * movementSpeed;

        transform.position = Vector3.MoveTowards(transform.position, destination, _step);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition,0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(destination, 0.5f);
    }
}
