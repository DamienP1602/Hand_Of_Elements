using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public enum CardElement
{
    Fire,
    Water,
    Earth,
    Air
}

public class BaseCardData : ScriptableObject
{
    [Header("Base Data")]
    public int cardID;
    public string cardName;
    public CardElement cardElement;
    public int cardCost;

    [Header("Effect")]
    public CardEffectData effect;

#if UNITY_EDITOR
    [MenuItem("CardTools/Sort Card IDs")]
    public static void SortID()
    {
        BaseCardData[] _cards = Resources.FindObjectsOfTypeAll<BaseCardData>();

        int _ID = 0;
        foreach (BaseCardData _data in _cards)
        {
            _data.cardID = _ID;
            EditorUtility.SetDirty(_data);
            Debug.Log(_data.name + " -> " + _ID.ToString());

            _ID++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}



