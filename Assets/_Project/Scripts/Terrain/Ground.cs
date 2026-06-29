using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 일반 발판(지형) 마커 컴포넌트.
    ///
    /// 스펙 참조: 기술기획서_ver4.md 4.2장 - 지형 타입 구분(enum TerrainType)을
    /// 콜라이더에 직접 붙여서 코드에서 "이 콜라이더가 어떤 지형인지" 조회할 수 있게 한다.
    ///
    /// 지금은 Type이 항상 Normal인 발판(Ground Tilemap)에만 붙어있지만,
    /// 추후 이동 발판/스프링 등을 추가할 때 이 컴포넌트를 상속하거나 Type을 바꿔서
    /// 재사용할 수 있도록 설계했다 (4.2장 설계 의도).
    /// </summary>
    public class Ground : MonoBehaviour
    {
        [SerializeField] private TerrainType type = TerrainType.Normal;

        public TerrainType Type => type;
    }
}
