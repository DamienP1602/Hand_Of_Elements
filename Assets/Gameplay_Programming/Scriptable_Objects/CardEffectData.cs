using System;

[Serializable]
public class CardEffectData
{
    [Serializable]
    public enum CardEffectTriggerMode
    {
        OnPlayed,
        AtDeath
    }

    [Serializable]
    public enum CardEffectSelectionMode
    {
        NoTarget,
        SingleTarget,
        Self,
        Opponent,
    }

    [Serializable]
    public enum CardEffectMode
    {
        Summon,
        Heal,
        InstantDamage,
        Debuff
    }

    [Serializable]
    public enum CardEffectModifier
    {
        Burn,
        AntiHeal,
        MagicShield
    }

    public CardEffectSelectionMode selectionMode;
    public CardEffectMode effectMode;
    public CardEffectModifier effectModifier;
    public CardEffectTriggerMode triggerMode;
    public BaseCardData cardReference;
    public int amount;
}