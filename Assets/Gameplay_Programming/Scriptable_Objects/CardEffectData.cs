using System;

[Serializable]
public struct CardEffectData
{
    [Serializable]
    public enum CardEffectSelectionMode
    {
        SingleTarget,
        Self,
        Opponent,
        NoMoreTarget
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
    public BaseCardData cardReference;
    public int amount;
}