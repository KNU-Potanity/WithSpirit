using System.Collections.Generic;
using UnityEngine;

namespace BasePlatformer.Player
{
    /// <summary>
    /// 일반 발판 충돌 처리 - 접지 판정 (ToDo: "일반 발판 충돌 처리")
    ///
    /// 기존 PlayerJump.cs에 있던 임시 구현(발 밑 OverlapBox로 Ground 레이어 감지)을
    /// 정식 충돌 이벤트(OnCollisionEnter2D/Stay2D/Exit2D) 기반으로 교체한 컴포넌트.
    ///
    /// 동작 방식:
    ///   - 플레이어의 BoxCollider2D가 지형의 Collider2D(TilemapCollider2D + CompositeCollider2D)와
    ///     실제로 물리 충돌할 때 발생하는 접촉점(ContactPoint2D)의 법선(normal)을 확인한다.
    ///   - 법선이 위쪽(world up)에 가까운 접촉만 "발판 위에 서있는 상태"로 인정한다.
    ///     (옆면에 부딪힌 경우는 법선이 거의 수평이라 제외됨 -> 벽에 닿았다고 점프 가능 상태가 되는 일이 없음)
    ///   - 즉사 장애물(가시)은 트리거 콜라이더로 구현되어 있어 OnCollision 콜백 자체가 발생하지
    ///     않으므로(Trigger는 OnTrigger로 별도 처리), 이 컴포넌트는 자연히 일반 발판(Normal)만 다룬다.
    ///
    /// 여러 콜라이더와 동시에 접촉할 수 있으므로(예: 평평한 발판이 여러 타일로 나뉜 경우),
    /// "현재 접지로 인정된 콜라이더 집합"을 들고 있다가 비어있지 않으면 IsGrounded = true로 본다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerGroundDetector : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [Tooltip("이 값보다 위쪽에 가까운 접촉 법선만 '바닥'으로 인정한다 (0~1, 1에 가까울수록 거의 수직으로 위를 향해야 함). 0.9 = 약 25.8도 이내만 인정 (발판 모서리/코너의 대각선 접촉을 바닥으로 오인하지 않도록)")]
        private float minUpwardNormalY => playerData.minUpwardNormalY;

        private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();

        /// <summary> 현재 접지 상태 여부 (발판 위에 서있는지) </summary>
        public bool IsGrounded => groundedColliders.Count > 0;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            RefreshContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            RefreshContact(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            groundedColliders.Remove(collision.collider);
        }

        private void RefreshContact(Collision2D collision)
        {
            bool isStandingOnTop = false;

            int contactCount = collision.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.normal.y >= minUpwardNormalY)
                {
                    isStandingOnTop = true;
                    break;
                }
            }

            if (isStandingOnTop)
            {
                groundedColliders.Add(collision.collider);
            }
            else
            {
                groundedColliders.Remove(collision.collider);
            }
        }
    }
}
