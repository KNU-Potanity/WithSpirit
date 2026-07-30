# 스탯 기획서

## 0. 문서 목적

> 마지막 수정일: 2026-07-28 (3.6절에 몬스터 7종 개별 수치(ScriptableObject 필드 기준) 추가 — ver4)

확장 단계에서 등장하는 플레이어·정령·몬스터의 스탯 **항목 및 수치**를 정리합니다.
2026-07-14 개정: 기존에 "(추후 확정)"으로 비워두었던 정령·몬스터 항목에 실제 수치를 채웠습니다. 아래 수치는 1차 밸런스 기준값이며, 플레이 테스트(`QA_테스트_계획서.md` 9장 E-01~E-15) 결과에 따라 조정될 수 있습니다.

---

## 1. 플레이어

| 스탯 | 설명 |
|---|---|
| 점프력 | `캐릭터컨트롤_스펙.md`의 점프 스펙과 연결 |
| 이동 속도 | `캐릭터컨트롤_스펙.md`의 이동 스펙과 연결 |
| 체력 | 하트 단위로 표시. 낙사를 제외한 데미지는 하트 1씩 감소 (`월드_스테이지_구성.md` 6장 참고) |

---

## 2. 정령

### 2.1 기본 정령

| 스탯 | 수치 |
|---|---|
| 공격력 | 5 (몬스터 처치 시 필요 타격 수의 기준치, 근거리형 몬스터 체력 10 기준 2회) |
| 쿨타임 | 0.8초 |
| 플랫폼 크기 | 2.0 × 0.3 유닛 (플레이어 1회 착지 가능한 폭) |
| 플랫폼 종류 | 버프 없는 일반 발판형 1종 (추가 정령의 플랫폼 특화 효과 없음) |

### 2.2 공격 특화 정령

| 스탯 | 수치 |
|---|---|
| 공격력 | 8 (기본 정령보다 높은 화력) |
| 쿨타임 | 1.2초 |
| 공격 범위 | **폭발**: 타격 지점 반경 1.5유닛 범위 데미지 / **스턴**: 단일 대상, 명중 시 1.5초간 행동불가 부여 / **체인**: 최초 대상 기준 반경 2.0유닛 내 몬스터에게 최대 3체까지 연쇄 전파 |

### 2.3 플랫폼 특화 정령

| 스탯 | 수치 |
|---|---|
| 쿨타임 | 1.0초 (플랫폼 재생성 대기시간) |
| 플랫폼 종류 | **이속 증가 발판** / **2단 점프 발판** / **초당 데미지 발판** — 발판 크기는 기본 정령과 동일(2.0 × 0.3 유닛) |
| 스탯 변화량 | 이속 증가: 발판 탑승 중 이동 속도 +50%(9.0유닛/초), 하차 후 2초간 유지 / 2단 점프: 발판 탑승 중 공중에서 1회 추가 점프 허용(높이는 기본 점프와 동일 3.0타일) / 초당 데미지: 발판 위 몬스터에게 초당 2데미지 |

---

## 3. 몬스터

### 3.1 공통

| 스탯 | 수치 |
|---|---|
| 체력 | 근거리형 10 / 원거리형 8 / 디버프형 6 / 보스 60 |
| 이동 속도 | 근거리형 2.0유닛/초 / 원거리형 1.0유닛/초(제자리 순찰 위주) / 디버프형 1.5유닛/초 / 보스 1.5유닛/초 |
| 공격력 | 하트 1 감소로 고정 (몬스터 종류와 무관하게 접촉·투사체 피격 시 동일 — `월드_스테이지_구성.md` 6장) |
| 쿨타임 | 공격 재사용 대기시간 1.5초 (공통 기준값, 보스는 3.5절 참고) |

### 3.2 디버프형 추가 스탯

| 스탯 | 수치 |
|---|---|
| 디버프 종류 | 이동 속도 50% 감소, 지속 3초 (슬라임 접촉 시 적용) |
| 공격 범위 | 근접 접촉형, 판정 범위 0.5유닛 |

### 3.3 근거리형 추가 스탯

공통 스탯(3.1)만 사용합니다 (추가 스탯 없음).

### 3.4 원거리형 추가 스탯

| 스탯 | 수치 |
|---|---|
| 공격 범위 | 감지 범위 5.0유닛 / 발사 사거리 4.0유닛 |
| 투사체 종류 | 단일 직선 투사체 (스켈레톤: 화살) — 투사체 속도 8.0유닛/초 |
| 발사 방식 | 플레이어 위치를 조준해 직선으로 발사, 발사 후 쿨타임(3.1절 기준 1.5초) 동안 재장전 |

### 3.5 보스 추가 스탯

| 스탯 | 수치 |
|---|---|
| 공격 범위 | 근접 패턴 3.0유닛 / 원거리 패턴 사거리 6.0유닛 (패턴 전환형) |
| 투사체 종류 | 낙엽 폭풍(광역) / 뿌리 공격(근접 돌출형) — 세부 패턴 타이밍과 연출은 별도 보스 설계 문서에서 확정 (`몬스터_기획서.md` 4장 참고) |

### 3.6 몬스터별 개별 수치 (ScriptableObject 필드 기준, 2026-07-28 추가)

3.1~3.5절의 분류별 수치는 설계 방향이고, 실제 구현은 `MonsterData`(공통) + `GroundMonsterData`/`FloatingMonsterData`/`RangedMonsterData`/`DebuffMonsterData`(유형별) 필드 구조를 따릅니다. 이 절은 **몬스터 이름 하나하나에 대한 실제 데이터 에셋 수치**를 정리합니다 (`스크립트_설명서.md`의 ScriptableObject 구조 참고).

| 상태 표시 | 의미 |
|---|---|
| ✅ 구현 완료 | 실제 데이터 에셋(.asset)이 존재하고 Inspector에서 확인한 값 |
| 📝 제안 초안 | 아직 데이터 에셋이 없어, 3.1~3.5절 방향과 벌/버섯의 실측값을 참고해 제안하는 값 (담당자가 에셋 생성 시 조정 가능) |

#### 벌 (Bee) — ✅ 구현 완료 (`FloatingMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 10 |
| Move Speed | 5 |
| Damage | 1 |
| Cool Time | 0.5 |
| Attack Range X | 0.5 |
| Attack Range Y | 0.8 |
| Detection Range | 8 |
| Knockback | 2 |
| Attack Anim Delay | 0.4 |
| Smooth Time | 0.3 |

#### 버섯 (Mushroom) — ✅ 구현 완료 (`GroundMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 10 |
| Move Speed | 2 |
| Damage | 1 |
| Cool Time | 1.5 |
| Attack Range X | 1.2 |
| Attack Range Y | 2 |
| Detection Range | 8 |
| Knockback | 2 |
| Attack Anim Delay | 0.4 |
| Min Horizontal Normal X | 0.9 |

#### 슬라임 (Slime) — 📝 제안 초안 (`DebuffMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 8 |
| Move Speed | 1.5 |
| Damage | 1 |
| Cool Time | 1.5 |
| Attack Range X | 1.0 |
| Attack Range Y | 1.5 |
| Detection Range | 6 |
| Knockback | 1.5 |
| Attack Anim Delay | 0.3 |
| Debuff Type | (이동 속도 50% 감소, 3초 지속 — `DebuffTypes` enum 값 채우기 필요) |

> ⚠️ `DebuffMonsterData`에는 디버프 지속시간/감소율을 담을 필드가 없습니다. `DebuffType` enum 값 채우기와 함께, 지속시간·감소율 필드 추가가 필요합니다 (`ToDoList.md` 참고).

#### 나무 골렘 (TreeGolem) — 📝 제안 초안 (`GroundMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 20 (덩치가 큰 만큼 버섯보다 단단하게) |
| Move Speed | 1.2 |
| Damage | 1 |
| Cool Time | 2.0 |
| Attack Range X | 1.5 |
| Attack Range Y | 2.2 |
| Detection Range | 7 |
| Knockback | 3 |
| Attack Anim Delay | 0.6 (덩치가 큰 만큼 예비동작도 느리게) |
| Min Horizontal Normal X | 0.9 |

#### 식충식물 (CarnivorousPlant) — 📝 제안 초안 (`GroundMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 10 |
| Move Speed | 0 (제자리 고정, 순찰 이동 없음 — `몬스터_기획서.md` 3장 "근접형 — 제자리에서 근접 범위 공격" 참고) |
| Damage | 1 |
| Cool Time | 1.2 |
| Attack Range X | 1.0 |
| Attack Range Y | 1.2 |
| Detection Range | 5 (매복형이라 짧게) |
| Knockback | 2 |
| Attack Anim Delay | 0.35 |
| Min Horizontal Normal X | 0.9 |

#### 스켈레톤 (Skeleton) — 📝 제안 초안 (`RangedMonsterData`)

| 필드 | 값 |
|---|---|
| Health | 8 |
| Move Speed | 1.0 (제자리 순찰 위주) |
| Damage | 1 |
| Cool Time | 1.5 |
| Attack Range X | 4.0 (발사 사거리) |
| Attack Range Y | 2.0 |
| Detection Range | 5.0 |
| Knockback | 2 |
| Attack Anim Delay | 0.5 (활 조준 시간) |
| Projectile Type | 화살 (`ProjectileTypes` enum 값 채우기 필요) |
| Firing Type | Straight |

#### 밤 나무 (Boss_Ent) — 📝 제안 초안 (전용 데이터 클래스 없음, `MonsterData` 기준)

| 필드 | 값 |
|---|---|
| Health | 60 |
| Move Speed | 1.5 |
| Damage | 1 (접촉 기준, 패턴별 데미지는 별도 설계 예정) |
| Cool Time | 3.0 |
| Attack Range X | 3.0 (근접 패턴) |
| Attack Range Y | 3.0 |
| Detection Range | 10 |
| Knockback | 4 |
| Attack Anim Delay | 0.8 (거대한 몸집의 예비동작) |

> ⚠️ 보스는 근접+원거리 복합 패턴(`몬스터_기획서.md` 4장)인데, 지금 구조(`GroundMonsterData`/`RangedMonsterData` 상속 분리)로는 한 몬스터가 두 유형을 동시에 가질 수 없습니다. 전용 `BossMonsterData` 클래스 신설이 필요합니다 (`기술기획서.md` 4.9절 "상속 대신 컴포지션 고려" 참고).

---

## 4. 다음 단계

- 위 수치는 1차 밸런스 기준값이며, 실제 플레이 테스트(`QA_테스트_계획서.md` E-01~E-15) 결과에 따라 ±10~20% 단위로 조정 (`캐릭터컨트롤_스펙.md` 9장과 동일한 조정 원칙 적용)
- 보스(밤 나무)의 세부 공격 패턴·타이밍은 별도 문서로 분리해 설계 예정
- 수치가 조정되면 이 문서를 다음 버전으로 갱신
