using UnityEditor;

[CustomEditor(typeof(SpellCardData))]
public class SpellCardCustomInspector : CardCustomInspector
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SpellCardData _target = (SpellCardData)target;

        if (!_target.hasEffect)
            _target.hasEffect = true;

        DrawBaseData(_target);
        DrawEffect(_target);

        DrawTitle("Card Description");
        _target.description = EditorGUILayout.TextArea(_target.description);

        EditorUtility.SetDirty(target);
    }
}
