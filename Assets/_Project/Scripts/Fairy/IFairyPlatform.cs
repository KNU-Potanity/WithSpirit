using UnityEngine;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 정령의 플랫폼 변신 기능을 추상화하는 공통 인터페이스.
    /// </summary>
    public interface IFairyPlatform
    {
        /// <summary>현재 플랫폼 변신이 가능한 상태인지 여부</summary>
        bool CanTransform { get; }

        /// <summary>현재 활성화된 플랫폼 인스턴스가 있거나 변신 중인지 여부</summary>
        bool HasActivePlatform { get; }

        /// <summary>지정한 마커가 현재 이 정령의 활성 플랫폼 상태인지 확인</summary>
        bool IsMyMarkerActive(PlatformClickMarker marker);

        /// <summary>목표 위치에 플랫폼 변신 요청</summary>
        void RequestTransform(Vector3 targetPosition, PlatformClickMarker marker = null);

        /// <summary>기존 변신을 해제하고 지정된 마커 위치로 즉시 재변신 요청</summary>
        void RequestTransformOrReplace(Vector3 targetPosition, PlatformClickMarker marker = null);

        /// <summary>플랫폼 변신 해제</summary>
        void RevertTransform();

        /// <summary>클릭 소비 등록 (빈 공간 해제 방지용)</summary>
        void MarkClickHandled();
    }
}
