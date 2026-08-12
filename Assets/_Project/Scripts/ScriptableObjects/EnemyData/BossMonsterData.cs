using UnityEngine;

[System.Serializable]
public struct PatternSpawnData
{
    [Tooltip("소환할 패턴 오브젝트/몬스터/투사체 프리팹")]
    public GameObject patternPrefab;

    [Tooltip("해당 패턴 소환 쿨타임 (초)")]
    public float spawnInterval;

    [Tooltip("한 번에 소환할 개수")]
    public int count;

    [Tooltip("보스 자식 오브젝트 중 소환 위치로 사용할 GameObject/Transform의 이름 목록 (예: SpawnPos_1)")]
    public System.Collections.Generic.List<string> spawnPointNames;

    [Tooltip("소환 위치 offset (자식 이름을 찾지 못했거나 미지정 시 보스 기준 위치)")]
    public Vector3 spawnOffset;

    [Tooltip("다수 소환 시 개별 오브젝트 간의 간격 (Vector3)")]
    public Vector3 spawnSpacing;
}

[CreateAssetMenu(fileName = "BossMonsterData", menuName = "Scriptable Objects/BossMonsterData")]
public class BossMonsterData : MonsterData
{
    [Header("Charge Pattern Settings")]
    public float chargeSpeedMultiplier = 3.0f; // 돌진 시 이동 속도 배율
    public float chargePreDelay = 0.8f;        // 돌진 전 예비동작(텔레그래프/경고) 시간 (초)
    public float chargeCooldown = 5.0f;        // 돌진 패턴 쿨타임 (초)
    public float chargeKnockback = 4.0f;       // 돌진 충돌 시 넉백 힘

    [Header("Spawn Pattern Settings")]
    [Tooltip("보스가 주기적으로 소환할 패턴 프리팹 및 쿨타임 목록")]
    public System.Collections.Generic.List<PatternSpawnData> spawnPatterns;
}
