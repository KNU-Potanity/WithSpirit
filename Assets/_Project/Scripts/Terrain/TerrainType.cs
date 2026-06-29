namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 지형 타입 구분.
    ///
    /// 스펙 참조: 기술기획서_ver4.md 4.2장
    ///   "지형 타입은 enum TerrainType { Normal, Hazard } 형태로 코드 상에서 구분
    ///    -> 추후 이동 발판, 스프링 등 추가 시 타입만 늘리면 되도록 설계"
    ///
    /// Normal: 일반 발판 (이 ToDo, "일반 발판 충돌 처리"에서 다룸)
    /// Hazard: 즉사 장애물 (가시 등, 별도 ToDo "즉사 장애물 판정 + 리스폰 트리거 연결"에서 다룸.
    ///         가시는 Tilemap이 아닌 개별 GameObject + 트리거 콜라이더로 구현되어 있어,
    ///         물리적 충돌(OnCollision)이 아니라 트리거(OnTrigger)로 감지된다.)
    /// </summary>
    public enum TerrainType
    {
        Normal,
        Hazard
    }
}
