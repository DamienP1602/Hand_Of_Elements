using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
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