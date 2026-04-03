using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    static T instance = null;

    public static T Instance => instance;

    protected virtual void Awake() => InitSingleton();


    void InitSingleton()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
    }
}