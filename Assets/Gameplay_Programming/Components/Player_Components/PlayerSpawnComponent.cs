using UnityEngine;

public class PlayerSpawnComponent : MonoBehaviour
{
    [SerializeField] bool currentPlayerPosition;

    #region Getters

    public bool CurrentPlayerPosition => currentPlayerPosition;

    #endregion
}
