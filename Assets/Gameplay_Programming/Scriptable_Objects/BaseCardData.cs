using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[Serializable]
public enum CardElement
{
    Fire,
    Water,
    Earth,
    Air,
    NONE
}

public class BaseCardData : ScriptableObject
{
    [Header("Base Data")]
    public int cardID;
    public string cardName;
    public CardElement cardElement;
    public int cardCost;

    [Header("Effect")]
    public bool hasEffect;
    // move to spell
    public bool isHiddenEffect;
    public CardEffectData effect;
    public bool hasElementaryCombo;
    public CardEffectData elementaryComboEffect;
    public string description;
    public string elementaryComboDescription;
}
