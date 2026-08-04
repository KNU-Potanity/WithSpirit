using UnityEngine;

public enum DebuffTypes
{
}

[CreateAssetMenu(fileName = "DebuffMonsterData", menuName = "Scriptable Objects/DebuffMonsterData")]
public class DebuffMonsterData : MonsterData
{
    public DebuffTypes DebuffType;
}
