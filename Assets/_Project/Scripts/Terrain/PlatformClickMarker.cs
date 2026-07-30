using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlatformClickMarker : MonoBehaviour
{
    [Header("Marker Settings")]
    public GameObject markerObject;

    [Header("Platform Target")]
    public GameObject targetPlatformObject;

    private FairyPlatformController fairyPlatform;
    private Collider2D col;
    private bool isHovering = false;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        fairyPlatform = FindAnyObjectByType<FairyPlatformController>();

        if (fairyPlatform == null)
            Debug.LogWarning("[PlatformClickMarker] 씬에서 FairyPlatformController를 찾을 수 없습니다!");

        if (targetPlatformObject != null)
            targetPlatformObject.SetActive(false);

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
            if (fairyPlatform != null && targetPlatformObject != null && fairyPlatform.CanTransform)
            {
                fairyPlatform.RequestTransform(transform.position, targetPlatformObject);
                HideMarker();
            }
        }
    }

    private bool IsPlatformInstalled()
    {
        if (fairyPlatform != null && fairyPlatform.HasActivePlatform)
            return true;

        return targetPlatformObject != null && targetPlatformObject.activeInHierarchy;
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
