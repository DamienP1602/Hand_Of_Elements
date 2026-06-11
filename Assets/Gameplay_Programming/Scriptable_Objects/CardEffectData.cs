using System;
using UnityEngine.VFX;

[Serializable]
public enum DebuffType
{
    NONE,
    BurnToken
}

[Serializable]
public class CardEffectData
{
    [Serializable]
    public enum CardEffectTriggerMode
    {
        OnPlayed,
        AtDeath,
        OnDiscarded
    }

    [Serializable]
    public enum CardEffectSelectionMode
    {
        NoTarget,
        SingleTarget,
        Self,
        Opponent,
        RandomOpponent,
        AllOpponentSoldier,
        AllOpponents
    }

    [Serializable]
    public enum CardEffectMode
    {
        NONE,
        Summon,
        Heal,
        InstantDamage,
        Debuff,
        Draw,
        RestaureArcane,
        GainArcane
    }

    [Serializable]
    public enum CardEffectModifier
    {
        Burn
    }

    [Serializable]
    public enum KeyEffect
    {
        NONE,
        Overload,
        Etherial,
        Hidden,
        Purification,
        AntiHeal,
        ManaShield
    }

    public CardEffectSelectionMode selectionMode;
    public CardEffectMode effectMode;
    public CardEffectModifier effectModifier;
    public CardEffectTriggerMode triggerMode;
    public BaseCardData cardReference;
    public DebuffType debuffType;
    public int amount;
    public VisualEffectAsset effectAsset;
    public bool isInstantEffect;
    public float effectTime;
    public CardElement specificElement;
    public KeyEffect specificKeyEffect;
}