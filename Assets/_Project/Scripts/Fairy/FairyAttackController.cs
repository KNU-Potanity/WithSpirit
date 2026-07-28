using UnityEngine;

public class FairyAttackController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  데이터 참조
    // ─────────────────────────────────────────────
    [Header("Fairy Data")]
    public MainFairyData fairyData;

    // ─────────────────────────────────────────────
    //  내부 변수
    // ─────────────────────────────────────────────
    private int attackDamage;
    private float coolTime;
    private float coolTimer = 0f; // 0이면 공격 가능

    // ─────────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────────
    private void Awake()
    {
        if (fairyData != null)
        {
            attackDamage = fairyData.Damage;
            coolTime     = fairyData.CoolTime;
        }
        else
        {
            Debug.LogWarning("[FairyAttackController] fairyData가 할당되지 않았습니다!");
        }
    }

    // ─────────────────────────────────────────────
    //  쿨타임 업데이트
    // ─────────────────────────────────────────────
    private void Update()
    {
        if (coolTimer > 0f)
            coolTimer -= Time.deltaTime;
    }

    // ─────────────────────────────────────────────
    //  공격 요청 (MonsterClickMarker에서 호출)
    // ─────────────────────────────────────────────
    /// <summary>
    /// 몬스터 마커를 클릭했을 때 MonsterClickMarker가 호출합니다.
    /// 쿨타임이 지난 경우에만 실제로 공격합니다.
    /// </summary>
    /// <param name="target">공격할 몬스터의 GroundMonsterHealth</param>
    public void RequestAttack(GroundMonsterHealth target)
    {
        // 쿨타임 중이면 무시
        if (coolTimer > 0f)
        {
            Debug.Log($"[FairyAttackController] 쿨타임 중 ({coolTimer:F1}초 남음)");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("[FairyAttackController] 공격 대상이 없습니다.");
            return;
        }

        // 공격 실행
        target.TakeDamage(attackDamage);
        Debug.Log($"[FairyAttackController] {target.gameObject.name} 공격 — 데미지: {attackDamage}");

        // 쿨타임 시작
        coolTimer = coolTime;
    }

    // ─────────────────────────────────────────────
    //  상태 조회 (외부 참조용)
    // ─────────────────────────────────────────────
    /// <summary>현재 공격 가능한 상태인지 반환합니다.</summary>
    public bool CanAttack => coolTimer <= 0f;
}
