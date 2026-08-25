using System.Collections.Generic;
using UnityEngine;
using BasePlatformer.Fairy;

/// <summary>
/// 게임에서 선택지로 등장할 수 있는 전체 정령 풀을 저장하는 ScriptableObject.
/// Assets 우클릭 → Create → Scriptable Objects → FairyPool 로 생성하세요.
/// </summary>
[CreateAssetMenu(fileName = "FairyPool", menuName = "Scriptable Objects/FairyPool")]
public class FairyPool : ScriptableObject
{
    [Tooltip("선택지 풀에 등록할 정령 목록")]
    public List<FairyInfo> fairies = new List<FairyInfo>();
}
