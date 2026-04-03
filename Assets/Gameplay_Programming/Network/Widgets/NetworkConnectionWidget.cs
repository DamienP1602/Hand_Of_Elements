using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
                debug.SetDebugText("Owner Created");
            }
            else
            {
                debug.SetDebugText("Client is connected : " + _data.ClientId.ToString());
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

    void LaunchHost()
    {
        SetIPAddress();
        hostIpText.gameObject.SetActive(true);
        hostIpText.text = GetHostIP();

        NetworkManager.Singleton.StartHost();
    }

    void SetIPAddress()
    {
        string _text = clientIpField.text;
        UnityTransport _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        UnityTransport.ConnectionAddressData _data = _transport.ConnectionData;
        _data.Address = _text;
        _transport.ConnectionData = _data;
    }

    void LaunchClient()
    {
        SetIPAddress();

        NetworkManager.Singleton.StartClient();
    }

    string GetHostIP()
    {
        IPHostEntry _host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress _ip in _host.AddressList)
        {
            if (_ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return _ip.ToString();
            }
        }
        return "";
    }
}
