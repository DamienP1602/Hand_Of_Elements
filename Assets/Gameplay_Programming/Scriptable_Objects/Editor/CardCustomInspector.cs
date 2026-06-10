using NUnit.Framework.Internal;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using static CardEffectData;

public class CardCustomInspector : Editor
{
    protected virtual void DrawBaseData(BaseCardData _target)
    {
        DrawTitle(_target.cardName + " Data");

        _target.cardID = EditorGUILayout.IntField("Card ID", _target.cardID);

        _target.cardName = EditorGUILayout.TextField("Card Name", _target.cardName);

        _target.cardRarity = (CardRarity)EditorGUILayout.EnumPopup("Rarity", _target.cardRarity);

        _target.cardElement = (CardElement)EditorGUILayout.EnumPopup("Card Element", _target.cardElement);

        _target.cardCost = EditorGUILayout.IntField("Arcane Cost", _target.cardCost);
    }

    protected void DrawEffect(BaseCardData _target)
    {
        GUILayout.Space(10.0f);

        DrawTitle("Card Effect");

        _target.hasUniqueEffect = EditorGUILayout.Toggle("Unique Effect (Mythic Cards)", _target.hasUniqueEffect);
        if (_target.hasUniqueEffect)
        {
            _target.uniqueEffectData = (UniqueEffectData)EditorGUILayout.ObjectField("Unique Effect", _target.uniqueEffectData, typeof(UniqueEffectData), false);
            GUILayout.Space(10.0f);
        }

        DrawEffectDatas(_target.effect);

        GUILayout.Space(10.0f);

        HorizontalGUI(() =>
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Has Elementary Combo", GUILayout.Width(200.0f)))
            {
                _target.hasElementaryCombo = !_target.hasElementaryCombo;
            }
            GUILayout.FlexibleSpace();
        });

        if (_target.hasElementaryCombo)
        {
            DrawTitle("Elementary Combo Effect");
            DrawEffectDatas(_target.elementaryComboEffect);
        }
    }

    void DrawEffectDatas(CardEffectData _effect)
    {
        _effect.triggerMode = (CardEffectTriggerMode)EditorGUILayout.EnumPopup("Trigger Mode", _effect.triggerMode);

        _effect.selectionMode = (CardEffectSelectionMode)EditorGUILayout.EnumPopup("Targets", _effect.selectionMode);

        _effect.effectMode = (CardEffectMode)EditorGUILayout.EnumPopup("Effect", _effect.effectMode);

        _effect.triggerEffectOnDiscard = EditorGUILayout.Toggle("Trigger effect on discard", _effect.triggerEffectOnDiscard);

        DrawEffectMode(_effect);

        _effect.effectAsset = (VisualEffectAsset)EditorGUILayout.ObjectField("Visual Asset", _effect.effectAsset, typeof(VisualEffectAsset), false);
        _effect.isInstantEffect = EditorGUILayout.Toggle("In Effect Instant", _effect.isInstantEffect);
    }

    protected void DrawKeyEffect(BaseCardData _data)
    {
        EditorGUILayout.Space(10.0f);

        _data.hasKeyEffect = EditorGUILayout.Toggle("Has Key Effect", _data.hasKeyEffect);
        if (!_data.hasKeyEffect)
            return;

        DrawTitle("Key Effect");
        _data.effect.keyEffect = (KeyEffect)EditorGUILayout.EnumPopup("Key Effect", _data.effect.keyEffect);

        switch (_data.effect.keyEffect)
        {
            case KeyEffect.NONE:
                break;
            case KeyEffect.Overload:
                _data.effect.keyEffectValue = EditorGUILayout.IntField("Amount of Card Discarded", _data.effect.keyEffectValue);
                break;
            case KeyEffect.Etherial:
                break;
            case KeyEffect.Hidden:
                break;
            case KeyEffect.Purification:
                break;
            case KeyEffect.AntiHeal:
                break;
            case KeyEffect.ManaShield:
                break;
            default:
                break;
        }
    }

    void DrawEffectMode(CardEffectData _effect)
    {
        switch (_effect.effectMode)
        {
            case CardEffectMode.Summon:
                DrawSummon(_effect);
                break;
            case CardEffectMode.Heal:
                DrawHeal(_effect);
                break;
            case CardEffectMode.InstantDamage:
                DrawInstantDamage(_effect);
                break;
            case CardEffectMode.Debuff:
                DrawDebuff(_effect);
                break;
            case CardEffectMode.Draw:
                DrawDrawCard(_effect);
                break;
        }
    }

    void DrawSummon(CardEffectData _effect)
    {
        DrawTitle("Summon Data");

        _effect.cardReference = (SoldierCardData)EditorGUILayout.ObjectField("Card Summoned", _effect.cardReference, typeof(SoldierCardData), false);

        _effect.amount = EditorGUILayout.IntField("Summon Amount", _effect.amount);
    }

    void DrawHeal(CardEffectData _effect)
    {
        DrawTitle("Heal Data");

        _effect.amount = EditorGUILayout.IntField("Heal Amount", _effect.amount);
    }

    void DrawInstantDamage(CardEffectData _effect)
    {
        DrawTitle("Instant Damage Data");

        _effect.amount = EditorGUILayout.IntField("Damage Amount", _effect.amount);
    }

    void DrawDebuff(CardEffectData _effect)
    {
        DrawTitle("Debuff Data");

        _effect.debuffType = (DebuffType)EditorGUILayout.EnumPopup("Turn Based Effect", _effect.debuffType);

        _effect.amount = EditorGUILayout.IntField("Amount", _effect.amount);
    }

    void DrawDrawCard(CardEffectData _effect)
    {
        DrawTitle("Draw Data");

        _effect.amount = EditorGUILayout.IntField("Amount", _effect.amount);

        _effect.specificElement = (CardElement)EditorGUILayout.EnumPopup("Specific Element", _effect.specificElement);

        _effect.specificKeyEffect = (KeyEffect)EditorGUILayout.EnumPopup("Specific Key Effect", _effect.specificKeyEffect);
    }


    void DrawUniqueEffect(BaseCardData _target)
    {
        

        _target.effect.effectAsset = (VisualEffectAsset)EditorGUILayout.ObjectField("Visual Asset", _target.effect.effectAsset, typeof(VisualEffectAsset), false);
        _target.effect.isInstantEffect = EditorGUILayout.Toggle("In Effect Instant", _target.effect.isInstantEffect);
    }

    protected void DrawTitle(string _label, float _space = 5.0f)
    {
        GUILayout.Space(_space);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_label, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    protected void HorizontalGUI(Action _fields)
    {
        GUILayout.BeginHorizontal();
        _fields.Invoke();
        GUILayout.EndHorizontal();
    }
}
