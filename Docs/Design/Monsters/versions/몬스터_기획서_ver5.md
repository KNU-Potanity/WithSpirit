# 몬스터 기획서

## 0. 문서 목적

> 마지막 수정일: 2026-07-16 (6장 애니메이션 구현 방식·샘플 추가 — ver5)

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

## 5. AI 이동 로직 — 지상 근접몹 (신규)

공중형(벌)을 제외한 근거리형 몬스터(버섯, 나무 골렘, 식충식물)의 이동 방식입니다.

| 상태 | 조건 | 동작 |
|---|---|---|
| 순찰 (기본) | 플레이어가 몬스터가 위치한 플랫폼 범위 안에 들어오지 않음 | 자신이 위치한 플랫폼 내에서 왔다갔다 이동 |
| 추격 | 플레이어가 몬스터가 위치한 플랫폼 범위 안에 들어옴 **AND** 몬스터와 플레이어 사이 x축 경로 상에 가시 등 장애물이 없어 x축으로 만날 수 있음 | 플레이어를 향해 이동 |
| 순찰 유지 | 플레이어가 플랫폼 범위 안에 들어왔지만, 사이에 가시 등 장애물이 있어 x축으로 만날 수 없음 | 추격하지 않고 플랫폼 내 왔다갔다 이동 유지 |

- **판정 기준은 x축(가로) 경로만 봅니다.** 플레이어의 점프로 인한 y값(높이) 변화 때문에 실제로는 못 만나는 경우가 있어도, 이 판정에서는 고려하지 않습니다 — x축상 장애물 유무만으로 추격 여부를 결정합니다.
- "플랫폼 범위"는 몬스터가 순찰하는 발판의 x축 시작~끝 구간을 의미합니다.
- 공중형(벌)은 이 로직과 별개로 처리합니다 (지형에 구애받지 않고 비행 이동).

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
| Animator Controller | `Assets/_Project/Animators/AC_Monster_Sample.controller` (State: Idle/Walk/Attack, Parameter: `Speed`(Float), `Attack`(Trigger)) |
| 데미지/죽음 스크립트 | `Assets/_Project/Scripts/Monster/MonsterHealth.cs` (`TakeDamage(int)` 호출 시 빨간색 깜빡임, 체력 0 이하가 되면 페이드 아웃 후 제거) |

- 다른 몬스터에 적용할 때는 Animator Controller를 복제해 Idle/Walk/Attack 클립만 해당 몬스터 것으로 교체하고, `MonsterHealth.cs`는 그대로 부착하면 됩니다.
- `MonsterHealth.cs`의 `Die()` 안에서 실제 AI 스크립트를 비활성화하는 부분은 각 몬스터의 실제 AI 스크립트명으로 교체가 필요합니다 (현재는 플레이스홀더).

---

## 7. 다음 단계

- 각 몬스터의 세부 수치(체력, 이동 속도, 공격력, 쿨타임 등)는 `스탯_기획서.md` 3장을 채워 확정
- 몬스터별 공격 패턴·모션은 아트·기획 협업으로 추후 구체화
- 보스(밤 나무) 패턴 설계는 별도 문서로 분리 예정
- 공중형(벌)의 이동 로직은 5장과 별도로 추후 구체화
- `MonsterHealth.cs`의 AI 스크립트 비활성화 부분을 실제 몬스터별 AI 스크립트로 연결
- 각 몬스터별 Animator Controller를 샘플(6.1절) 기준으로 복제·제작
