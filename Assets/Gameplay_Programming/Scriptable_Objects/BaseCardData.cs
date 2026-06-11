using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CardEffectData;

[Serializable]
public enum CardElement
{
    Fire,
    Ice,
    Earth,
    Thunder,
    NONE
}

[Serializable]
public enum CardRarity
{
    Summoned,
    Common,
    Rare,
    Epic,
    Mythic
}

public class BaseCardData : ScriptableObject
{
    [Header("Base Data")]
    public int cardID;
    public string cardName;
    public CardElement cardElement;
    public CardRarity cardRarity;
    public int cardCost;

    [Header("Effect")]
    public bool hasEffect;
    public bool hasKeyEffect;
    // move to spell
    public bool isHiddenEffect;
    public CardEffectData effect;
    public bool hasElementaryCombo;
    public CardEffectData elementaryComboEffect;
    public string description;
    public string elementaryComboDescription;
    public bool hasUniqueEffect;
    public UniqueEffectData uniqueEffectData;
    public KeyEffect keyEffect;
    public int keyEffectValue;
}
