using UnityEngine;

[CreateAssetMenu(fileName = "MainFairyData", menuName = "Scriptable Objects/MainFairyData")]
public class MainFairyData : ScriptableObject
{
    public int Damage;
    public float CoolTime;
    public float PlatformSize;
    public PlatformTypes PlatformType;
    
    [Tooltip("플랫폼 변신 시 Instantiate할 플랫폼 프리팹")]
    public GameObject PlatformPrefab;

    [Tooltip("마커 위치 기준 플랫폼 생성 오프셋 (x: 좌우, y: 상하)")]
    public Vector2 PlatformSpawnOffset;
}
