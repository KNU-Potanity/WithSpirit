using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 몬스터의 자식 오브젝트에 붙이는 마커 컴포넌트.
/// Update()에서 OverlapPoint로 마우스 위치를 직접 감지하므로
/// 부모 오브젝트의 콜라이더나 Physics 2D Raycaster 세팅과 무관하게 동작합니다.
/// 이 오브젝트의 Collider2D 크기 = 마우스 인식 범위(에임 보정)
///
/// 씬 구성 예시:
/// Mushroom (몬스터)
/// └── MonsterMarker (이 스크립트가 붙는 자식 오브젝트)
///     ├── Collider2D — IsTrigger ✅, 크기로 마우스 감지 범위 조절
///     └── SpriteRenderer (선택) — 마커 비주얼을 여기 바로 달아도 됨
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MonsterClickMarker : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  마커 설정
    // ─────────────────────────────────────────────
    [Header("Marker Settings")]
    [Tooltip("마우스를 올렸을 때 표시할 마커 오브젝트 (이 오브젝트 자신 또는 자식 GameObject)")]
    public GameObject markerObject;

    // ─────────────────────────────────────────────
    //  내부 참조
    // ─────────────────────────────────────────────
    private FairyAttackController fairyAttack;
    private GroundMonsterHealth monsterHealth;
    private Collider2D col;

    private bool isHovering = false; // 현재 마우스가 이 콜라이더 위에 있는지

    // ─────────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────────
    private void Start()
    {
        col = GetComponent<Collider2D>();

        // 부모 오브젝트에서 체력 컴포넌트를 찾음
        monsterHealth = GetComponentInParent<GroundMonsterHealth>();
        if (monsterHealth == null)
            Debug.LogWarning($"[MonsterClickMarker] {gameObject.name} 의 부모에서 GroundMonsterHealth를 찾을 수 없습니다!");

        // 씬에서 정령 공격 컨트롤러를 찾아 캐싱
        fairyAttack = FindAnyObjectByType<FairyAttackController>();
        if (fairyAttack == null)
            Debug.LogWarning("[MonsterClickMarker] 씬에서 FairyAttackController를 찾을 수 없습니다!");

        // 마커는 처음에 꺼둠
        HideMarker();
    }

    // ─────────────────────────────────────────────
    //  매 프레임 마우스 위치 감지
    // ─────────────────────────────────────────────
    private void Update()
    {
        // 마우스의 화면 좌표 → 월드 좌표 변환
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // 이 오브젝트의 콜라이더 위에 마우스가 있는지 체크
        bool over = col.OverlapPoint(mouseWorldPos);

        // 마우스가 새로 올라왔을 때
        if (over && !isHovering)
        {
            isHovering = true;
            ShowMarker();
        }
        // 마우스가 벗어났을 때
        else if (!over && isHovering)
        {
            isHovering = false;
            HideMarker();
        }

        // 마우스가 올라온 상태에서 왼쪽 클릭
        if (isHovering && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (fairyAttack != null && monsterHealth != null)
                fairyAttack.RequestAttack(monsterHealth);
        }
    }

    // ─────────────────────────────────────────────
    //  마커 표시/숨김
    // ─────────────────────────────────────────────
    private void ShowMarker()
    {
        if (markerObject != null)
            markerObject.SetActive(true);
    }

    /// <summary>
    /// 마커를 강제로 숨깁니다.
    /// 몬스터 사망, 공격 완료 등 외부에서 호출할 수 있습니다.
    /// </summary>
    public void HideMarker()
    {
        isHovering = false;
        if (markerObject != null)
            markerObject.SetActive(false);
    }
}
