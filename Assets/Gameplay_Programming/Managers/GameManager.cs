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
        firstPlayer.HandComponent.value = true;
        firstPlayer.Init();

        secondPlayer = _players[1];
        secondPlayer.HandComponent.value = false;
        secondPlayer.Init();
    }

}
