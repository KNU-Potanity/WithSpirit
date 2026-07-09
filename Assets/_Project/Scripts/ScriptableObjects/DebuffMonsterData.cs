using UnityEngine;

public enum DebuffTypes
{
}

[CreateAssetMenu(fileName = "DebuffMonsterData", menuName = "Scriptable Objects/DebuffMonsterData")]
public class DebuffMonsterData : ScriptableObject
{
    public DebuffTypes DebuffType;
    public float AttackRange;
}
