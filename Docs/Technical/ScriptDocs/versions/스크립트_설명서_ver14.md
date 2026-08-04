# BasePlatformer 스크립트 설명서

동아리원 배포용 | 2026

---

## 문서 개요

이 문서는 BasePlatformer 프로젝트에서 구현한 스크립트를 동아리원들이 읽고 이해할 수 있도록 정리한 설명서입니다. (MVP 10개 + 확장 단계 진행 중 스크립트)
각 스크립트마다 역할, 핵심 동작 원리, Inspector 조정 가능 항목, 의존 관계를 설명합니다.

---

## 스크립트 목록

| 분류 | 파일명 | 역할 요약 |
|---|---|---|
| Player | `PlayerMovement.cs` | 플레이어 좌우 이동 + 이동 경계 처리 |
| Player | `PlayerJump.cs` | 기본 점프 + 가변 점프 + 코요테 타임 |
| Player | `PlayerGroundDetector.cs` | 발판 위에 서있는지 판정 (접지 감지) |
| Player | `PlayerAnimatorController.cs` | 애니메이션 상태 전환 + 좌우 반전 처리 |
| Terrain | `TerrainType.cs` | 지형 타입 구분 열거형 (enum) |
| Terrain | `Ground.cs` | 일반 발판 마커 컴포넌트 |
| Terrain | `HazardTrigger.cs` | 즉사 장애물(가시) 트리거 — 플레이어 접촉 시 리스폰 |
| Respawn | `RespawnManager.cs` | 리스폰(재시작) 실행 관리자 |
| Respawn | `FallRespawnDetector.cs` | 낭떠러지 추락 감지 — Y 좌표 기준 리스폰 |
| Goal | `GoalTrigger.cs` | 목표 지점(트로피) 도달 감지 — 스테이지 클리어 처리 |

---

## 스크립트 상세 설명

---

### PlayerMovement.cs

```
경로: Assets/_Project/Scripts/Player/PlayerMovement.cs
부착: PlayerStartMarker (루트 오브젝트)
```

#### 역할

- A/D 키 또는 좌우 화살표를 누르면 캐릭터가 좌우로 이동합니다.
- 속도가 즉시 최대치로 뛰는 것이 아니라 가속/감속 곡선이 있어 자연스럽게 움직입니다.
- 방향을 바꾸면 먼저 감속한 뒤 반대 방향으로 재가속합니다.
- 스테이지 왼쪽 끝(x=0)과 오른쪽 끝(x=60) 밖으로는 이동하지 못합니다.

#### 동작 원리

- 매 `Update`에서 입력값(`MoveInput`)을 읽어두고, `FixedUpdate`에서 `Rigidbody2D` 속도에 반영합니다.
- Unity 기본 물리의 X축 속도만 덮어쓰며, Y축(중력/점프)은 건드리지 않습니다.
- `Mathf.MoveTowards`로 가속/감속을 표현합니다.
- 경계 초과 시 `rb.position`을 직접 클램프하고 속도를 0으로 초기화합니다.

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `maxMoveSpeed` | float | 6.0 | 최고 이동 속도 (유닛/초) |
| `accelerationTime` | float | 0.15 | 0 → 최대 속도까지 걸리는 시간(초) |
| `decelerationTime` | float | 0.10 | 최대 속도 → 0까지 걸리는 시간(초) |
| `leftBoundX` | float | 0 | 왼쪽 이동 한계 X 좌표 |
| `rightBoundX` | float | 60 | 오른쪽 이동 한계 X 좌표 |

> **참고:** 이 스크립트는 Y축 속도를 건드리지 않습니다. 점프/중력은 `PlayerJump`가 담당합니다.

---

### PlayerJump.cs

```
경로: Assets/_Project/Scripts/Player/PlayerJump.cs
부착: PlayerStartMarker (루트 오브젝트)
```

#### 역할

- Space / W / 위쪽 화살표를 누르면 캐릭터가 점프합니다.
- 버튼을 오래 누를수록 높이 뜨고(최대 3타일), 짧게 탭하면 낮게 뜹니다(최소 1.2타일). — **가변 점프**
- 발판 끝에서 걸어 나간 직후 0.12초 안에 점프하면 점프가 됩니다. — **코요테 타임**
- Unity의 기본 중력 대신 스크립트에서 직접 중력을 계산합니다 (스펙 수치를 정확히 맞추기 위해).

#### 동작 원리

- `WasPressedThisFrame` / `WasReleasedThisFrame`은 `Update`에서 버퍼링한 후 `FixedUpdate`에서 소비합니다.
  - `FixedUpdate`에서 직접 읽으면 프레임 타이밍 차이로 입력 누락 버그가 발생하기 때문입니다.
- 점프 초기 속도와 중력은 목표 높이·시간으로부터 역산합니다: `v0 = 2h/t`, `g = 2h/t²`
- **가변 점프:** 버튼을 뗄 때 현재 속도를 50%로 즉시 감쇠. 단, 최소 높이(1.2타일) 미만이면 최소 높이까지 올라갈 수 있는 속도를 보장합니다.
- **코요테 타임:** 땅 위에 있는 동안 타이머를 항상 0.12초로 채워두고, 공중에서 줄어들게 합니다. 점프 순간 타이머를 0으로 소진하여 이중 점프를 방지합니다.

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `maxJumpHeight` | float | 3.0 | 최대 점프 높이 (타일) |
| `timeToApex` | float | 0.35 | 최고점 도달 시간(초) |
| `minJumpHeight` | float | 1.2 | 탭 시 최소 보장 점프 높이(타일) |
| `jumpCutMultiplier` | float | 0.5 | 버튼 뗄 때 속도 감쇠 비율 |
| `coyoteTime` | float | 0.12 | 코요테 타임 허용 시간(초) |

#### 의존 컴포넌트

`PlayerGroundDetector` (같은 오브젝트에 함께 있어야 합니다)

> **참고:** `Rigidbody2D.gravityScale`은 0으로 설정되어 있으며, 이 스크립트가 중력 역할을 직접 수행합니다.

---

### PlayerGroundDetector.cs

```
경로: Assets/_Project/Scripts/Player/PlayerGroundDetector.cs
부착: PlayerStartMarker (루트 오브젝트)
```

#### 역할

- 플레이어가 발판 위에 서있는지를 판정하는 전용 컴포넌트입니다.
- `OnCollision` 이벤트(물리 충돌 이벤트)를 사용해 접촉점의 법선 방향을 검사합니다.
- 법선이 거의 수직 위(0.9 이상)인 경우만 '바닥'으로 인정합니다. 벽 옆면에 닿아도 접지로 오인하지 않습니다.
- `PlayerJump.IsGrounded` 속성이 이 컴포넌트의 결과를 그대로 사용합니다.

#### 동작 원리

- 동시에 여러 타일 콜라이더에 닿을 수 있으므로, 접지 중인 콜라이더를 `HashSet`으로 관리합니다.
- `OnCollisionExit2D` 시 해당 콜라이더를 Set에서 제거하고, Set이 비어있으면 공중 상태로 판정합니다.

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `minUpwardNormalY` | float | 0.9 | 바닥으로 인정하는 법선 최솟값 (0~1). 0.9 ≈ 약 26° 이내 |

> **참고:** 가시(Hazard)는 트리거 콜라이더라 `OnCollision` 이벤트가 발생하지 않으므로 이 컴포넌트가 자동으로 무시합니다.

---

### PlayerAnimatorController.cs

```
경로: Assets/_Project/Scripts/Player/PlayerAnimatorController.cs
부착: PlayerStartMarker/Visual (자식 오브젝트)
```

#### 역할

- 이동 속도·접지 여부·수직 속도를 읽어서 Animator의 파라미터에 매 프레임 넘겨줍니다.
- Idle(대기) / Run(이동) / Jump(점프 상승) / Fall(낙하) 4가지 상태를 자동으로 전환합니다.
- 왼쪽으로 이동 시 `SpriteRenderer.flipX`로 스프라이트를 좌우 반전합니다.

#### 동작 원리

- 물리 컴포넌트들은 부모(루트) 오브젝트에 있고, 이 스크립트는 자식(`Visual`)에 있으므로 `GetComponentInParent`로 참조합니다.
- `Speed` 파라미터는 실제 이동 속도(`CurrentSpeed`) 대신 입력값(`MoveInput`)으로 구동합니다.
  - 경계에서 속도가 0이 되어도 입력이 있으면 달리는 애니메이션이 유지됩니다.
- 정지 시에는 마지막으로 바라보던 방향을 유지합니다 (입력이 있을 때만 `flipX`를 갱신).

#### 의존 컴포넌트

`PlayerMovement`, `PlayerJump`, `Rigidbody2D` (부모에), `Animator`, `SpriteRenderer` (자신에)

> **참고:** Animator Controller 파라미터: `Speed`(float), `Grounded`(bool), `VerticalVelocity`(float)

---

### TerrainType.cs

```
경로: Assets/_Project/Scripts/Terrain/TerrainType.cs
부착: 없음 (코드 정의만, 오브젝트에 붙지 않음)
```

#### 역할

- 발판 종류를 코드에서 구분하기 위한 `enum`(열거형)입니다.
- `Normal`(일반 발판)과 `Hazard`(즉사 장애물) 두 종류가 있습니다.

#### 동작 원리

- `Ground.cs`, `HazardTrigger.cs` 등에서 지형 타입을 참조할 때 사용합니다.
- 나중에 이동 발판이나 스프링 등을 추가할 때 이 enum에 값만 추가하면 됩니다.

---

### Ground.cs

```
경로: Assets/_Project/Scripts/Terrain/Ground.cs
부착: Ground (Tilemap 오브젝트)
```

#### 역할

- Ground Tilemap 오브젝트가 '일반 발판'임을 코드에서 식별할 수 있도록 붙여두는 마커입니다.
- 현재는 `TerrainType.Normal`로 고정되어 있고, 나중에 이동 발판 등을 추가할 때 활용합니다.

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `type` | TerrainType | Normal | 이 지형의 타입 (현재는 Normal 고정) |

---

### HazardTrigger.cs

```
경로: Assets/_Project/Scripts/Terrain/HazardTrigger.cs
부착: Hazard_Trigger_25 ~ Hazard_Trigger_46 (가시 트리거 오브젝트 6개)
```

#### 역할

- 플레이어가 가시에 닿으면 `RespawnManager`를 호출해 게임을 처음부터 다시 시작합니다.
- 닿자마자(Enter) 또는 닿은 채로 있을 때(Stay) 모두 감지합니다.
- 리스폰 직후 같은 트리거가 연속으로 반응하지 않도록 쿨다운이 있습니다.

#### 동작 원리

- `OnTriggerEnter2D` / `OnTriggerStay2D` 이벤트를 사용합니다.
- 트리거 이벤트가 발생하려면 오브젝트에 `Rigidbody2D(Kinematic)`가 필요합니다. (Unity 물리 규칙)
- 플레이어 여부는 `PlayerMovement` 컴포넌트 존재 여부로 확인합니다.
- `Awake`에서 씬 전체에서 `RespawnManager`를 자동으로 찾습니다.

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `respawnCooldown` | float | 0.5 | 리스폰 후 중복 반응 방지 쿨다운(초) |

#### 의존 컴포넌트

`RespawnManager` (씬 어딘가에 존재해야 합니다)

> **참고:** 레이어 매트릭스: Hazard 레이어는 Player 레이어하고만 충돌하도록 설정되어 있습니다.

---

### RespawnManager.cs

```
경로: Assets/_Project/Scripts/Respawn/RespawnManager.cs
부착: RespawnManager (씬의 빈 오브젝트)
```

#### 역할

- `HazardTrigger`나 `FallRespawnDetector`가 리스폰이 필요하다고 판단했을 때 이 스크립트의 `RespawnPlayer()`를 호출합니다.
- 현재 MVP에서는 씬 전체를 다시 로드해서 게임을 처음 상태로 되돌립니다.

#### 동작 원리

- `SceneManager.LoadScene`으로 현재 씬을 리로드합니다.
- 나중에 체크포인트를 추가할 때는 이 메서드 내부만 바꾸면 됩니다. (`HazardTrigger`, `FallRespawnDetector`는 수정 불필요)

---

### FallRespawnDetector.cs

```
경로: Assets/_Project/Scripts/Respawn/FallRespawnDetector.cs
부착: PlayerStartMarker (루트 오브젝트)
```

#### 역할

- 플레이어가 발판 사이 구멍으로 떨어져 화면 아래로 사라지면 리스폰을 실행합니다.
- 카메라 화면 하단보다 조금 더 아래의 Y 좌표를 기준선으로 사용합니다.

#### 동작 원리

- 매 `Update`마다 자신의 Y 좌표를 `killPlaneY`와 비교합니다.
- 중복 호출 방지를 위한 쿨다운이 있습니다 (`HazardTrigger`와 동일한 패턴).

#### Inspector 항목

| 필드명 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `killPlaneY` | float | -3.5 | 이 Y 좌표 이하로 내려가면 리스폰 (카메라 하단 -2.91보다 아래) |
| `respawnCooldown` | float | 0.5 | 중복 호출 방지 쿨다운(초) |

#### 의존 컴포넌트

`RespawnManager` (씬 어딘가에 존재해야 합니다)

---

### GoalTrigger.cs

```
경로: Assets/_Project/Scripts/Goal/GoalTrigger.cs
부착: GoalPoint
```

#### 역할

- 플레이어가 스테이지 끝의 트로피에 닿으면 클리어 판정을 내립니다.
- 현재 MVP에서는 콘솔에 `"Clear"`를 출력합니다.
- 한 번 클리어된 후엔 중복 감지를 하지 않습니다.

#### 동작 원리

- `OnTriggerEnter2D`로 플레이어 접촉을 감지합니다.
- `cleared` 플래그로 중복 호출을 방지합니다.
- `OnClear()` 메서드 내부만 교체하면 클리어 UI, 다음 씬 전환 등으로 확장할 수 있습니다.

> **참고:** 나중에 클리어 화면이나 다음 레벨 전환을 추가할 때는 `OnClear()` 메서드 안만 수정하면 됩니다.

---

## 스크립트 관계도

### 플레이어 제어 흐름

```
PlayerMovement        →  Rigidbody2D.velocity.x      (X축 이동)
PlayerJump            →  Rigidbody2D.velocity.y      (Y축 이동 + 중력)
PlayerGroundDetector  →  PlayerJump.IsGrounded        (접지 정보 제공)
PlayerAnimatorController → Animator 파라미터         (PlayerMovement, PlayerJump, Rigidbody2D 읽기)
```

### 지형 / 위험 요소 흐름

```
Ground            →  TerrainType.Normal 마커         (Tilemap 오브젝트에 부착)
HazardTrigger     →  RespawnManager.RespawnPlayer()  (가시 오브젝트에 부착)
FallRespawnDetector → RespawnManager.RespawnPlayer() (플레이어에 부착)
RespawnManager    →  SceneManager.LoadScene()        (씬 전체 리셋)
```

### 클리어 흐름

```
GoalTrigger  →  Debug.Log("Clear")  (트로피에 부착)
```

---


## 확장 시스템 스크립트

### 구현 완료 — Monsters (2026-07-16 추가)

| 파일명 | 경로 | 역할 |
|---|---|---|
| `MonsterHealth.cs` | `Assets/_Project/Scripts/Monster/MonsterHealth.cs` | 몬스터 체력/피격/사망 처리. `TakeDamage(int)` 호출 시 `SpriteRenderer.color`를 빨간색으로 잠깐 바꿨다가 복귀(피격 연출), 체력 0 이하가 되면 `GroundMonsterMovement`/Animator 비활성화 후 알파값 페이드 아웃하며 제거(사망 연출). 시각 루트·Animator를 자식 오브젝트에서 자동으로 찾도록 구현됨(`GetComponentInChildren`). **2026-07-30 확인: `MushroomMarker`/`BeeMarker`에 실제로 부착되어 동작 중**입니다 (이전엔 미부착 상태였음). `MonsterData`(공통 필드)에서 체력/피격 연출 시간을 가져옵니다. |
| `MonsterFacing.cs` | `Assets/_Project/Scripts/Monster/MonsterFacing.cs` | 좌우 반전 처리. 모든 몬스터 클립이 오른쪽만 바라보므로, `Rigidbody2D`의 x축 속도를 읽어 `SpriteRenderer.flipX`로 반전(`PlayerAnimatorController.cs`와 동일 방식). **여전히 실제 몬스터 오브젝트에는 부착되어 있지 않은 상태**입니다 (`GroundMonsterMovement.cs`/`FloatingMonsterMovement.cs`가 `localScale.x` 반전으로 자체 처리 중이라 중복 가능성 있음 — 실제 필요 여부 확인 필요). |
| `GroundMonsterMovement.cs` | `Assets/_Project/Scripts/Enemy/GroundMonsterMovement.cs` | 지상 근접몹(버섯 등) 이동/공격 스크립트. **Patrol/Chase/Attack 상태 머신**. 2026-07-28 갱신: (1) 수치를 `GroundMonsterData` ScriptableObject에서 가져오도록 리팩터링 (2) "같은 플랫폼" 판정을 발밑 레이캐스트로 얻은 바닥 Y좌표 비교 방식으로 변경 (3) 공격이 트리거 즉시 데미지를 주지 않고 `AttackAnimDelay`초 후 판정하는 예비동작(텔레그래프) 방식으로 변경 (`몬스터_기획서.md` 5장·6.2절 참고). |
| `FloatingMonsterMovement.cs` | `Assets/_Project/Scripts/Enemy/FloatingMonsterMovement.cs` | 공중형(벌) 이동/공격 스크립트. 지상 몹과 동일한 Patrol/Chase/Attack 상태 머신이지만, 플랫폼 개념 없이 `SmoothDamp`로 자유롭게 날아다니며 직선 레이캐스트(Line of Sight)로만 장애물을 확인합니다. 2026-07-28 갱신: `FloatingMonsterData` ScriptableObject에서 수치를 가져오도록 리팩터링, 공격도 지상 몹과 동일하게 `AttackAnimDelay` 예비동작 방식 적용. 씬의 `BeeMarker`/`Bee_Idle_0`에 Animator와 `AC_Monster_Bee.controller` 연결까지 완료된 상태입니다 (`몬스터_기획서.md` 5.1절 참고). |

### 구현 완료 — Player/UI (2026-07-16 확인 및 추가)

| 파일명 | 경로 | 역할 |
|---|---|---|
| `PlayerHealth.cs` | `Assets/_Project/Scripts/Player/PlayerHealth.cs` | 하트 기반 체력 관리. `TakeDamage(int, Vector2)` 호출 시 하트 감소 + 넉백 + 무적 코루틴 시작, 하트가 0이 되면 `RespawnManager.RespawnPlayer()` 호출. `OnHealthChanged` 이벤트로 UI에 변경 사항을 알립니다. `PlayerData`(ScriptableObject)에서 최대 체력을 가져옵니다. |
| `HeartUI.cs` | `Assets/_Project/Scripts/UI/HeartUI.cs` | 하트 UI 표시. `PlayerHealth.OnHealthChanged`를 구독해 하트 개수만큼 아이콘을 생성하고, 현재 체력에 따라 `fullHeartSprite`/`emptyHeartSprite`로 스프라이트를 교체합니다. |

> **참고 (2026-07-16 정리 완료)**: `PlayerHealth.cs`의 무적 시간/깜빡임 주기는 코드 필드 초기값(10초/0.1초)이 아니라, 씬에 저장된 **Inspector 실측값(0.5초/0.15초)**이 실제로 적용됩니다 — `캐릭터컨트롤_스펙.md`는 이 실측값 기준으로 유지합니다. 하트 UI는 `월드_스테이지_구성.md`를 실제 코드(`HeartUI.cs`, 단일 스프라이트 교체 방식) 기준으로 수정했습니다. 단, `fullHeartSprite`에 넣을 빨강+외곽선이 합쳐진 완성 이미지는 아직 제작 전입니다 (`ToDoList.md` 참고).

### 구현 완료 — Fairies (2026-07-16 확인 및 추가)

| 파일명 | 경로 | 역할 |
|---|---|---|
| `FairyMovement.cs` | `Assets/_Project/Scripts/Fairy/FairyMovement.cs` | 정령이 플레이어를 따라다니는 이동 로직. `SmoothDamp`로 플레이어 위치 + 오프셋을 부드럽게 추적하고, `Sin` 곡선으로 위아래로 둥둥 떠다니는(Hover) 효과를 더합니다. 플레이어가 반대쪽에 있으면 `localScale.x` 반전으로 좌우를 바라봅니다. |
| `FairyAttackController.cs` (신규, 2026-07-30) | `Assets/_Project/Scripts/Fairy/FairyAttackController.cs` | 정령의 몬스터 공격 로직. `RequestAttack(MonsterHealth)` 호출 시: `FairyMovement.LockFollow()`로 추적 정지 → 대상까지 직접 돌진(dashSpeed) → `MonsterHealth.TakeDamage()` 호출 → 원위치로 복귀(returnSpeed) → `UnlockFollow()`. `MainFairyData`에서 Damage/CoolTime을 가져오며, `CanAttack` 프로퍼티로 쿨타임 여부를 외부에 노출합니다. |
| `MonsterClickMarker.cs` (신규, 2026-07-30) | `Assets/_Project/Scripts/Enemy/MonsterClickMarker.cs` | 몬스터의 자식 오브젝트(`AttackMarker`)에 부착. 새 Input System(`Mouse.current`)으로 매 프레임 마우스 위치를 `Collider2D.OverlapPoint`로 확인해 마커 스프라이트를 표시하고, 마커가 떠 있는 상태에서 좌클릭하면 `FairyAttackController.RequestAttack()`을 호출합니다. `LateUpdate`에서 부모의 좌우 반전에 영향받지 않도록 스케일을 보정합니다. |
| `FairyPlatformController.cs` (신규, 2026-08-04) | `Assets/_Project/Scripts/Fairy/FairyPlatformController.cs` | 정령의 플랫폼 변신/해제 컨트롤러. `RequestTransform()` 호출 시 마커 위치로 이동(`Interact` 트리거+`Speed=1`) → 도착 시 대상 플랫폼 활성화 + `Animator.Play("Interact",0,0f)`로 즉시 강제 재생 → 재생 끝나면 정령의 `SpriteRenderer`/`Collider2D` 비활성화. `RevertTransform()`은 좌클릭 또는 플랫폼이 카메라 화면 밖으로 나가면 호출되어 정령을 원위치에서 재활성화. `CanTransform`/`HasActivePlatform` 프로퍼티로 상태를 외부에 노출합니다. |
| `PlatformClickMarker.cs` (신규, 2026-08-04) | `Assets/_Project/Scripts/Terrain/PlatformClickMarker.cs` | 플랫폼 설치 장소에 부착. `MonsterClickMarker.cs`와 같은 패턴(마우스 호버 시 마커 표시, 좌클릭 시 요청)이며, 몬스터 마커와 동시에 겹쳐 있을 때는 몬스터 쪽을 우선하도록 처리되어 있습니다(`IsAnyMonsterMarkerHovered`). 이미 플랫폼이 설치되어 있으면 마커를 띄우지 않습니다. |
| `FairyPlatformEntityPusher.cs` (신규, 2026-08-04) | `Assets/_Project/Scripts/Terrain/FairyPlatformEntityPusher.cs` | 정령이 변신해 활성화되는 플랫폼 자식 오브젝트에 자동으로 붙는 컴포넌트(`FairyPlatformController`가 없으면 자동 추가). 활성화 시점(`OnEnable`)과 그 후 일정 시간(`pushDuration`) 동안, 플랫폼 영역과 겹친 Dynamic Rigidbody(플레이어/몬스터)를 플랫폼 위로 밀어 올립니다. Static/Kinematic 바디는 건드리지 않습니다. |

### 구현 완료 — 스탯 데이터 (ScriptableObject 기반, 2026-07-16 확인 및 추가)

몬스터·정령·플레이어 스탯을 코드에 하드코딩하지 않고 `ScriptableObject` 에셋으로 분리해서 관리하는 구조입니다. 기획자가 `스탯_기획서.md`의 수치를 Inspector에서 직접 조정할 수 있습니다.

| 파일명 | 경로 | 필드 | 대응하는 기획 문서 절 |
|---|---|---|---|
| `PlayerData.cs` | `Assets/_Project/Scripts/ScriptableObjects/PlayerData.cs` | JumpPower, MoveSpeed, Health, timeToApex, jumpCutMultiplier, coyoteTime, minUpwardNormalY, killPlaneY, respawnCooldown, invincibilityDuration, blinkInterval | `캐릭터컨트롤_스펙.md` 전체 (2026-07-28 갱신: `PlayerJump`/`PlayerGroundDetector`/`FallRespawnDetector`/`PlayerHealth`에 흩어져 있던 수치가 전부 이 자산 하나로 통합됨) |
| `MonsterData.cs` | `Assets/_Project/Scripts/ScriptableObjects/EnemyData/MonsterData.cs` | Health, MoveSpeed, Damage, CoolTime, AttackRangeX, AttackRangeY, DetectionRange, Knockback, AttackAnimDelay, hurtFlashDuration, deathFadeDuration (2026-07-30: 피격/사망 연출 시간 필드 추가) | `스탯_기획서.md` 3.1 공통 |
| `MeleeMonsterData.cs` | `.../EnemyData/MeleeMonsterData.cs` | (MonsterData 상속, 추가 필드 없음) | `스탯_기획서.md` 3.3 근거리형 |
| `GroundMonsterData.cs` (신규, 2026-07-28) | `.../EnemyData/GroundMonsterData.cs` | (MonsterData 상속) + minHorizontalNormalX | `스탯_기획서.md` 3.1 공통 (지상 몹 전용 판정값) |
| `FloatingMonsterData.cs` (신규, 2026-07-28) | `.../EnemyData/FloatingMonsterData.cs` | (MonsterData 상속) + smoothTime | `스탯_기획서.md` 3.1 공통 (공중 몹 전용 이동값) |
| `RangedMonsterData.cs` | `.../EnemyData/RangedMonsterData.cs` | ProjectileType(`enum ProjectileTypes` 비어있음), FiringType(`enum FiringTypes`: Straight/Arc) — AttackRange는 2026-07-28에 base `MonsterData.AttackRangeX/Y`로 이동 | `스탯_기획서.md` 3.4 원거리형 |
| `DebuffMonsterData.cs` | `.../EnemyData/DebuffMonsterData.cs` | DebuffType(`enum DebuffTypes` 비어있음) — AttackRange는 2026-07-28에 base `MonsterData.AttackRangeX/Y`로 이동 | `스탯_기획서.md` 3.2 디버프형 |
| `MainFairyData.cs` | `.../FairyData/MainFairyData.cs` | Damage, CoolTime, PlatformSize, PlatformType(`enum PlatformTypes` 비어있음) — 2026-07-30 기준 실제 에셋(`MainFairyData.asset`) 값: Damage=5, CoolTime=0.5 | `스탯_기획서.md` 2.1 기본 정령 |
| `AttackFairyData.cs` | `.../FairyData/AttackFairyData.cs` | Damage, CoolTime, AttackRange | `스탯_기획서.md` 2.2 공격 특화 정령 |
| `PlatformFairyData.cs` | `.../FairyData/PlatformFairyData.cs` | CoolTime, PlatformType, StatModifier | `스탯_기획서.md` 2.3 플랫폼 특화 정령 |

> ⚠️ `ProjectileTypes`(`RangedMonsterData.cs`), `DebuffTypes`(`DebuffMonsterData.cs`), `PlatformTypes`(`Assets/_Project/Scripts/Enum/PlatformTypes.cs`) 3개 enum이 **아직 값이 비어있는 빈 껍데기 상태**입니다. `스탯_기획서.md`에 정리된 실제 종류(예: 이속 증가/2단 점프/초당 데미지 발판, 이동속도감소 디버프 등)를 enum 값으로 채워 넣는 작업이 남아있습니다.

### 아직 미구현 — 예정 (2026-07-09 최초 작성)

구현이 시작되면 아래 스크립트들이 이 문서에 추가될 예정입니다 (설계 근거는 `정령_시스템_기획서.md`, `발판_확장_기획서.md`, `기술기획서.md` 4.8절 참고).

| 분류 | 예정 스크립트 | 역할(예정) |
|---|---|---|
| Fairies | `FairyInteractionController.cs` | 마우스 위치 기반 마커 표시, 클릭 상호작용 처리 |
| Fairies | `FairyBase.cs` / `Fairy_*.cs` | 기본 정령 및 추가 정령 6종의 공격/플랫폼 생성 로직 (현재 `FairyMovement.cs`는 이동만 담당, 공격/플랫폼 생성 로직은 미구현) |
| Terrain | `MovingPlatform.cs`, `CollapsingPlatform.cs`, `StatChangePlatform.cs` | 이동/무너짐/스탯 변화 발판 |

---

*문서 끝*
