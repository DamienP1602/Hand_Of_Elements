using UnityEngine;

public class PlayerSpawnComponent : MonoBehaviour
{
    [SerializeField] bool currentPlayerPosition;

    public bool CurrentPlayerPosition => currentPlayerPosition;
}
