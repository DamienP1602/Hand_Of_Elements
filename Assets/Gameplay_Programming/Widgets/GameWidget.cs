using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameWidget : NetworkBehaviour
{
    [SerializeField] Button endTurnButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endTurnButton.onClick.AddListener(ChangeTurn_ServerRpc);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ServerRpc]
    void ChangeTurn_ServerRpc()
    {
        PlayerEnum _newTurnTag = GameManager.Instance.PlayerTurnTag == PlayerEnum.Player_One ? PlayerEnum.Player_Two : PlayerEnum.Player_One;
        GameManager.Instance.ChangeTurn_ClientRpc(_newTurnTag);
    }
}
