# 몬스터 기획서

## 0. 문서 목적

> 마지막 수정일: 2026-08-06 (7.3·7.4절 보스 패턴 구체적 수치·선택 로직 추가 — ver21)

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
| 근거리형 | 벌 | 테마 1 (마을·도시) | 공중형 — 하늘을 날아다니며 접근 |
| 근거리형 | 버섯 | 테마 2 (변방) | |
| 원거리형 | 스켈레톤 | 테마 2 (변방) | |
| 디버프형 | 슬라임 | 테마 2 (변방) | 접촉 시 이동 속도 감소 |
| 근거리형 | 나무 골렘 | 테마 3 (숲) | |
| 보스 | 밤 나무 | 테마 3 (숲, 최종 스테이지) | 최종 보스 — 처치가 게임 클리어 조건 |
| 근거리형 | 식충식물 | 테마 3 (숲) | 근접형 — 제자리에서 근접 범위 공격 (기존 원거리형 '덤불'에서 변경) |

> 테마(월드) 구성은 `월드_스테이지_구성.md`를 참고하세요.

---

## 4. 에셋 경로

| 몬스터 | 에셋 경로 |
|---|---|
| 벌 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Bee` |
| 버섯 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Mushroom` |
| 스켈레톤 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Skeleton Archer` |
| 슬라임 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Slime` |
| 나무 골렘 | `Assets/100 Top Down Monsters - Pixel Art - Vol 1/Monsters/Plant Treant` |
| 밤 나무 | `Assets/craftpix-net-838021-top-down-pixel-ent-character-sprites/PNG/Ent3/Without_shadow` |
| 식충식물 | `Assets/craftpix-net-284465-free-predator-plant-mobs-pixel-art-pack/PNG/Plant1/Without_shadow` |

> 위 3개 에셋 팩(`100 Top Down Monsters - Pixel Art - Vol 1`, `craftpix-net-284465-...`, `craftpix-net-838021-...`)의 출처·라이선스는 `에셋_출처_라이선스_관리.md`에 별도로 기록합니다.

---

## 5. AI 이동 로직 — 지상 근접몹 (2026-07-23, 실제 구현 기준으로 갱신)

공중형(벌)을 제외한 근거리형 몬스터(버섯, 나무 골렘, 식충식물)의 이동 방식입니다. 초기 설계(x축 경로만 확인)에서 더 발전해, 실제로는 **Patrol/Chase/Attack 3단계 상태 머신**으로 구현되어 있습니다 (`GroundMonsterMovement.cs`).

| 상태 | 전환 조건 | 동작 |
|---|---|---|
| Patrol (순찰, 기본) | 플레이어가 감지 범위 밖이거나, 같은 플랫폼이 아니거나, 경로가 막힘 | 자신이 위치한 플랫폼 내에서 왔다갔다 이동 (낭떠러지·벽 감지 시 방향 전환) |
| Chase (추격) | 수평 감지 범위(`detectionRangeX`) 안 **AND** 같은 플랫폼 **AND** 경로가 막히지 않음 | 플레이어를 향해 이동, 방향이 다르면 즉시 반전(`ForceFlip`) |
| Attack (공격) | Chase 중 플레이어와의 거리가 `AttackRangeX`(수평) · `AttackRangeY`(수직) 이내로 들어옴 | 제자리에 멈추고, 쿨타임(`CoolTime`)마다 공격 애니메이션 트리거 → **`AttackAnimDelay`초 대기(공격 예비 동작/텔레그래프) 후** 그 시점에도 여전히 범위 안이면 데미지 적용 (플레이어가 타이밍 맞춰 회피 가능) |

**공격 판정 방향 제한 (2026-07-28 결정)**: 현재 몬스터 공격 애니메이션은 **좌우 방향만** 제작되어 있고 위쪽에서 때리는 모션은 없습니다. 애니메이션과 실제 판정이 어긋나지 않도록, 몬스터가 플레이어를 공격할 때는 **`AttackRangeY`를 좁게 잡아 몬스터 기준 좌우에서만 데미지가 들어가도록** 제한합니다 (몬스터 머리 위에 있을 때는 공격 판정에서 제외). 정령이 몬스터를 공격하는 쪽에는 이 제약이 적용되지 않습니다 — 몬스터가 플레이어를 공격하는 방향에만 해당하는 제약입니다.

**"같은 플랫폼" 판정 방식 (2026-07-28 갱신)**: 단순 x축 범위나 y범위 밴드가 아니라, **몬스터와 플레이어 발밑으로 각각 레이캐스트를 쏴서 실제로 닿아있는 바닥의 Y좌표를 구하고, 그 값이 서로 거의 같은지(오차 0.2 이내)** 로 판정합니다. 추가로 두 위치가 씬의 `CompositeCollider2D`(바닥)에서 추출한 동일 플랫폼의 x범위 안에 있는지도 함께 확인합니다. 발밑 바닥 Y좌표를 직접 비교하는 방식이라, 플레이어가 점프해서 일시적으로 y가 달라져도(공중에 떠 있는 순간) 착지한 바닥 기준으로는 여전히 같은 층으로 인정되어, 초기 설계 의도(점프로 인한 사소한 y 변화는 무시)와 부합합니다.

**"경로가 막히지 않음" 판정 방식**: 플레이어 방향으로 발끝·몸통·머리 높이 3줄의 레이캐스트를 쏴서 장애물(가시 등)이 있는지 확인하고, 몬스터와 플레이어 사이 중간 지점 아래로 레이를 쏴서 낭떠러지가 있는지도 함께 확인합니다. 둘 중 하나라도 걸리면 추격을 멈추고 Patrol로 돌아갑니다.

- 공중형(벌)은 이 로직과 별개로 처리합니다 (지형에 구애받지 않고 비행 이동).

### 5.1 공중형(벌) 이동 로직 — 구현 완료 (2026-07-23 확인, `FloatingMonsterMovement.cs`)

지상 근접몹과 마찬가지로 **Patrol/Chase/Attack 상태 머신**이지만, 지형(플랫폼)에 구애받지 않고 자유롭게 날아다니는 방식으로 구현되어 있습니다.

| 상태 | 전환 조건 | 동작 |
|---|---|---|
| Patrol (정지, 기본) | 플레이어가 `chaseRange` 밖이거나 시야가 막힘 | 제자리에서 부드럽게 정지 (`SmoothDamp`로 현재 위치 유지) |
| Chase (추격) | 플레이어가 `chaseRange` 안 **AND** 장애물에 막히지 않은 시야(Line of Sight)가 확보됨 | `SmoothDamp`로 플레이어 위치를 향해 자유롭게 이동 (지상 몹과 달리 플랫폼 제약 없음), 좌우 반전으로 플레이어 방향을 바라봄 |
| Attack (공격) | Chase 중 `AttackRangeX`(수평)·`AttackRangeY`(수직) 이내로 들어오고 쿨타임(`CoolTime`)이 끝남 | 정지 + 공격 애니메이션 트리거 → `AttackAnimDelay`초 대기 후 범위 안이면 데미지+넉백 적용 → (쿨타임 − 딜레이)만큼 추가 대기 후 Patrol로 복귀 |

- 지상 근접몹의 "같은 플랫폼" 판정 대신, 단순 **직선 레이캐스트(Line of Sight)**로 장애물 유무만 확인합니다 — 벌은 날아다니므로 낭떠러지/플랫폼 개념 자체가 적용되지 않습니다.
- 씬에는 `Bee` 오브젝트(2026-08-06 `Bee.prefab` 인스턴스로 재배치, 기존 `BeeMarker`에서 이름 변경)로 적용되어 있으며, Animator와 `AC_Monster_Bee.controller` 연결까지 완료된 상태입니다.

### 5.2 디버프형(슬라임) 이동/공격 로직 — 구현 완료 (2026-08-06 추가, `DebuffMonsterMovement.cs`)

지상 근접몹과 동일한 **Patrol/Chase/Attack 상태 머신**(`GroundMonsterMovement.cs`와 거의 같은 구조)이며, 공격이 닿으면 데미지+넉백에 더해 **둔화 디버프**를 함께 적용합니다.

| 상태 | 전환 조건 | 동작 |
|---|---|---|
| Patrol | 플레이어가 감지 범위 밖 | 왔다갔다 순찰, 벽/장애물 감지 시 방향 전환 |
| Chase | 감지 범위 안 | 플레이어를 향해 이동, 방향 다르면 즉시 반전 |
| Attack | `AttackRangeX`·`AttackRangeY` 이내 | 정지 → 공격 애니메이션 → `AttackAnimDelay`초 후 범위 안이면 데미지+넉백+둔화 디버프 적용 |

- **"같은 플랫폼" 판정 없이 단순 감지 범위(`detectionRange`)만 사용**합니다 — 지상 근접몹(5장)의 발밑 레이캐스트 방식보다 단순화된 버전입니다.
- 둔화 디버프는 `PlayerMovement.ApplySlowDebuff(감소율, 지속시간)`을 호출해 적용하며, 수치는 `DebuffMonsterData`(`SpeedDecreaseRate`, `Duration`)에서 가져옵니다 — 4.1절과 필드가 1:1로 대응합니다.
- 씬에는 `Slime` 오브젝트(`Slime.prefab` 인스턴스, `SlimeData` 연결)로 배치되어 있습니다.

### 5.3 원거리형(스켈레톤) 이동/공격 로직 및 투사체 시스템 — 구현 완료 (2026-08-06 추가, `RangedMonsterMovement.cs` + `MonsterProjectile.cs`)

| 상태 | 전환 조건 | 동작 |
|---|---|---|
| Idle (제자리, 기본) | 플레이어가 감지 범위 밖 | 이동 없이 대기 |
| Chase | 감지 범위 안 | 플레이어를 향해 이동 |
| Attack | `AttackRangeX`·`AttackRangeY` 이내 | 정지 → 공격 애니메이션 → `AttackAnimDelay`초 후 `RangedMonsterData.ProjectilePrefab`을 `spawnPoint`(자식 오브젝트 `FirePoint`) 위치에서 플레이어 방향으로 생성 |

**투사체(`MonsterProjectile.cs`) 동작 방식**:
- 지정된 방향으로 직선 이동(`ProjectileSpeed`), 이동 방향에 따라 좌우 반전
- 바닥(Ground 레이어)에 닿으면 소멸, 플레이어에 닿으면 데미지+넉백 적용 후 소멸
- `lifetime`(기본 5초) 경과 시 자동 소멸
- 스켈레톤 화살 프리팹: `Assets/_Project/Prefabs/Arrow.prefab` (`SkeletonData.ProjectilePrefab`으로 연결, 속도 5)

- 이 투사체 시스템은 스켈레톤 전용이 아니라 **범용 구조**라, 7.2절에서 제안했던 보스(밤 나무)의 "밤송이 투척" 패턴에도 그대로 재사용 가능합니다 (밤송이를 `MonsterProjectile` 프리팹으로 만들고 보스 공격 로직에서 생성하면 됨). 다만 "밤송이를 정령 공격으로 없앨 수 있다"는 파훼법을 구현하려면, 투사체에 `MonsterHealth`+`MonsterClickMarker`류의 피격/클릭 판정을 추가로 붙여야 합니다 — 지금 `MonsterProjectile.cs`에는 아직 없습니다.
- 씬에는 `Skeleton` 오브젝트(`Skeleton.prefab` 인스턴스, `SkeletonData` 연결)로 배치되어 있습니다.

### 5.4 사망 시 이동 스크립트 자동 비활성화 — 구조 개선 (2026-08-06)

`MonsterHealth.Die()`가 더 이상 `GroundMonsterMovement`/`FloatingMonsterMovement`를 하나하나 이름으로 지정해 비활성화하지 않습니다. 대신 **`IMonsterMovement`라는 빈 마커 인터페이스**를 만들어 모든 몬스터 이동 스크립트(`GroundMonsterMovement`, `FloatingMonsterMovement`, `DebuffMonsterMovement`, `RangedMonsterMovement`)가 이를 구현하도록 하고, `Die()`에서는 `GetComponents<IMonsterMovement>()`로 찾은 것을 전부 순회하며 비활성화합니다.

- 지난번 QA에서 발견했던 "공중 몬스터가 죽어도 계속 움직이는" 버그(하드코딩 누락이 원인)가 이 구조로 바뀌면서 **앞으로 새로 추가되는 몬스터 이동 스크립트도 `IMonsterMovement`만 구현하면 자동으로 비활성화 대상에 포함**됩니다.

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
| Animator Controller (7종 전체) | `Assets/_Project/Animators/Monsters/AC_Monster_이름.controller` — Slime/Bee/Mushroom/TreeGolem/Skeleton/CarnivorousPlant/Boss_Ent 7개 모두 생성 완료 (State: Idle/Walk/Attack, Parameter: `Speed`(Float)/`Attack`(Trigger) 공통 구조). **2026-07-23 기준 7종 전체 Idle/Walk/Attack 클립이 모두 연결 완료**되었습니다. 에셋 팩에 클립이 있던 5종(Slime/Bee/Mushroom/Skeleton/TreeGolem)은 팩 내 원본 클립을 그대로 사용했고, 팩에 클립이 없던 2종(CarnivorousPlant/Boss_Ent)은 `Assets/_Project/Animations/Monsters/CarnivorousPlant/`, `.../Boss_Ent/`에 `anim_monster_이름_동작.anim` 형식으로 직접 제작해 연결했습니다. |
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

### 6.3 스탯 데이터 (ScriptableObject 기반, 2026-07-28 갱신)

- 몬스터 스탯은 `MonsterData`(공통 기반: Health/MoveSpeed/Damage/CoolTime/AttackRangeX/AttackRangeY/DetectionRange/Knockback/AttackAnimDelay) + `MeleeMonsterData`/`RangedMonsterData`/`DebuffMonsterData`/`GroundMonsterData`/`FloatingMonsterData` (`Assets/_Project/Scripts/ScriptableObjects/EnemyData/`) `ScriptableObject`로 구현되어 있으며, 필드가 `스탯_기획서.md` 3장 항목과 대응됩니다.
- `GroundMonsterMovement.cs`/`FloatingMonsterMovement.cs`가 이제 `monsterData` 필드(각각 `GroundMonsterData`/`FloatingMonsterData`)를 통해 이 수치들을 가져오도록 리팩터링되었습니다 (이전에는 스크립트에 값이 직접 박혀있었음).
- 자세한 필드는 `스크립트_설명서.md` 참고.
- 단, `RangedMonsterData`의 `ProjectileTypes`와 `DebuffMonsterData`의 `DebuffTypes` enum이 아직 **값이 비어있는 상태**라, 실제 종류(화살, 이동속도감소 등)를 채워 넣는 작업이 남아있습니다.

---

### 6.4 근접 지상 몬스터 프리팹 (2026-08-04 추가)

지상 근접몹을 씬에 직접 배치하지 않고 **재사용 가능한 프리팹**으로 만드는 작업이 완료됐습니다.

| 항목 | 경로 |
|---|---|
| 공통 템플릿 | `Assets/_Project/Prefabs/Entities/Monsters/GroundMonsterPrefab.prefab` — `BoxCollider2D`+`Rigidbody2D`+`GroundMonsterMovement`+`SpriteRenderer`+`MonsterHealth` (루트) / `MonsterVisual`(SpriteRenderer+Animator) / `AttackMarker`(BoxCollider2D+MonsterClickMarker) 3-계층 구조 |
| 버섯 | `Assets/_Project/Prefabs/Entities/Monsters/Mushroom.prefab` — `MushroomData_temp_real` 데이터 연결 |
| 나무 골렘 | `Assets/_Project/Prefabs/Entities/Monsters/TreeGolem.prefab` — `TreeGolemData` 데이터 연결 |
| 식충식물 | `Assets/_Project/Prefabs/Entities/Monsters/CarnivorousPlantData.prefab` — `CarnivorousPlantData` 데이터 연결 (⚠️ 파일명이 `CarnivorousPlant.prefab`이 아니라 `CarnivorousPlantData.prefab`으로 되어있어 데이터 에셋과 이름이 헷갈릴 수 있음) |
| 벌 (참고, 공중형) | `Assets/_Project/Prefabs/Entities/Monsters/Bee.prefab` — `FloatingMonsterMovement` 사용, `MonsterVisual` 계층 없이 애니메이터가 자식에 바로 있음 |
| 슬라임 (디버프형) | `Assets/_Project/Prefabs/Entities/Monsters/Slime.prefab` — `DebuffMonsterMovement` 사용, `SlimeData` 연결 (2026-08-06 추가) |
| 스켈레톤 (원거리형) | `Assets/_Project/Prefabs/Entities/Monsters/Skeleton.prefab` — `RangedMonsterMovement` 사용, `SkeletonData` 연결, 자식 `FirePoint`에서 화살 발사 (2026-08-06 추가) |

- **폴더 위치**: `Assets/_Project/Prefabs/Entities/Monsters/` (2026-08-04 확정 — 계획했던 `Prefabs/Monsters/`에서 `Entities` 폴더가 추가된 구조로 정리, 플레이어 프리팹도 `Prefabs/Entities/Player/`에 함께 위치. 오타였던 `Entitys`는 `Entities`로 수정 완료)
- **몬스터 데이터 에셋도 전부 만들어짐**: `Assets/_Project/ScripableObjects/Monster/`(폴더명도 계획했던 `Data/Monsters/`와 다름)에 7종 전체(`SlimeData`, `TreeGolemData`, `CarnivorousPlantData`, `SkeletonData`, `Boss_EntData`, `BeeData_temp_real`, `MushroomData_temp_real`)가 생성되어 있습니다.
- ⚠️ **`스탯_기획서.md` 3.6절 수치와 실제 에셋 값이 다릅니다.** 스켈레톤·슬라임은 문서 최신 수치와 일치하지만, **나무 골렘·식충식물·밤 나무는 문서에서 값을 갱신하기 이전(더 오래된) 수치로 만들어져 있습니다.** 예: 나무 골렘 Health — 문서 10 / 실제 에셋 20. 실제 에셋 값을 문서의 최신 확정값으로 다시 맞춰야 합니다 (`ToDoList.md` 참고).

## 7. 보스(밤 나무) 패턴 설계 (2026-08-04 추가)

밤 나무는 근접(펀치) / 화면 전체(돌진) / 원거리(밤송이 투척) 세 가지 패턴으로, 플레이어가 어느 거리에 있든 위협이 되도록 설계합니다. 각 패턴의 파훼법은 정령의 서로 다른 기능(플랫폼 설치·거리 조절·공격)을 쓰도록 구성해, 지금까지 만든 정령 시스템을 보스전에서 전부 활용하게 만드는 것이 목표입니다.

| 패턴 | 내용 | 파훼법 | 구현 난이도 |
|---|---|---|---|
| 1. 돌진 | 한쪽 방향으로 카메라 끝까지 직선 돌진, 부딪히면 데미지 | 플랫폼을 설치해 보스를 뛰어넘는다 | 중간 — 새 애니메이션 불필요(Move 클립 재사용), "화면 끝까지 직선 이동" 상태만 추가하면 됨 |
| 2. 펀치 | 보스와 플레이어 거리가 가까워지면 그 자리로 펀치, 데미지 | 멀리서 견제(거리를 벌려서 회피) | 낮음 — 기존 근접형 몬스터 패턴(거리 판정+Attack 애니메이션)을 그대로 재사용 |
| 3. 밤송이 투척 | 플레이어에게 날아오는 밤송이를 투척, 닿으면 데미지 | 밤송이가 플레이어에게 닿기 전에 정령 공격으로 제거 | 중간 — 2026-08-06 기준 투사체 시스템(`MonsterProjectile.cs`) 자체는 스켈레톤 화살로 구현 완료되어 재사용 가능. 다만 밤송이는 "정령 공격으로 제거" 가능해야 하므로, 현재 투사체에는 없는 체력/피격 판정(`MonsterHealth`류)과 클릭 마커(`MonsterClickMarker`류)를 추가로 붙이는 작업만 남음 |

### 7.1 패턴별 확인 필요 사항

- **돌진**: Idle/Move/Attack 3개 클립만으로는 돌진 직전 "경고" 연출이 약할 수 있음 — 플레이어가 반응할 시간을 주려면 돌진 직전에 짧은 예비 동작(정지 프레임 등)이 필요한지 검토
- **돌진 파훼법 성립 조건**: 플랫폼 설치 마커가 스테이지에 촘촘하게 배치되어 있어 어느 타이밍에 돌진이 오든 근처에 쓸 수 있는 마커가 있을 가능성이 높음 (보스 스테이지 마커 배치 스케치로 확인됨) — 별도 타이밍 설계 부담은 적을 것으로 예상
- **밤송이 투척 파훼법 성립 조건**: 밤송이 이동 속도와, 정령이 밤송이에게 도달해 공격이 실제로 맞기까지 걸리는 시간 사이에 여유가 있어야 파훼가 실제로 가능함 — 수치 튜닝 시 확인 필요

### 7.2 공용화 가능한 부분

- ✅ 투사체 시스템(`MonsterProjectile.cs`)이 스켈레톤 화살로 구현되어 공용 기능으로 이미 존재합니다 (5.3절 참고). 밤송이도 이 프리팹 구조를 그대로 복제해서 만들면 됩니다.
- 돌진/펀치는 새 애니메이션 없이 기존 Idle/Walk/Attack 3개 클립과 수치 조합(속도, 사거리, 쿨타임)만으로 구현 가능

### 7.3 패턴별 구체적인 수치 (2026-08-06 추가)

| 패턴 | 발동 조건(쿨타임) | 수치 |
|---|---|---|
| 1. 돌진 | 10초에 한 번 | 돌진 중 이동 속도 = 평상시 `MoveSpeed`의 **2배** |
| 2. 펀치 | 5초에 한 번, **공격 사거리(`AttackRangeX`) 안에 플레이어가 있을 때만** | 사거리 **4** |
| 3. 밤송이 투척 | 15초에 한 번 | 밤송이 **3마리** 동시 소환 |

**밤송이 개별 스탯**

| 필드 | 값 |
|---|---|
| 체력 | 2 |
| 이동 속도 | 1 |
| 사거리(호밍 시작 거리) | 1.5 |

- 소환 직후에는 일반 이동으로 접근하다가, **플레이어와의 거리가 1.5 이내로 들어오면 그때부터 포물선 궤도로 플레이어를 향해 날아듭니다.**

### 7.4 패턴 선택 로직 (2026-08-06 추가)

- 세 패턴은 각자 독립된 쿨타임(10초/5초/15초)을 가지며, **어느 한 패턴이라도 실행 중이면 다른 패턴은 발동하지 않습니다** (동시 발동 없음, 상호 배제).
- 쿨타임이 다 된 패턴이 **여러 개 동시에 준비되면, 그중 하나를 랜덤으로 선택**해 발동합니다.
- 펀치는 쿨타임이 다 됐어도 플레이어가 사거리(4) 밖에 있으면 발동 후보에서 제외됩니다 (쿨타임만 소모되지 않고 대기).

## 8. 다음 단계

- 각 몬스터의 세부 수치(체력, 이동 속도, 공격력, 쿨타임 등)는 `스탯_기획서.md` 3장을 채워 확정
- 몬스터별 공격 패턴·모션은 아트·기획 협업으로 추후 구체화
- 보스(밤 나무) 패턴 설계는 별도 문서로 분리 예정
- 공중형(벌)의 이동 로직은 5장과 별도로 추후 구체화
- `MonsterHealth.cs`의 AI 스크립트 비활성화 부분을 실제 몬스터별 AI 스크립트로 연결
- 나머지 6개 Animator Controller(Bee/Mushroom/TreeGolem/Skeleton/CarnivorousPlant/Boss_Ent)에 실제 클립 채워넣기
- 6.2절의 Speed/Attack/Kinematic 패턴을 나무 골렘·식충식물 이동 스크립트에도 동일 적용
- `MonsterFacing.cs`를 실제 몬스터 오브젝트(예: MushroomMarker)에 부착
