using System;

[Serializable]
public struct CardEffectData
{
    [Serializable]
    public enum CardEffectSelectionMode
    {
        SingleTarget,
        Self,
        Opponent
    }

    [Serializable]
    public enum CardEffectMode
    {
        Summon,
        Heal,
        InstantDamage,
        Debuff
    }

    public CardEffectSelectionMode selectionMode;
    public CardEffectMode effectMode;
    public BaseCardData cardReference;
    public int amount;
    public bool elementaryCombo;
}