using UnityEngine;

public abstract class UniqueEffectData : ScriptableObject
{
    public abstract void ExecuteEffect(CardComponent _card);

    public abstract string ChangeSpecificText(CardComponent _card, string _text);
}
