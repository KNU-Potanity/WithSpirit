using UnityEngine;

[CreateAssetMenu(fileName = "AttackFairyData", menuName = "Scriptable Objects/AttackFairyData")]
public class AttackFairyData : ScriptableObject
{
    public int Damage;
    public float CoolTime;
    public float AttackRange;
}
