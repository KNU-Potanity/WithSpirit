using BasePlatformer.Monsters;
using BasePlatformer.Fairy;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 몬스터의 자식 오브젝트에 붙이는 마커 컴포넌트.
/// Update()에서 OverlapPoint로 마우스 위치를 직접 감지하므로
/// 부모 오브젝트의 콜라이더나 Physics 2D Raycaster 세팅과 무관하게 동작합니다.
/// 이 오브젝트의 Collider2D 크기 = 마우스 인식 범위(에임 보정)
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
    private Transform parentTransform;
    private MonsterHealth monsterHealth;
    private Collider2D col;

    private bool isHovering = false; // 현재 마우스가 이 콜라이더 위에 있는지

    public bool IsHovering => isHovering;

    // ─────────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────────
    private void Start()
    {
        col = GetComponent<Collider2D>();
        parentTransform = transform.parent;

        // 부모 오브젝트에서 체력 컴포넌트를 찾음
        monsterHealth = GetComponentInParent<MonsterHealth>();
        if (monsterHealth == null)
            Debug.LogWarning($"[MonsterClickMarker] {gameObject.name} 의 부모에서 GroundMonsterHealth를 찾을 수 없습니다!");

        // 마커는 처음에 꺼둠
        HideMarker();
    }

    // ─────────────────────────────────────────────
    //  매 프레임 마우스 위치 감지
    // ─────────────────────────────────────────────
    private void Update()
    {
        if (Camera.main == null || Mouse.current == null) return;

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

        // 호버 중일 때: 우클릭으로 정령이 변경되거나 쿨다운 상태가 변할 수 있으므로 마커 색상을 실시간 갱신
        if (isHovering && markerObject != null && markerObject.activeSelf)
        {
            ApplyFairyColor();
        }

        // 마우스가 올라온 상태에서 왼쪽 클릭
        if (isHovering && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 플랫폼 활성 상태면 먼저 해제 후 공격
            if (FairyManager.Instance != null)
            {
                FairyManager.Instance.RevertAllActivePlatforms();

                // 선택된 정령에서 가장 가까운 공격 가능한 정령을 찾아서 공격 요청
                IFairyAttack attackTarget = FairyManager.Instance.GetBestFairyForAttack();
                if (attackTarget != null && monsterHealth != null)
                {
                    attackTarget.RequestAttack(monsterHealth);
                }
            }
            else
            {
                // Fallback: 씬 내 단일 컨트롤러 검색
                var fairyAttack = FindAnyObjectByType<FairyAttackController>();
                if (fairyAttack != null && monsterHealth != null)
                {
                    fairyAttack.RequestAttack(monsterHealth);
                }
            }
        }
    }

    // ─────────────────────────────────────────────
    //  마커 표시/숨김
    // ─────────────────────────────────────────────
    private void ShowMarker()
    {
        if (markerObject != null)
        {
            ApplyFairyColor();
            markerObject.SetActive(true);
        }
    }

    private void ApplyFairyColor()
    {
        if (FairyManager.Instance == null) return;

        // 실제로 이 몬스터를 공격하러 출격할 정령의 색상 획득
        IFairyAttack attackTarget = FairyManager.Instance.GetBestFairyForAttack();
        Color fairyColor = FairyManager.Instance.GetFairyColor(attackTarget);

        // markerObject 또는 자식의 SpriteRenderer 또는 UI Image 색상 적용
        var sr = markerObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = markerObject.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = fairyColor;
            return;
        }

        var img = markerObject.GetComponent<UnityEngine.UI.Image>();
        if (img == null) img = markerObject.GetComponentInChildren<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = fairyColor;
        }
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
    
    void LateUpdate()
    {
        if (parentTransform == null) return;

        // 부모의 Scale.x가 음수이면 자식의 Scale.x도 -1을 곱해 상쇄시킵니다.
        Vector3 currentScale = transform.localScale;
        
        float parentSignX = Mathf.Sign(parentTransform.lossyScale.x);
        currentScale.x = Mathf.Abs(currentScale.x) * parentSignX;

        transform.localScale = currentScale;
    }
}
