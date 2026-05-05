using UnityEngine;

[CreateAssetMenu(fileName = "New Spell Card Data", menuName = "Scriptable Objects/Spell Card")]
public class SpellCardData : BaseCardData
{
    [Header("Spell Data")]
    public string description;
}
