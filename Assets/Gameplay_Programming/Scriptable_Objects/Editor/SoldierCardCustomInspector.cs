using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoldierCardData))]
public class SoldierCardCustomInspector : CardCustomInspector
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SoldierCardData _target = (SoldierCardData)target;
        
        DrawBaseData(_target);

        DrawKeyEffect(_target);

        GUILayout.Space(10.0f);
        HorizontalGUI(() =>
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Has Effect", GUILayout.Width(100.0f)))
            {
                _target.hasEffect = !_target.hasEffect;
            }
            GUILayout.FlexibleSpace();
        });
        if (_target.hasEffect)
            DrawEffect(_target);

        DrawTitle("Card Description");
        _target.description = EditorGUILayout.TextArea(_target.description);

        EditorUtility.SetDirty(target);
    }

    protected override void DrawBaseData(BaseCardData _target)
    {
        base.DrawBaseData(_target);

        SoldierCardData _soldier = (SoldierCardData)_target;
        if (_soldier)
        {
            _soldier.attackAmount = EditorGUILayout.IntField("Attack Amount", _soldier.attackAmount);

            _soldier.healthAmount = EditorGUILayout.IntField("Health Amount", _soldier.healthAmount);
        }
    }
}
