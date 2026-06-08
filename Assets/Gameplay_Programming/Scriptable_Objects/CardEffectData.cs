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
        AtDeath
    }

    [Serializable]
    public enum CardEffectSelectionMode
    {
        NoTarget,
        SingleTarget,
        Self,
        Opponent,
        RandomOpponent,
        AllOpponentSoldier
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
    public CardElement specificElement;
    public KeyEffect keyEffect;
    public int keyEffectValue;

    public string ChangeSpecialText(string _text)
    {
        string _toReplace = "";

        if (keyEffect == KeyEffect.Overload)
            _toReplace = keyEffectValue.ToString();

        return _text.Replace("#",_toReplace);
    }
}