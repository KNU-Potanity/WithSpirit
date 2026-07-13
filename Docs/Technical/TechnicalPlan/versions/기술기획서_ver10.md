# 기술 기획서 (Technical Design Document) — MVP

> 📎 MVP(핵심 기능) 단계 문서 — 회장(PM)이 혼자 구현한 기존 기능 정의입니다. 새 기능 설계 시 기존 기준으로 참고하세요.


## 0. 문서 목적

이 문서는 `플랫포머_게임_기획서.md`, `레벨디자인_MVP_스테이지.md`, `캐릭터컨트롤_스펙.md`에서 정의한 내용을 **Unity 엔진으로 어떻게 구현했는지**, 그리고 **GitHub로 어떻게 협업할지**를 정리합니다.

- 사용 엔진: **Unity (고정)**
- 협업 툴: **GitHub (고정)**
- **ver6 변경 사항**: 카메라/리스폰/스크립트 구조를 실제 구현 완료 상태로 전면 갱신. 스크립트가 단일 PlayerController가 아닌 역할별로 분리된 구조로 구현되었음을 반영. t.cs(빈 테스트 파일) 삭제.
- **2026-07-09 개정**: 정령 시스템 채택에 따라 3장 폴더 구조에 정령/몬스터 스크립트·에셋 폴더(예정)를 추가하고, 4.8절에 확장 시스템 구현 방향을 정리했습니다. 아직 코드로 구현된 내용은 아닙니다.

---

## 1. 개발 환경

| 항목 | 내용 |
|---|---|
| 엔진 | Unity (LTS 버전 사용 권장) |
| 스크립팅 언어 | C# |
| 2D/3D | 2D 프로젝트 (URP 2D) |
| 타겟 플랫폼 | PC (Windows 빌드 기준, 추후 확장 가능) |

> 부원 전체가 **동일한 Unity 버전**을 사용해야 합니다. 버전이 다르면 프로젝트 파일이 깨지거나 GitHub에서 불필요한 충돌이 발생할 수 있어요. 프로젝트 시작 시 버전을 한 번 확정하고 모두 동일하게 설치합니다.

---

## 2. GitHub 협업 규칙

### 2.1 브랜치 전략

| 브랜치 | 용도 |
|---|---|
| `main` | 항상 정상 실행되는 안정 버전만 유지 |
| `develop` | 다음 기능들을 통합하는 작업용 브랜치 |
| `feature/기능명` | 개별 기능 작업 (예: `feature/player-movement`, `feature/camera-follow`, `feature/spirit-system`) |

- 부원은 각자 `feature/` 브랜치를 만들어 작업 → `develop`으로 Pull Request → 리뷰 후 머지
- `main`은 `develop`이 충분히 안정화된 시점에만 병합 (예: MVP 완성 시점)

### 2.2 커밋 규칙
- 커밋 메시지는 `[타입] 내용` 형식 권장 (예: `[feat] 코요테 타임 구현`, `[fix] 리스폰 위치 오류 수정`)
- 타입 예시: `feat`(기능 추가), `fix`(버그 수정), `level`(레벨 데이터 수정), `docs`(문서)

### 2.3 Unity 프로젝트의 Git 설정 (필수)

1. **Edit > Project Settings > Editor**
   - `Asset Serialization` → **Force Text**로 설정
   - `Version Control` → **Visible Meta Files**로 설정
2. **.gitignore**: Unity 공식 `.gitignore` 템플릿 사용
3. **Git LFS**: 이미지, 오디오 등 바이너리 에셋 증가 시 권장 (`.png`, `.wav`, `.mp3` 등)
4. **씬/프리팹 동시 작업 주의**: 같은 씬을 여러 명이 동시에 수정하면 충돌이 잦습니다.

---

## 3. 프로젝트 폴더 구조

```
Assets/
 ├─ _Project/
 │   ├─ Scripts/
 │   │   ├─ Player/         # PlayerMovement, PlayerJump, PlayerGroundDetector, PlayerAnimatorController
 │   │   ├─ Terrain/        # Ground, HazardTrigger, TerrainType
 │   │   ├─ Respawn/        # RespawnManager, FallRespawnDetector
 │   │   ├─ Goal/           # GoalTrigger
 │   │   ├─ Spirits/        # (예정) 정령 상호작용, 정령별 공격/플랫폼 생성 로직
 │   │   ├─ Monsters/       # (예정) 몬스터 공통/근거리/원거리/디버프/보스 AI
 │   │   └─ Camera/         # (추후 카메라 관련 스크립트 추가 시)
 │   ├─ Prefabs/
 │   │   ├─ Player/
 │   │   ├─ Spirits/        # (예정)
 │   │   ├─ Monsters/       # (예정)
 │   │   ├─ Terrain/
 │   │   └─ UI/
 │   ├─ Scenes/
 │   │   └─ SampleScene.unity    # 현재 사용 중인 씬 (추후 Stage1.unity로 변경 예정)
 │   ├─ Tiles/              # Tilemap용 Tile 에셋 (.asset)
 │   │   └─ Sources/        # 원본 PNG를 PPU=16으로 재설정한 복제본
 │   ├─ Art/
 │   │   ├─ Characters/
 │   │   ├─ Spirits/        # (예정)
 │   │   ├─ Monsters/       # (예정)
 │   │   ├─ Environment/
 │   │   └─ Effects/
 │   └─ Audio/
 │       ├─ SFX/
 │       └─ BGM/
 └─ Plugins/
```

> `_Project` 폴더로 모든 작업물을 모아두면, 외부 에셋(CraftPix 등)과 우리 작업물이 섞이지 않아 정리가 쉽습니다. (예정) 표시된 폴더는 정령/몬스터 시스템 구현 착수 시 생성합니다.

---

## 4. 핵심 시스템 구현 방식

### 4.1 캐릭터 컨트롤러 (구현 완료)

단일 스크립트가 아니라 **역할별로 분리된 4개 컴포넌트**로 구현되었습니다. 모두 `PlayerStartMarker` 오브젝트(또는 자식)에 붙어있습니다.

| 스크립트 | 위치 | 역할 |
|---|---|---|
| `PlayerMovement.cs` | PlayerStartMarker | 좌우 이동(가속/감속), 경계 클램프 |
| `PlayerJump.cs` | PlayerStartMarker | 기본 점프, 가변 점프, 코요테 타임 |
| `PlayerGroundDetector.cs` | PlayerStartMarker | 물리 충돌 이벤트 기반 접지 판정 |
| `PlayerAnimatorController.cs` | Visual (자식) | PlayerMovement/Jump 상태 → Animator 파라미터 연결, flipX 처리 |
| `FallRespawnDetector.cs` | PlayerStartMarker | 낙사 감지 (killPlaneY 이하 시 리스폰) |

- 구현 방식: `캐릭터컨트롤_스펙.md` 수치(속도/가속/감속/점프 높이/코요테 타임)를 역산 공식으로 직접 계산 (`h = 0.5 * v0 * t` → `v0 = 2h/t`, `g = 2h/t²`)
- 입력: Unity Input System 패키지 직접 사용 (Move: A/D/화살표, Jump: Space/W/위 화살표)
- `Rigidbody2D.gravityScale = 0` — 물리 중력을 끄고 스크립트에서 직접 수직 속도를 제어
- (예정) 공격/정령 선택 입력(마우스 왼쪽/오른쪽 클릭)은 정령 시스템 구현 시 별도 입력 핸들러로 추가 예정 (`캐릭터컨트롤_스펙.md` 8장 참고)

### 4.2 지형 / 충돌 (구현 완료, 확장 예정)

- **Tilemap**: `Grid` → 자식 `Ground(Tilemap)` 구조. `TilemapCollider2D` + `Rigidbody2D(Static)` + `CompositeCollider2D` 조합으로 구간별 충돌체 자동 병합
- **Grid 설정**: `cellSize=(1,1,1)`, `position=(-0.5, -1.0, 0.0)` (타일(0,0) 기준 발판 표면 y=0)
- **Tilemap 깊이**: row 0(top) ~ row -10(bottom), 총 11단
- **타일 에셋**: `Assets/_Project/Tiles/*_Tile.asset` (9종)
- **PPU 주의사항**: 원본은 16px(PPU=100 기준 0.16유닛), `Sources/`에 복제 후 PPU=16으로 재설정해 1타일=1유닛으로 렌더링
- **즉사 장애물**: `HazardTrigger.cs` (트리거 콜라이더 + OnTriggerEnter/Stay2D, 쿨다운 처리)
- **지형 타입**: `TerrainType.cs` (Normal / Hazard enum), `Ground.cs` (지형 마커 컴포넌트)
- (예정) `TerrainType`에 이동 발판/무너지는 발판/스탯 변화 발판 타입 추가 — `발판_확장_기획서.md` 참고

### 4.3 리스폰 시스템 (구현 완료, 확장 예정)

- `RespawnManager.cs`: 씬 리로드 방식으로 리스폰 처리 (`SceneManager.LoadScene(buildIndex)`)
- `FallRespawnDetector.cs`: `PlayerStartMarker`에 붙어서 killPlaneY 이하 추락 감지 → RespawnManager 호출
- `HazardTrigger.cs`: 가시 트리거 콜라이더에 붙어서 플레이어 접촉 감지 → RespawnManager 호출
- MVP 이후 체크포인트가 추가되면 `RespawnManager.RespawnPlayer()` 내부 로직만 교체하면 됨
- (예정) 하트(체력) 시스템 도입 시, 낙사는 기존처럼 즉시 리스폰하고 그 외 데미지는 하트 차감으로 분기 처리 — `월드_스테이지_구성.md` 6장 참고

### 4.4 레벨 데이터 관리 (구현 완료)

- 레벨 레이아웃: `SampleScene.unity` 안의 `Level_Stage1_ver2` 오브젝트 (좌표: `레벨디자인_MVP_스테이지.md` 기준)
- 시작 지점: `PlayerStartMarker` (0, 0.75, 0)
- 도착 지점: `GoalPoint` (60, 0.60, 0) — `GoalTrigger.cs` 부착, `End_Idle_0` 스프라이트 표시
- (예정) 다중 테마·다중 스테이지 확장 시 스테이지별 씬 분리 필요 — `월드_스테이지_구성.md` 1장 참고

### 4.5 카메라 (구현 완료)

- **Cinemachine 3.x** 패키지 사용 (`Unity.Cinemachine` 네임스페이스)
- `CM_PlayerCamera` 오브젝트: `CinemachineCamera` + `CinemachinePositionComposer` + `CinemachineConfiner2D`
- `Main Camera`에 `CinemachineBrain` 부착
- `CameraBounds` 오브젝트: `BoxCollider2D` (size=61×52.91, center=(30, 23.55)) → `CinemachineConfiner2D`의 경계로 사용
- MVP 사양(플레이어 추적, 좌우 스크롤, 경계에서 멈춤) 구현 완료
- 확장 단계에서도 카메라 로직은 수정하지 않기로 결정됨 (`월드_스테이지_구성.md` 4장)

> **주의**: Cinemachine 3.x는 구버전과 API가 다릅니다. `CinemachineVirtualCamera`(구버전) 대신 `CinemachineCamera`를 사용하고, body/aim 컴포넌트 대신 `CinemachinePositionComposer`를 별도 컴포넌트로 붙이는 방식입니다.

### 4.6 이벤트 시스템

- 현재 이벤트는 스크립트 직접 호출(예: `respawnManager.RespawnPlayer()`, `Debug.Log(Clear)`) 방식으로 처리
- 추후 사운드/UI가 추가될 때를 대비해 이벤트를 `C# event` 또는 `UnityEvent`로 공식 분리하는 것을 권장
- 각 이벤트에 필요한 사운드는 `사운드_큐_리스트.md` 참고 (정령/몬스터/하트 관련 이벤트는 4장에 추가됨)

### 4.7 사용 에셋 매핑

| 분류 | 용도 | 원본 에셋 경로 |
|---|---|---|
| 캐릭터 | 대기(Idle) | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Idle` |
| 캐릭터 | 이동(Run) | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Run` |
| 캐릭터 | 점프 상승(Jump) | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Jump` |
| 캐릭터 | 낙하(Fall) | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Fall` |
| 발판 | 위쪽 왼쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_01` |
| 발판 | 위쪽 중간 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_02` |
| 발판 | 위쪽 오른쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_03` |
| 발판 | 중간 왼쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_15` |
| 발판 | 중간 중간 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_16` |
| 발판 | 중간 오른쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_17` |
| 발판 | 아래쪽 왼쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_29` |
| 발판 | 아래쪽 중간 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_30` |
| 발판 | 아래쪽 오른쪽 | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_31` |
| 즉사 장애물 | thorn | `Assets/CraftPix/SimplePlatformer2D/6 Traps/4_2` |
| 도착 지점 | finish | `Assets/CraftPix/SimplePlatformer2D/3 Objects/Checkpoints/End_Idle` |

> 정령/몬스터/신규 발판용 에셋은 아직 확보되지 않았습니다. 확보되면 이 표에 행을 추가합니다.

### 4.8 확장 시스템 구현 예정 (정령/몬스터) — 2026-07-09 추가

아직 코드로 구현되지 않았으며, 착수 시 아래 방향을 기준으로 진행합니다.

| 시스템 | 구현 방향 | 관련 설계 문서 |
|---|---|---|
| 정령 상호작용 | 마우스 위치의 대상(설치 장소/몬스터)을 레이캐스트 등으로 감지해 마커 표시, 클릭 시 이벤트 발생 | `정령_시스템_기획서.md` 2장 |
| 정령 종류 | 기본 정령(공격+플랫폼 겸용) 1개 + 추가 정령(공격/플랫폼 특화) 최대 3개, `ScriptableObject` 등으로 정령 데이터 분리 고려 | `정령_시스템_기획서.md` 3장 |
| 몬스터 AI | 공통 베이스(체력/이동속도/공격력/쿨타임) 후 근거리/원거리/디버프/보스로 상속 또는 컴포지션 확장 | `몬스터_기획서.md`, `스탯_기획서.md` |
| 발판 확장 | 기존 `TerrainType`/`Ground` 구조를 확장해 이동/무너짐/스탯변화 타입 추가 | `발판_확장_기획서.md` |
| 체력 시스템 | 하트 단위 체력 컴포넌트 신설, 낙사는 기존 리스폰 로직 유지, 그 외 데미지는 하트 차감 후 0이 되면 게임오버 이벤트 발생 | `월드_스테이지_구성.md` 6~8장 |

---

## 5. 입력 시스템

- Unity **Input System 패키지** 사용 (구버전 Input Manager 아님)
- 입력 액션은 각 스크립트 Awake에서 직접 생성 (별도 Input Action Asset 미사용)
  - 이동: A/D/좌우 화살표 → `PlayerMovement.cs`
  - 점프: Space/W/위 화살표 → `PlayerJump.cs` (누름/뗌 모두 감지, 버퍼링 처리)
  - (예정) 공격/기본 상호작용: 마우스 왼쪽 클릭
  - (예정) 정령 선택: 마우스 오른쪽 클릭

---

## 6. 코딩 / 네이밍 규칙

| 항목 | 규칙 |
|---|---|
| 클래스명 | PascalCase (예: `PlayerMovement`) |
| 변수명 | camelCase (예: `moveSpeed`) |
| 네임스페이스 | `BasePlatformer.Player`, `BasePlatformer.Terrain`, `BasePlatformer.Respawn`, `BasePlatformer.Goal`, `BasePlatformer.Spirits`(예정), `BasePlatformer.Monsters`(예정) |
| 상수/설정값 | `[SerializeField]`로 Inspector에 노출 |
| 주석 | 핵심 수치는 `캐릭터컨트롤_스펙.md` 또는 `스탯_기획서.md`의 어느 항목에 해당하는지 표기 |

---

## 7. 빌드 설정

| 항목 | 내용 |
|---|---|
| 타겟 플랫폼 | Windows (PC) 기준 1차 빌드 |
| 해상도 | 16:9 기준 (예: 1920x1080) |
| 빌드 주기 | MVP 완료 시점에 1차 빌드, 이후 확장 기능 추가 때마다 재빌드 |

---

## 8. 다음 단계

1. ~~GitHub 저장소 생성 + Git 설정 적용~~ → 완료
2. ~~Tilemap 레벨 배치~~ → 완료 (`레벨디자인_MVP_스테이지.md` 기준)
3. ~~PlayerController(이동/점프/코요테 타임) 구현~~ → 완료 (PlayerMovement + PlayerJump + PlayerGroundDetector)
4. ~~카메라(Cinemachine) 연결~~ → 완료 (CM_PlayerCamera, CameraBounds)
5. ~~가시 충돌 시 리스폰 이벤트 연결~~ → 완료 (HazardTrigger + RespawnManager)
6. ~~도착 지점 트리거 클리어 이벤트 연결~~ → 완료 (GoalTrigger)
7. ~~전체 플레이 테스트~~ → 완료 (QA 계획서 T-01~T-09 전체 통과, 2026-06-30)
8. ~~Windows 빌드 1회 진행 및 해상도 확인~~ → 완료 (1920x1080 기준 확인)
9. 정령 상호작용/기본 정령 프로토타입 구현 착수 (`ToDoList.md` 4장 참고)
