using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Players")]
    [field:SerializeField] public PlayerEntity firstPlayer { get; set; }
    [field:SerializeField] public PlayerEntity secondPlayer { get; set; }

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
        firstPlayer.Init();
        firstPlayer.transform.position = new Vector3(0.0f, 2.0f, 7.0f);

        secondPlayer = _players[1];
        secondPlayer.Init();
        secondPlayer.transform.position = new Vector3(0.0f, 2.0f, -7.0f);
    }

}
