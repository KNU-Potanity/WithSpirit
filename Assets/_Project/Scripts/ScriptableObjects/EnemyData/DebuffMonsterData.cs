using UnityEngine;

public enum DebuffTypes
{
    Slow
}

[CreateAssetMenu(fileName = "DebuffMonsterData", menuName = "Scriptable Objects/DebuffMonsterData")]
public class DebuffMonsterData : MonsterData
{
    public DebuffTypes DebuffType = DebuffTypes.Slow;
    public float Duration = 3.0f; // 지속시간 (기획서 4.1절 기준 3초)
    public float SpeedDecreaseRate = 0.5f; // 이속 감소율 (기획서 4.1절 기준 50% = 0.5)
}
