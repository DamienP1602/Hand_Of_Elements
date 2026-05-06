using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameWidget : MonoBehaviour
{
    [SerializeField] Button endTurnButton;

    void Start()
    {
        endTurnButton.onClick.AddListener(ChangeTurn);
    }

    void Update()
    {

    }

    #region Functions

    void ChangeTurn()
    {
        NetworkObject _obj = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (_obj.GetComponent<PlayerEntity>() is PlayerEntity _player)
        {
            _player.ChangeTurn();
        }
    }

    public void SetButtonIsVisible(bool _value)
    {
        endTurnButton.gameObject.SetActive(_value);
    }

    #endregion
}
