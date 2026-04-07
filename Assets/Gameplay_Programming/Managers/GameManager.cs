using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Players")]
    [field:SerializeField] public PlayerEntity firstPlayer { get; set; }
    [field:SerializeField] public PlayerEntity secondPlayer { get; set; }

    [SerializeField] Card card1;
    [SerializeField] Card card2;

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
        firstPlayer.Init(PlayerEnum.Player_One);
        firstPlayer.transform.position = new Vector3(0.0f, 2.0f, 7.0f);

        secondPlayer = _players[1];
        secondPlayer.Init(PlayerEnum.Player_Two);
        secondPlayer.transform.position = new Vector3(0.0f, 2.0f, -7.0f);
    }

    public PlayerEntity GetPlayer(PlayerEnum _type)
    {
        return _type == PlayerEnum.Player_One ? secondPlayer : firstPlayer;
    }

}
