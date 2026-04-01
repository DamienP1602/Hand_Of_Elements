using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

public class NetworkConnectionWidget : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var session = await MultiplayerService.Instance.CreateSessionAsync(options);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
