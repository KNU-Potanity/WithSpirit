using UnityEngine;
using BasePlatformer.Fairy;

[CreateAssetMenu(fileName = "FairySelectUIData", menuName = "Scriptable Objects/UI/FairySelectUIData")]
public class FairySelectUIData : ScriptableObject
{
    [Header("특화 카테고리 텍스트 색상")]
    [Tooltip("플랫폼 특화 텍스트 색상")]
    public Color platformCategoryColor = new Color(0.2f, 0.7f, 1f, 1f);

    [Tooltip("공격 특화 텍스트 색상")]
    public Color attackCategoryColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("상태이상 설명 텍스트")]
    [TextArea(2, 4)]
    [Tooltip("화상 상태이상 설명 텍스트")]
    public string burnEffectDescription = "화상: 일정 시간 동안 적에게 지속적인 피해를 입힙니다.";

    [TextArea(2, 4)] [Tooltip("빙결 상태이상 설명 텍스트")]
    public string freezeEffectDescription = "빙결: 일정 시간 동안 적의 이동 속도를 크게 감소시키거나 행동을 정지시킵니다.";

    /// <summary>
    /// 카테고리/특화에 따른 색상을 반환합니다.
    /// </summary>
    public Color GetCategoryColor(bool isAttack)
    {
        return isAttack ? attackCategoryColor : platformCategoryColor;
    }

    /// <summary>
    /// 링크 ID (예: "burn", "freeze" 등)에 따른 상태이상 설명 텍스트를 반환합니다.
    /// </summary>
    public string GetStatusDescriptionByLinkId(string linkId)
    {
        if (string.IsNullOrEmpty(linkId)) return string.Empty;

        switch (linkId.ToLower().Trim())
        {
            case "burn":
            case "화상":
                return burnEffectDescription;
            case "freeze":
            case "빙결":
                return freezeEffectDescription;
            default:
                return string.Empty;
        }
    }
}
