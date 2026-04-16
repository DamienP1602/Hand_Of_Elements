using System;
using Unity.Netcode;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    [field:SerializeField] public HandCardComponent handCardPrefab { get; private set; }
    [field:SerializeField] public BoardCardComponent boardCardPrefab { get; private set; }

    [field:SerializeField] public Vector3 cardShowPositon { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cardShowPositon, Vector3.one);
    }
}
