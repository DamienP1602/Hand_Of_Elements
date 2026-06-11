using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Master File", menuName = "Scriptable Objects/Master File")]
public class MasterCardData : ScriptableObject
{
    int ID = 0;
    public List<BaseCardData> fireCards = new();
    public List<BaseCardData> waterCards = new();
    public List<BaseCardData> earthCards = new();
    public List<BaseCardData> thunderCards = new();
    public List<BaseCardData> globalMagicCards = new();

#if UNITY_EDITOR
    #region Menu
    [ContextMenu("Sort Card IDs")]
    public void SortID()
    {
        ID = 0;
        SetList(fireCards);
        SetList(waterCards);
        SetList(earthCards);
        SetList(thunderCards);
        SetList(globalMagicCards);
    }

    void SetList(List<BaseCardData> _list)
    {
        foreach (BaseCardData _card in _list)
        {
            _card.cardID = ID;
            EditorUtility.SetDirty(_card);
            Debug.Log(_card.name + " -> " + ID.ToString());
            ID++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    #endregion
#endif

    #region Init
    public Dictionary<int, BaseCardData> GetAllCards()
    {
        Dictionary<int, BaseCardData> _dic = new Dictionary<int, BaseCardData>();

        PutInDictionary(fireCards, ref _dic);
        PutInDictionary(waterCards, ref _dic);
        PutInDictionary(earthCards, ref _dic);
        PutInDictionary(thunderCards, ref _dic);
        PutInDictionary(globalMagicCards, ref _dic);

        return _dic;
    }

    void PutInDictionary(List<BaseCardData> _list, ref Dictionary<int, BaseCardData> _dic)
    {
        foreach (BaseCardData _card in _list)
        {
            _dic.Add(_card.cardID, _card);
        }
    }
    #endregion
}
