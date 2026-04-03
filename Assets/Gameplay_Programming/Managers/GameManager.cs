using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Camera Parameters")]
    [SerializeField] Vector3 cameraPosition;
    [SerializeField] Camera playerCamera;

    [field:SerializeField] PlayerEntity firstPlayer { get; set; }
    [field:SerializeField] PlayerEntity secondPlayer { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitPlayers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitPlayers()
    {
        PlayerEntity[] _players = FindObjectsByType<PlayerEntity>(FindObjectsSortMode.InstanceID);

        firstPlayer = _players[0];
        firstPlayer.InitCamera(playerCamera, cameraPosition, false);
        secondPlayer = _players[1];
        secondPlayer.InitCamera(playerCamera, cameraPosition, true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cameraPosition, 1.0f);
    }

}
