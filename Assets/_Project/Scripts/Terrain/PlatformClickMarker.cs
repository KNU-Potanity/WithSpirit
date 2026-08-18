using BasePlatformer.Fairy;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlatformClickMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    public GameObject markerObject;

    private Collider2D col;
    private bool isHovering = false;

    public bool IsHovering => isHovering;

    private void Start()
    {
        col = GetComponent<Collider2D>();
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

        // 호버 중일 때: 우클릭으로 정령이 변경되거나 플랫폼 활성 상태가 변할 수 있으므로 마커 색상을 실시간 갱신
        if (isHovering && markerObject != null && markerObject.activeSelf)
        {
            ApplyFairyColor();
        }

        if (isHovering && IsMarkerVisible() && Mouse.current.leftButton.wasPressedThisFrame && !monsterHovered)
        {
            IFairyPlatform fairyPlatform = GetTargetPlatformController();

            if (fairyPlatform != null)
            {
                // 클릭 소비 등록 → FairyPlatformController.LateUpdate의 빈공간 해제 억제
                fairyPlatform.MarkClickHandled();
                // 변신 중·완료 상태라도 즉시 이 마커 위치로 전환 (쿨다운 없이)
                fairyPlatform.RequestTransformOrReplace(transform.position, this);
                HideMarker();
            }
        }
    }

    private IFairyPlatform GetTargetPlatformController()
    {
        if (FairyManager.Instance != null)
        {
            return FairyManager.Instance.GetBestFairyForPlatform();
        }
        return FindAnyObjectByType<FairyPlatformController>();
    }

    private bool IsPlatformInstalled()
    {
        // 씬 내 어떤 플랫폼 컨트롤러라도 이 마커를 현재 사용 중인지 확인
        if (FairyManager.Instance != null)
        {
            int count = FairyManager.Instance.TotalFairyCount;
            for (int i = 0; i < count; i++)
            {
                var fairy = FairyManager.Instance.GetFairyAtIndex(i);
                if (fairy == null) continue;

                var platCtrl = fairy.GetComponent<IFairyPlatform>();
                if (platCtrl != null && platCtrl.IsMyMarkerActive(this))
                    return true;
            }
            return false;
        }

        var singlePlatform = FindAnyObjectByType<FairyPlatformController>();
        return singlePlatform != null && singlePlatform.IsMyMarkerActive(this);
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
        {
            ApplyFairyColor();
            markerObject.SetActive(true);
        }
    }

    private void ApplyFairyColor()
    {
        if (FairyManager.Instance == null) return;

        // 실제로 이 위치에 플랫폼을 생성하러 출격할 정령의 색상 획득
        IFairyPlatform platformTarget = FairyManager.Instance.GetBestFairyForPlatform();
        Color fairyColor = FairyManager.Instance.GetFairyColor(platformTarget);

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

    public void HideMarker()
    {
        isHovering = false;
        if (markerObject != null)
            markerObject.SetActive(false);
    }
}
