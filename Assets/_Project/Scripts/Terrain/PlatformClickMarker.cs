using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlatformClickMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    public GameObject markerObject;

    private FairyPlatformController fairyPlatform;
    private Collider2D col;
    private bool isHovering = false;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        fairyPlatform = FindAnyObjectByType<FairyPlatformController>();

        if (fairyPlatform == null)
            Debug.LogWarning("[PlatformClickMarker] 씬에서 FairyPlatformController를 찾을 수 없습니다!");

        HideMarker();
    }

    private void Update()
    {
        if (IsPlatformInstalled())
        {
            if (isHovering)
                HideMarker();
            return;
        }

        if (Camera.main == null || Mouse.current == null)
            return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool monsterHovered = IsAnyMonsterMarkerHovered(mouseWorldPos);

        bool over = col.OverlapPoint(mouseWorldPos) && !monsterHovered;

        if (over && !isHovering)
        {
            isHovering = true;
            ShowMarker();
        }
        else if (!over && isHovering)
        {
            isHovering = false;
            HideMarker();
        }

        if (isHovering && IsMarkerVisible() && Mouse.current.leftButton.wasPressedThisFrame && !monsterHovered)
        {
            if (fairyPlatform != null)
            {
                // 클릭 소비 등록 → FairyPlatformController.LateUpdate의 빈공간 해제 억제
                fairyPlatform.MarkClickHandled();
                // 변신 중·완료 상태라도 즉시 이 마커 위치로 전환 (쿨다운 없이)
                // 프리팹은 FairyPlatformController가 PlatformFairyData에서 읽음
                fairyPlatform.RequestTransformOrReplace(transform.position, this);
                HideMarker();
            }
        }
    }

    private bool IsPlatformInstalled()
    {
        // 내 마커가 현재 활성 상태일 때만 true → 다른 마커가 활성돼도 이 마커는 클릭 가능
        return fairyPlatform != null && fairyPlatform.IsMyMarkerActive(this);
    }

    private bool IsMarkerVisible()
    {
        return markerObject != null && markerObject.activeInHierarchy;
    }

    private bool IsAnyMonsterMarkerHovered(Vector2 mouseWorldPos)
    {
        MonsterClickMarker[] monsterMarkers = FindObjectsByType<MonsterClickMarker>();
        foreach (var monsterMarker in monsterMarkers)
        {
            if (monsterMarker == null || !monsterMarker.enabled || !monsterMarker.gameObject.activeInHierarchy)
                continue;

            Collider2D monsterCol = monsterMarker.GetComponent<Collider2D>();
            if (monsterCol != null && monsterCol.OverlapPoint(mouseWorldPos))
                return true;
        }

        return false;
    }

    private void ShowMarker()
    {
        if (markerObject != null)
            markerObject.SetActive(true);
    }

    public void HideMarker()
    {
        isHovering = false;
        if (markerObject != null)
            markerObject.SetActive(false);
    }
}
