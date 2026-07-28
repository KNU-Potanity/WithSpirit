# ToDo List

## 0. 문서 목적

여러 문서에 흩어진 체크리스트와 다음 단계 항목들을 **한 곳에서 한눈에 확인**할 수 있도록 모은 문서입니다.

- 한 작업에 두 역할이 관여하는 경우, 하나의 항목으로 합치지 않고 역할별 항목으로 나눠서 각자의 몫을 적었습니다.
- 지금처럼 혼자 작업할 때는 모든 역할을 본인이 수행하면 되고, 추후 부원이 합류하면 본인 역할 묶음만 확인하면 됩니다.
- 항목을 완료하면 체크박스에 표시(`[x]`)하며 사용합니다.
- 2026-07-09 개정: 정령 시스템 채택에 따라 4장을 구체적인 항목으로 갱신했습니다.

---

## 1. 지금 당장 — 핵심 기능 (혼자, 내일까지)

> 출처: `일정_마일스톤.md`, `플랫포머_게임_기획서.md`

**프로그래밍**
- [x] Unity 프로젝트 생성 및 폴더 구조 세팅
- [x] GitHub 저장소 연결 및 Git 설정 (Force Text, Visible Meta Files, .gitignore)
- [x] 좌우 이동 구현
- [x] 기본 점프 구현 (`PlayerJump.cs` — 역산 공식으로 점프 속도/중력 계산)
- [x] 가변 점프 구현 (`PlayerJump.cs` — 버튼 뗄 때 50% 감쇠, 최소 1.2타일 보장)
- [x] 코요테 타임 구현 (`PlayerJump.cs` — 0.12초, 발판 이탈 후 점프 허용)
- [x] 캐릭터 애니메이션 클립 제작(대기/이동/점프 상승/낙하) 및 Animator Controller 연결 (`기술기획서.md` 4.7절 에셋 경로 참고 — 그림은 이미 제공되어 있어 신규 작업이 아니라 연결 작업)
- [x] 일반 발판 충돌 처리 (`PlayerGroundDetector.cs` — 물리 충돌 이벤트 + 법선 벡터 기반 접지 판정)
- [x] 즉사 장애물 판정 + 리스폰 트리거 연결 (`HazardTrigger.cs` — OnTriggerEnter/Stay2D + 쿨다운)
- [x] 리스폰 시스템 (`RespawnManager.cs` — 씬 리로드, `FallRespawnDetector.cs` — 낙사 감지)
- [x] 목표 지점(도착) 구현 (`GoalTrigger.cs` — 접촉 시 Debug.Log(Clear))
- [x] 카메라 추적 (`CM_PlayerCamera`: CinemachineCamera + CinemachinePositionComposer + CinemachineConfiner2D, `CameraBounds`: BoxCollider2D 61×52.91)
- [x] 플레이스홀더 스프라이트 색상 규칙 적용 — 실제 CraftPix 에셋을 직접 사용하므로 해당 없음 (에셋 자체에 색상 이미 포함)
- [x] 레벨디자인 문서의 구간①~⑤ 배치를 실제 Tilemap으로 구현 (`레벨디자인_MVP_스테이지.md` 기준 좌표로 구현 완료)
- [x] 시작~도착까지 끊김/오류 없이 동작하는지 기능적으로 1회 플레이
- [x] 작업 중간중간 `git add/commit/push` 수행 후 **GitHub 저장소에서 실제로 반영됐는지 확인** (커밋 시간/메시지로 확인)

**기획**
- [x] 구현된 Tilemap이 레벨디자인 문서의 의도(난이도 곡선)와 맞게 배치됐는지 확인 (ver3→ver4 좌표 검증 완료)
- [x] 같은 플레이를 하며 난이도/재미가 의도대로 체감되는지 평가

---

## 2. 핵심 기능 완료 후 확인할 것

> 출처: `QA_테스트_계획서.md` (T-01~T-09), `레벨디자인_MVP_스테이지.md` (6장)

**프로그래밍**
- [x] T-01~T-09 항목이 스펙대로 동작하는지 기능 확인 (영상 6종으로 기록 — `Assets/Videos/`)
- [x] 결정된 수치를 코드에 반영하고 `캐릭터컨트롤_스펙.md` 갱신
- [x] Windows 빌드 1회 진행, 해상도(1920x1080) 기준 정상 출력 확인

**기획**
- [x] 통과한 항목들이 실제로 의도한 체감과 일치하는지 최종 검토
- [x] 구간②~⑤ 갭/장애물이 너무 쉽거나 빡빡하지 않은지 체감 테스트
- [x] 코요테 타임이 실제로 체감되는지 확인
- [x] 필요 시 수치 조정 방향 결정 (±10~20% 단위)

---

## 3. 핵심 기능 완성 후 — 동아리 공유 준비

> 출처: `역할분담_문서.md`, `README.md`

**기획**
- [ ] 완성된 MVP 빌드/프로젝트를 부원들에게 공유
- [ ] README, 역할 분담 문서 최신 상태인지 한 번 더 확인
- [ ] 부원들에게 본인 역할에 맞는 문서부터 읽도록 안내

---

## 4. 확장 기능 단계 — 정령 시스템 착수 준비 (2026-07-09 갱신)

> 출처: `플랫포머_게임_기획서_확장기능.md`, `정령_시스템_기획서.md`, `몬스터_기획서.md`, `발판_확장_기획서.md`, `스탯_기획서.md`, `월드_스테이지_구성.md`

**기획**
- [x] 정령/몬스터/발판/스탯/월드 시스템 1차 설계 문서화 완료
- [x] `스탯_기획서.md`의 (추후 확정) 항목에 실제 수치 채우기 (2026-07-14, ver2)
- [x] `월드_스테이지_구성.md` 2장 스토리 구체화 → 별도 문서(`Docs/Design/Story/스토리.md`) 분리 (2026-07-14)
- [x] 하트(체력) 최대 개수 확정 (4개, 낙사 시 체력 0, 데미지 시 1씩 감소, UI는 좌측 상단·채워짐=빨강+검정 외곽선/빈칸=검정) (2026-07-14)
- [ ] 보스(밤 나무) 패턴 설계
- [ ] (보류) 크레딧 화면(또는 README 하단)에 체력 하트 아이콘(Flaticon) 출처 표기 문구 반영 — 단, 현재 실제 연결된 `ui_heart_full/empty.png`는 Flaticon과 무관한 자체 제작 임시 에셋이라 **지금은 표기 불필요**. `heart.png`/`heart_fill.png`(Flaticon 기반)를 나중에 정식 채택할 경우에만 적용 — `에셋_출처_라이선스_관리.md` 6장 참고
- [x] 몬스터/보스 에셋 3팩(100 Top Down Monsters Vol 1, craftpix-284465, craftpix-838021)의 정확한 출처·라이선스·출처표기 필요 여부 확인 (2026-07-16, CraftPix 2팩=표기 불필요 / 100 Top Down Monsters=Unity 에셋스토어 Single Entity License) — `에셋_출처_라이선스_관리.md` 4장 참고
- [ ] 확장 기능 기획서를 부원들에게 공유하고 관심 있는 항목 신청받기

**프로그래밍**
- [ ] 부원별로 `feature/` 브랜치 생성 안내
- [ ] 정령 상호작용(마커 표시, 마우스 클릭 처리) 프로토타입 구현
- [ ] 정령 선택/전환 로직(마우스 오른쪽 클릭) 구현
- [ ] 기본 정령(공격+플랫폼) 구현 → 이후 추가 정령 6종으로 확장 (이동 추적은 `FairyMovement.cs`로 구현 완료, 공격/플랫폼 생성 로직만 남음)
- [ ] `TerrainType`을 이동/무너짐/스탯변화 발판까지 포함하도록 확장
- [x] 하트(체력) 시스템 및 게임오버 로직 구현 (`PlayerHealth.cs`+`HeartUI.cs`로 구현 완료)
- [x] 무적 시간/깜빡임 값 확인 — Inspector 실측값은 0.5초/0.15초로, 스크립트 필드 초기값(10초/0.1초)과 다름을 확인. 문서는 Inspector 실측값 기준으로 유지 (2026-07-16, `캐릭터컨트롤_스펙.md` ver9)
- [x] 하트 UI 방식 — 문서를 실제 코드(단일 스프라이트 교체) 방식에 맞게 수정 완료 (2026-07-16, `월드_스테이지_구성.md` ver5)
- [ ] `HeartUI.cs`의 `fullHeartSprite`/`emptyHeartSprite`(`ui_heart_full.png`/`ui_heart_empty.png`)는 Unity 기능으로 만든 임시 플레이스홀더 — 정식 아트로 교체 필요 (2026-07-21 확인, `월드_스테이지_구성.md` 6.1절 참고). 하트 UI 구조 자체(`HeartContainer`+`HeartPrefab.prefab`, `Assets/_Project/Prefabs/UI/`)는 완성되어 있어 스프라이트만 교체하면 됨 (2026-07-23 확인)
- [ ] 미사용 상태인 `Assets/UI/Icons/Heart/heart.png`, `heart_fill.png`(Flaticon 기반) 처리 방향 결정 — 정식 아트로 채택하거나, 아니면 정리
- [ ] 몬스터 공통 스탯/AI 베이스 구현 → 근거리/원거리/디버프/보스로 확장 (부분 진행: 버섯 이동 스크립트(`GroundMonsterMovement.cs`) 애니메이션 연동 완료, 나머지 몬스터는 미착수)
- [x] 버섯(`GroundMonsterMovement.cs`) Animator 연동 — Speed/Attack 파라미터 반영 (2026-07-16 최초 적용 → 2026-07-23 팀원이 Patrol/Chase/Attack 상태 머신으로 전면 재작성, 병합 중 유실된 Animator 연동을 새 구조에 맞게 재적용, `몬스터_기획서.md` 5장·6.2절 참고)
- [ ] 나머지 지상 근접몹(나무 골렘, 식충식물)에도 동일한 Patrol/Chase/Attack 상태 머신 + Speed/Attack Animator 연동 패턴 적용
- [x] 공중형(벌) 이동 로직 구현 완료 (2026-07-23, 팀원 작성 `FloatingMonsterMovement.cs` — Patrol/Chase/Attack 상태 머신, `SmoothDamp` 자유 비행 + 시야 레이캐스트, 씬의 `BeeMarker`에 적용됨, `몬스터_기획서.md` 5.1절 참고)
- [ ] 벌(`BeeMarker`/`Bee_Idle_0`)에 Animator 부착 및 `AC_Monster_Bee.controller` 연결 + Speed/Attack 연동 (다른 몬스터와 동일 패턴)
- [ ] `MonsterHealth.cs` / `MonsterFacing.cs`를 실제 몬스터 오브젝트(MushroomMarker 등)에 부착
- [ ] QA에서 발견된 무적 시간/피격 판정 버그 3건 수정 (`QA_테스트_계획서.md` 10장 BUG-01~03 참고)
- [ ] 빈 껍데기 상태인 enum 3개에 실제 값 채우기: `RangedMonsterData.ProjectileTypes`, `DebuffMonsterData.DebuffTypes`, `Enum/PlatformTypes.cs` (`스탯_기획서.md`/`발판_확장_기획서.md` 참고)
- [ ] 몬스터/정령별 실제 데이터 에셋(.asset) 인스턴스 생성 — 2026-07-28 기준 `BeeData_temp_real.asset`, `MushroomData_temp_real.asset` 2개 생성됨, 나머지 몬스터 5종·정령 7종은 아직 없음
- [ ] 기존 데이터 에셋 2개(`BeeData_temp_real.asset`, `MushroomData_temp_real.asset`)를 새 네이밍 규칙(`SO_Monster_이름.asset`, `Assets/_Project/Data/Monsters/`)에 맞게 이름 변경·이동 (`에셋_네이밍_관리규칙.md` 3.3.1절 참고)

**아트**
- [ ] 정령 7종 색상 팔레트(HEX) 확정 (`아트스타일_가이드.md` 9.1절)
- [x] 기본 정령 멈춤(기본) 애니메이션 제작 (2026-07-14, `anim_fairy_base_idle.anim`, 01→02→03→02→01)
- [x] 기본 정령 멈춤/이동/상호작용 Animator 구성 완료 (2026-07-23, `AC_Fairy_Base.controller` — Idle/Move/InteractMove/InteractArrive/Interact 상태, `anim_fairy_base_idle`/`anim_fairy_base_interact` 클립 연결, `정령_시스템_기획서.md` 5.2절 참고)
- [ ] 기본 정령 공격 애니메이션 제작 (현재 이동 애니메이션과 클립 공유 중 — 별도 공격 클립 필요 여부 논의)
- [ ] `FairyInteractionController.cs`(정령 상호작용 로직, 미구현)에서 `Interact`/`Arrived` 트리거 및 `Speed` 파라미터 실제 호출 연결
- [ ] ⚠️ `AC_Fairy_Base.controller`의 `Interact` 상태가 현재는 한 번 재생 후 자동으로 Idle로 복귀하도록 되어 있는데, `정령_시스템_기획서.md` 2.2~2.3절에 맞춰 수정 필요: **Interact 애니메이션이 끝나면 Idle로 자동 복귀하는 대신 정령 오브젝트를 비활성화**하고, 해제 조건(공격키 입력 또는 카메라 이탈) 발생 시 다시 활성화하여 Idle로 돌아가도록 구조 변경
- [ ] 추가 정령(6종) 스프라이트/애니메이션 제작
- [x] 몬스터 6종 + 보스 스프라이트/애니메이션 확보 (2026-07-16, 자체 제작 대신 구매/무료 에셋 팩 사용 — `몬스터_기획서.md` 4장 에셋 경로 참고. 스프라이트+애니메이션 클립 모두 포함된 상태로 Assets에 존재)
- [x] 몬스터 Animator Controller 7종 생성 (2026-07-16, `Assets/_Project/Animators/Monsters/AC_Monster_이름.controller` — Slime만 클립 연결 완료, 나머지 6개는 구조만 있고 클립은 비어있음)
- [x] 나머지 6개 몬스터 Animator Controller에 실제 클립 채워넣기 완료 (2026-07-23 — Bee/Mushroom/Skeleton/TreeGolem은 에셋 팩 클립 연결, CarnivorousPlant/Boss_Ent는 클립이 없어 직접 제작해 연결, `몬스터_기획서.md` 6.1절 참고)
- [ ] 신규 발판 3종 타일/오브젝트 아트 제작 (`아트스타일_가이드.md` 9.3절)
- [x] 정령 변신 플랫폼용 임시 스프라이트 배치 (2026-07-28, `env_platform_fairy_01.png` — 정식 아트로 교체 필요, `정령_시스템_기획서.md` 2.2절 참고)
- [x] 하트 UI 아이콘 초안 제작 (2026-07-16, `heart.png`+`heart_fill.png` 2-레이어로 제작했으나 **실제로는 채택되지 않음** — 현재는 `ui_heart_full.png`/`ui_heart_empty.png` 임시 플레이스홀더가 연결되어 있음, `월드_스테이지_구성.md` 6.1절 참고)
- [ ] 마커 UI 목업 제작 (`아트스타일_가이드.md` 9.4절)

---

## 5. 보류 중 / 추후 결정 필요

**기획**
- [x] 사운드 큐 리스트 작성 (어떤 상황에 어떤 소리가 필요한지 — 점프/피격/클리어 등) → `사운드_큐_리스트.md` 작성 완료
- [x] 정령/몬스터 관련 사운드 큐 목록 작성 → `사운드_큐_리스트.md` 4장 작성 완료

**아트**
- [ ] `사운드_큐_리스트.md`를 바탕으로 실제 음원 제작/선정 및 톤 결정 (사운드 담당 정해진 후, 출처/라이선스는 `에셋_출처_라이선스_관리.md`에 기록)
- [ ] 에셋 네이밍 규칙에 실제 색상 팔레트(HEX 코드) 추가 (아트 부원 합류 후)

**프로그래밍**
- [ ] ToDo 리스트 관리 방식 — 마크다운 유지 vs GitHub Issues/Projects 전환 결정

---

## 6. 사용 방법

- 작업하다가 새로운 할 일이 생기면, 가장 관련된 단계(1~5) 안에서 해당 역할 묶음에 항목을 추가합니다.
- 두 역할이 함께 관여하는 작업은 하나로 합치지 말고, 역할별로 항목을 나눠서 적습니다.
- 단계 1~2가 모두 끝나면 핵심 기능 MVP 완성으로 간주하고, 3단계로 넘어갑니다.
- 이 문서는 수시로 갱신되는 문서이므로, 다른 기획 문서처럼 한 번 쓰고 고정하는 게 아니라 계속 체크/추가해나가면 됩니다.
