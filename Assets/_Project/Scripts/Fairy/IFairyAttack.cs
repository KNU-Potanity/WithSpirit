using BasePlatformer.Monsters;
using UnityEngine;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 정령의 공격 기능을 추상화하는 공통 인터페이스.
    /// </summary>
    public interface IFairyAttack
    {
        /// <summary>현재 공격이 가능한 상태인지 여부</summary>
        bool CanAttack { get; }

        /// <summary>공격 대상에게 공격을 요청합니다.</summary>
        void RequestAttack(MonsterHealth target);
    }
}
