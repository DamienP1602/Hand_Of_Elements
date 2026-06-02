using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using static CardEffectData;

[CustomEditor(typeof(SpellCardData))]
public class SpellCardCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SpellCardData _target = (SpellCardData)target;

        DrawTitle(_target.cardName + " Data");
        DrawBaseData(_target);
        DrawEffect(_target);

        EditorUtility.SetDirty(target);
    }

    void DrawBaseData(SpellCardData _target)
    {
        _target.cardID = EditorGUILayout.IntField("Card ID", _target.cardID);

        _target.cardName = EditorGUILayout.TextField("Card Name", _target.cardName);

        _target.cardElement = (CardElement)EditorGUILayout.EnumPopup("Card Element", _target.cardElement);

        _target.cardCost = EditorGUILayout.IntField("Arcane Cost", _target.cardCost);
    }

    void DrawEffect(SpellCardData _target)
    {
        GUILayout.Space(20.0f);

        DrawTitle("Card Effect");
        DrawEffectDatas(_target.effect);

        GUILayout.Space(20.0f);

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

        DrawTitle("Card Description");
        _target.description = EditorGUILayout.TextArea(_target.description);
    }

    void DrawEffectDatas(CardEffectData _effect)
    {
        _effect.triggerMode = (CardEffectTriggerMode)EditorGUILayout.EnumPopup("Trigger Mode", _effect.triggerMode);

        _effect.selectionMode = (CardEffectSelectionMode)EditorGUILayout.EnumPopup("Targets", _effect.selectionMode);

        _effect.effectMode = (CardEffectMode)EditorGUILayout.EnumPopup("Effect", _effect.effectMode);

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
        }

        _effect.effectAsset = (VisualEffectAsset)EditorGUILayout.ObjectField("Visual Asset", _effect.effectAsset, typeof(VisualEffectAsset), false);
        _effect.isInstantEffect = EditorGUILayout.Toggle("In Effect Instant", _effect.isInstantEffect);
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

    void DrawTitle(string _label, float _space = 5.0f)
    {
        GUILayout.Space(_space);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_label, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void HorizontalGUI(Action _fields)
    {
        GUILayout.BeginHorizontal();
        _fields.Invoke();
        GUILayout.EndHorizontal();
    }
}
