using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    static T instance = null;

    #region Getters

    public static T Instance => instance;

    #endregion

    protected virtual void Awake() => InitSingleton();

    #region Inits

    void InitSingleton()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
    }

    #endregion
}