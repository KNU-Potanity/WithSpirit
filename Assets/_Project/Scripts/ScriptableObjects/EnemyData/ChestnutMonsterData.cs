using UnityEngine;

[CreateAssetMenu(fileName = "ChestnutMonsterData", menuName = "Scriptable Objects/ChestnutMonsterData")]
public class ChestnutMonsterData : GroundMonsterData
{
    [Header("Chestnut Jump Attack Settings")]
    [Tooltip("점프 공격 시 발사 각도(도 단위, 예: 45도)")]
    public float jumpAngle = 45f;

    [Tooltip("원본 에셋 스프라이트가 기본적으로 왼쪽을 바라보고 있으면 체크하세요.")]
    public bool defaultLeftFacing = false;
}
