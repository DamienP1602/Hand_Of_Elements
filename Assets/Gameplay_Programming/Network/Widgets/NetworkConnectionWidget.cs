using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class NetworkConnectionWidget : NetworkBehaviour
{
    [SerializeField] NetworkDebugWidget debug;
    [SerializeField] TMP_InputField clientIpField;
    [SerializeField] TMP_Text hostIpText;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button startGame;

    async void Start()
    {
        // Initalize the service
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            debug.SetDebugText("Sign in to server");
        }
        
        // Init buttons
        hostButton.onClick.AddListener(LaunchHost);
        joinButton.onClick.AddListener(LaunchClient);

        startGame.onClick.AddListener(() => NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single));

        NetworkManager.Singleton.OnConnectionEvent += OnConnection;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnConnection(NetworkManager _manager, ConnectionEventData _data)
    {
        if (IsOwner)
        {
            if (_data.ClientId == _manager.CurrentSessionOwner)
            {
                debug.SetDebugText("Server Created");
            }
            else
            {
                debug.SetDebugText("new Client : " + _data.ClientId.ToString());
            }
        }
        else if (IsClient)
        {
            if (_data.EventType == ConnectionEvent.ClientConnected)
                debug.SetDebugText("Connected to : " + _manager.CurrentSessionOwner.ToString());
            else if (_data.EventType == ConnectionEvent.ClientDisconnected)
                debug.SetDebugText("Client Disconnected : " + _data.ClientId.ToString());
        }

        SetConnectionText();
        CheckForStartGame();
    }

    void SetConnectionText()
    {
        string _s = "";

        foreach (ulong _client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.CurrentSessionOwner == _client)
            {
                _s += "Owner : " + _client.ToString() + "\n";
            }
            else
            {
                _s += "Client : " + _client.ToString() + "\n";
            }
        }

        debug.SetInfoText(_s);
    }

    void CheckForStartGame()
    {
        if (IsOwner)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                startGame.gameObject.SetActive(true);
            }
        }
    }

    async void LaunchHost()
    {
        Allocation _alloc = await RelayService.Instance.CreateAllocationAsync(1);

        string _joinCode = await RelayService.Instance.GetJoinCodeAsync(_alloc.AllocationId);

        UnityTransport _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        _transport.SetRelayServerData(new RelayServerData(_alloc,"dtls"));

        hostIpText.gameObject.SetActive(true);
        hostIpText.text = _joinCode;

        NetworkManager.Singleton.StartHost();
    }

    async void LaunchClient()
    {
        if (string.IsNullOrEmpty(clientIpField.text)) return;

        JoinAllocation _join = await RelayService.Instance.JoinAllocationAsync(clientIpField.text.Trim().ToUpper());

        UnityTransport _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        _transport.SetRelayServerData(new RelayServerData(_join,"dtls"));

        NetworkManager.Singleton.StartClient();
    }

}
