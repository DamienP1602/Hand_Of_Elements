using UnityEngine;

[CreateAssetMenu(fileName = "New Soldier Card Data", menuName = "Scriptable Objects/Soldier Card")]
public class SoldierCardData : BaseCardData
{
    [Header("Soldier Data")]
    public int damages;
    public int health;
}
