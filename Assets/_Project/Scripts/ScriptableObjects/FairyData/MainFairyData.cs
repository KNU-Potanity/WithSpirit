using UnityEngine;

[CreateAssetMenu(fileName = "MainFairyData", menuName = "Scriptable Objects/MainFairyData")]
public class MainFairyData : ScriptableObject
{
    [Header("UI & Metadata")]
    [Tooltip("카테고리 (예: 공격 정령, 플랫폼 정령)")]
    public string Category = "공격 정령";

    [Tooltip("정령 이름 (예: 화상 정령, 빙결 정령 등)")]
    public string FairyName = "정령";

    [TextArea(2, 4)]
    [Tooltip("정령 능력 설명")]
    public string Description;

    [Header("Stats")]
    public int Damage;
    public float CoolTime;
    public float PlatformSize;
    public PlatformTypes PlatformType;
    
    [Tooltip("플랫폼 변신 시 Instantiate할 플랫폼 프리팹")]
    public GameObject PlatformPrefab;

    [Tooltip("마커 위치 기준 플랫폼 생성 오프셋 (x: 좌우, y: 상하)")]
    public Vector2 PlatformSpawnOffset;
}
