# 몬스터 기획서

## 0. 문서 목적

> 마지막 수정일: 2026-07-23 (5장·6.2절을 실제 상태 머신(Patrol/Chase/Attack) 구현 기준으로 갱신 — ver9)

확장 단계에서 추가되는 몬스터의 종류, 분류, 테마별 배치를 정리합니다. 스탯 항목은 `스탯_기획서.md`를 참고하세요.

---

## 1. 공통 규칙

- 몬스터는 닿으면 플레이어에게 데미지를 입힙니다 (하트 1 감소, `월드_스테이지_구성.md` 6장 참고).
- 정령으로 몬스터를 클릭하면 공격해 데미지를 줄 수 있습니다 (`정령_시스템_기획서.md` 2장 참고).

---

## 2. 몬스터 분류

| 분류 | 설명 |
|---|---|
| 디버프형 | 직접 데미지는 주지 않지만 접촉 시 플레이어에게 디버프 부여 |
| 근거리형 | 근접 접촉 시 데미지 |
| 원거리형 | 투사체 등으로 원거리 공격 |
| 보스 | 최종 스테이지의 보스 몬스터 |

---

## 3. 테마별 몬스터 배치

| 분류 | 몬스터 | 등장 테마 | 비고 |
|---|---|---|---|
| 디버프형 | 슬라임 | 테마 2 (변방) | 접촉 시 이동 속도 감소 |
| 근거리형 | 벌 | 테마 1 (마을·도시) | 공중형 — 하늘을 날아다니며 접근 |
| 근거리형 | 버섯 | 테마 2 (변방) | |
| 근거리형 | 나무 골렘 | 테마 3 (숲) | |
| 근거리형 | 식충식물 | 테마 3 (숲) | 근접형 — 제자리에서 근접 범위 공격 (기존 원거리형 '덤불'에서 변경) |
| 원거리형 | 스켈레톤 | 테마 2 (변방) | |
| 보스 | 밤 나무 | 테마 3 (숲, 최종 스테이지) | 최종 보스 — 처치가 게임 클리어 조건 |

> 테마(월드) 구성은 `월드_스테이지_구성.md`를 참고하세요.

---

## 4. 에셋 경로

| 몬스터 | 에셋 경로 |
|---|---|
| 슬라임 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Slime` |
| 벌 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Bee` |
| 버섯 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Mushroom` |
| 나무 골렘 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Plant Treant` |
| 스켈레톤 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Skeleton Archer` |
| 식충식물 | `Assets/craftpix-net-284465-free-predator-plant-mobs-pixel-art-pack/PNG/Plant1/Without_shadow` |
| 밤 나무 | `Assets/craftpix-net-838021-top-down-pixel-ent-character-sprites/PNG/Ent3/Without_shadow` |

> 위 3개 에셋 팩(`100 Top Down Monsters - Pixel Art - Vol 1`, `craftpix-net-284465-...`, `craftpix-net-838021-...`)의 출처·라이선스는 `에셋_출처_라이선스_관리.md`에 별도로 기록합니다.

---

## 5. AI 이동 로직 — 지상 근접몹 (2026-07-23, 실제 구현 기준으로 갱신)

공중형(벌)을 제외한 근거리형 몬스터(버섯, 나무 골렘, 식충식물)의 이동 방식입니다. 초기 설계(x축 경로만 확인)에서 더 발전해, 실제로는 **Patrol/Chase/Attack 3단계 상태 머신**으로 구현되어 있습니다 (`GroundMonsterMovement.cs`).

| 상태 | 전환 조건 | 동작 |
|---|---|---|
| Patrol (순찰, 기본) | 플레이어가 감지 범위 밖이거나, 같은 플랫폼이 아니거나, 경로가 막힘 | 자신이 위치한 플랫폼 내에서 왔다갔다 이동 (낭떠러지·벽 감지 시 방향 전환) |
| Chase (추격) | 수평 감지 범위(`detectionRangeX`) 안 **AND** 같은 플랫폼 **AND** 경로가 막히지 않음 | 플레이어를 향해 이동, 방향이 다르면 즉시 반전(`ForceFlip`) |
| Attack (공격) | Chase 중 플레이어와의 거리가 `attackRange`(수평) · `attackRangeY`(수직) 이내로 들어옴 | 제자리에 멈추고, 쿨타임(`attackCooldown`)마다 데미지 적용 + 공격 애니메이션 트리거 |

**"같은 플랫폼" 판정 방식**: 초기 설계처럼 단순 x축 범위가 아니라, 씬의 `CompositeCollider2D`(바닥)에서 각 플랫폼의 x범위·상단 Y좌표를 미리 추출해두고, 몬스터와 플레이어가 **동일한 플랫폼의 x범위 안 + (상단Y ~ 상단Y+`platformYOffset`) y범위 안**에 함께 있는지로 판정합니다. `platformYOffset`을 캐릭터 키 이상으로 넉넉히 잡아두었기 때문에, 플레이어가 점프해서 y가 살짝 달라지는 정도는 대부분 같은 플랫폼으로 인정되어 초기 설계 의도(점프로 인한 사소한 y 변화는 무시)와 실질적으로 유사하게 동작합니다.

**"경로가 막히지 않음" 판정 방식**: 플레이어 방향으로 발끝·몸통·머리 높이 3줄의 레이캐스트를 쏴서 장애물(가시 등)이 있는지 확인하고, 몬스터와 플레이어 사이 중간 지점 아래로 레이를 쏴서 낭떠러지가 있는지도 함께 확인합니다. 둘 중 하나라도 걸리면 추격을 멈추고 Patrol로 돌아갑니다.

- 공중형(벌)은 이 로직과 별개로 처리합니다 (지형에 구애받지 않고 비행 이동, 추후 구체화 예정).

---

## 6. 애니메이션 구현 방식

몬스터 애니메이션은 5종(멈춤/걷기/공격/데미지 받음/죽음)으로 구성하되, **Animator와 스크립트의 역할을 분리**합니다.

| 애니메이션 | 구현 방식 |
|---|---|
| 멈춤 (Idle) | Animator State — 에셋의 Idle 클립 연결 |
| 걷기 (Walk) | Animator State — 에셋의 Move 클립 연결, `Speed` 파라미터로 Idle과 전환 |
| 공격 (Attack) | Animator State — 에셋의 Attack 클립 연결, `Attack` 트리거로 Any State에서 진입 후 Exit Time으로 복귀 |
| 데미지 받음 | Animator 상태로 만들지 않음 — `MonsterHealth.cs`가 `SpriteRenderer.color`를 빨간색으로 잠깐 바꿨다가 원래 색으로 되돌리는 방식으로 처리 (현재 Animator 상태와 무관하게 항상 동작) |
| 죽음 | Animator 비활성화(마지막 프레임 정지) + `MonsterHealth.cs`가 알파값을 1→0으로 페이드시킨 뒤 오브젝트 제거 |

이렇게 나누는 이유: Idle/Walk/Attack처럼 "지금 뭘 하고 있는지"는 Animator가, 데미지/죽음처럼 "상태 변화에 대한 시각 피드백"은 스크립트가 담당하게 하면 State Machine이 단순해지고, 몬스터마다 Idle/Walk/Attack 클립만 갈아끼우면 되므로 재사용이 쉬워집니다.

### 6.1 샘플 구현 (2026-07-16)

슬라임 에셋으로 위 구조의 샘플을 실제로 만들어 확인했습니다.

| 항목 | 경로 |
|---|---|
| Animator Controller (7종 전체) | `Assets/_Project/Animators/Monsters/AC_Monster_이름.controller` — Slime/Bee/Mushroom/TreeGolem/Skeleton/CarnivorousPlant/Boss_Ent 7개 모두 생성 완료 (State: Idle/Walk/Attack, Parameter: `Speed`(Float)/`Attack`(Trigger) 공통 구조). `AC_Monster_Slime`만 슬라임 클립이 실제로 연결되어 있고, 나머지 6개는 State/Transition 구조만 있고 Motion은 비어있어 담당자가 각자 클립만 채워 넣으면 됩니다. |
| 데미지/죽음 스크립트 | `Assets/_Project/Scripts/Monster/MonsterHealth.cs` (`TakeDamage(int)` 호출 시 빨간색 깜빡임, 체력 0 이하가 되면 페이드 아웃 후 제거) |
| 좌우 반전 스크립트 | `Assets/_Project/Scripts/Monster/MonsterFacing.cs` (오른쪽 클립만 사용, 이동 방향에 따라 `flipX` 자동 반전) |

- 다른 몬스터에 적용할 때는 Animator Controller를 복제해 Idle/Walk/Attack 클립만 해당 몬스터 것으로 교체하고, `MonsterHealth.cs`는 그대로 부착하면 됩니다.
- 모든 몬스터 애니메이션 클립은 **오른쪽을 바라보는 것만** 사용합니다. 왼쪽으로 이동할 때는 별도 클립 없이 `MonsterFacing.cs`가 `SpriteRenderer.flipX`로 좌우 반전을 처리합니다 (`PlayerAnimatorController.cs`와 동일한 방식). `Rigidbody2D`의 x축 속도를 자동으로 읽어 반전하며, 속도가 거의 0이면(정지 시) 마지막 방향을 유지합니다.
- `MonsterHealth.cs`의 `Die()` 안에서 실제 AI 스크립트를 비활성화하는 부분은 각 몬스터의 실제 AI 스크립트명으로 교체가 필요합니다 (현재는 플레이스홀더).

---

### 6.2 실제 적용 사례 — 버섯(`GroundMonsterMovement.cs`) (2026-07-16 최초 적용, 2026-07-23 갱신)

지상 근접몹(5장)의 실제 이동 스크립트인 `Assets/_Project/Scripts/Enemy/GroundMonsterMovement.cs`에 6장의 Animator 연동을 적용하며 발견·수정한 내용입니다.

**최초 적용 시 (충돌 기반 버전, 2026-07-16)**

| 문제 | 원인 | 수정 |
|---|---|---|
| 이동해도 Idle에 고정 | `Animator.SetFloat("Speed", ...)`를 호출하는 코드가 아예 없었음 | 매 프레임 실제 속도를 `Speed` 파라미터에 반영하도록 추가 |
| 공격 애니메이션이 재생되지 않음 | `Animator.SetTrigger("Attack")` 호출이 없었음 | 플레이어 접촉 시 트리거 호출 |
| 공격 중에도 계속 움직이거나 튕겨나감 | 방향 전환 판정이 공격 판정보다 먼저 실행됨 + 물리 엔진이 겹친 Dynamic Rigidbody를 자동으로 밀어냄 | 판정 순서 변경 + 공격 중 `Rigidbody2D.bodyType`을 `Kinematic`으로 전환 |

**⚠️ 이후 팀원이 스크립트를 Patrol/Chase/Attack 상태 머신 버전(5장 참고)으로 전면 재작성하면서, 위 충돌 기반 로직(`OnCollisionEnter2D` 판정, `Kinematic` 전환, `DoAttack` 코루틴)은 모두 대체되었습니다.** 이 과정에서 병합 충돌로 Animator 연동 부분이 유실되어, 새 상태 머신 구조에 맞게 다시 적용했습니다 (2026-07-23).

**현재 버전 (상태 머신 기반)**

| 항목 | 구현 위치 | 내용 |
|---|---|---|
| Speed 갱신 | `FixedUpdate()` 마지막 | 매 프레임 `animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x))` |
| Attack 트리거 | `ExecuteAttack()` | Attack 상태에서 쿨타임이 돌아와 실제로 데미지를 주는 순간 `animator.SetTrigger("Attack")` 호출 |
| 공격 중 정지 | `ExecuteAttack()` | Kinematic 전환 없이, Attack 상태인 동안 `rb.linearVelocity`의 x를 0으로 유지하는 것만으로 처리 (상태 머신이 Chase로 안 넘어가는 한 계속 정지) |

- `Animator`는 자식 오브젝트(예: `Mushroom_Move_0`)에 붙어있고 `GroundMonsterMovement`는 부모에 붙어있으므로, `GetComponentInChildren<Animator>()`로 참조합니다.
- 이 패턴(Speed 갱신 + Attack 트리거)은 다른 지상 근접몹(나무 골렘, 식충식물)에도 동일하게 적용 예정입니다.

### 6.3 스탯 데이터 (ScriptableObject 기반, 2026-07-16 확인)

- 몬스터 스탯은 `MonsterData`/`MeleeMonsterData`/`RangedMonsterData`/`DebuffMonsterData` (`Assets/_Project/Scripts/ScriptableObjects/EnemyData/`) `ScriptableObject`로 구현되어 있으며, 필드가 `스탯_기획서.md` 3장 항목과 대응됩니다. 자세한 필드는 `스크립트_설명서.md` 참고.
- 단, `RangedMonsterData`의 `ProjectileTypes`와 `DebuffMonsterData`의 `DebuffTypes` enum이 아직 **값이 비어있는 상태**라, 실제 종류(화살, 이동속도감소 등)를 채워 넣는 작업이 남아있습니다.

---

## 7. 다음 단계

- 각 몬스터의 세부 수치(체력, 이동 속도, 공격력, 쿨타임 등)는 `스탯_기획서.md` 3장을 채워 확정
- 몬스터별 공격 패턴·모션은 아트·기획 협업으로 추후 구체화
- 보스(밤 나무) 패턴 설계는 별도 문서로 분리 예정
- 공중형(벌)의 이동 로직은 5장과 별도로 추후 구체화
- `MonsterHealth.cs`의 AI 스크립트 비활성화 부분을 실제 몬스터별 AI 스크립트로 연결
- 나머지 6개 Animator Controller(Bee/Mushroom/TreeGolem/Skeleton/CarnivorousPlant/Boss_Ent)에 실제 클립 채워넣기
- 6.2절의 Speed/Attack/Kinematic 패턴을 나무 골렘·식충식물 이동 스크립트에도 동일 적용
- `MonsterFacing.cs`를 실제 몬스터 오브젝트(예: MushroomMarker)에 부착
