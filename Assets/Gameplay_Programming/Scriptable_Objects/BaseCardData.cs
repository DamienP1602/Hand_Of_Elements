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
        object[] _cards = Resources.FindObjectsOfTypeAll(typeof(BaseCardData));

        int _size = _cards.Length;
        for (int _i = 0; _i < _size; _i++)
        {
            BaseCardData _card = _cards[_i] as BaseCardData;
            if (_card)
            {
                _card.cardID = _i;
                EditorUtility.SetDirty(_card);
                AssetDatabase.SaveAssetIfDirty(_card);
                Debug.Log(_card.cardName + " -> " + _card.cardID.ToString());
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}



