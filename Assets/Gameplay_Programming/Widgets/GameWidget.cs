using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameWidget : MonoBehaviour
{
    [SerializeField] Button endTurnButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endTurnButton.onClick.AddListener(ChangeTurn);
    }
    // Update is called once per frame
    void Update()
    {

    }

    void ChangeTurn()
    {
        NetworkObject _obj = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (_obj.GetComponent<PlayerEntity>() is PlayerEntity _player)
        {
            _player.ChangeTurn();
        }
    }
}
