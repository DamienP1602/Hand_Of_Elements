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
    [SerializeField] List<Vector3> destinations;
    [SerializeField] int destinationIndex;
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

    public void SetDestination(List<Vector3> _destination)
    {
        initialPosition = transform.position;
        destinations = _destination;
        destinationIndex = 0;
    }

    public void MoveTo()
    {
        if (destinationIndex == destinations.Count) return;

        if (transform.position == destinations[destinationIndex])
        {
            destinationIndex++;
            return;
        }

        float _value = Mathf.InverseLerp(initialPosition.magnitude, destinations[destinationIndex].magnitude, transform.position.magnitude);
        float _step = Time.deltaTime * movementBasedOnCurve.Evaluate(_value) * movementSpeed;

        transform.position = Vector3.MoveTowards(transform.position, destinations[destinationIndex], _step);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (destinationIndex == destinations.Count) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition,0.5f);

        int _size = destinations.Count;
        for (int _i = 0; _i < _size; _i++)
        {
            if (_i == 0)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(destinations[0], 0.5f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, destinations[destinationIndex]);
    }
}
