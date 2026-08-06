using UnityEngine;

[CreateAssetMenu(fileName = "BossMonsterData", menuName = "Scriptable Objects/BossMonsterData")]
public class BossMonsterData : MonsterData
{
    [Header("Charge Pattern Settings")]
    public float chargeSpeedMultiplier = 3.0f; // 돌진 시 이동 속도 배율
    public float chargePreDelay = 0.8f;        // 돌진 전 예비동작(텔레그래프/경고) 시간 (초)
    public float chargeCooldown = 5.0f;        // 돌진 패턴 쿨타임 (초)
    public float chargeKnockback = 4.0f;       // 돌진 충돌 시 넉백 힘
}
