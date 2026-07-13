# 기술 기획서 (Technical Design Document) — MVP

## 0. 문서 목적

이 문서는 `플랫포머_게임_기획서_MVP.md`, `레벨디자인_MVP_스테이지1.md`, `캐릭터컨트롤_스펙_MVP.md`에서 정의한 내용을 **Unity 엔진으로 어떻게 구현할지**, 그리고 **GitHub로 어떻게 협업할지**를 정리합니다.

- 사용 엔진: **Unity (고정)**
- 협업 툴: **GitHub (고정)**
- 목표는 동아리 부원 여러 명이 동시에 작업해도 충돌이 적게 나도록 구조를 미리 정하는 것입니다.
- **ver4 변경 사항**: 실제로 사용하는 CraftPix `SimplePlatformer2D` 에셋의 경로를 4.7절에 표로 추가했고, Tilemap 구현 과정에서 확정된 세부사항(4.2절)을 보강했습니다.

---

## 1. 개발 환경

| 항목 | 내용 |
|---|---|
| 엔진 | Unity (LTS 버전 사용 권장) |
| 스크립팅 언어 | C# |
| 2D/3D | 2D 프로젝트 (2D Built-in Renderer 또는 URP 2D) |
| 타겟 플랫폼 | PC (Windows 빌드 기준, 추후 확장 가능) |

> 부원 전체가 **동일한 Unity 버전**을 사용해야 합니다. 버전이 다르면 프로젝트 파일이 깨지거나 GitHub에서 불필요한 충돌이 발생할 수 있어요. 프로젝트 시작 시 버전을 한 번 확정하고 모두 동일하게 설치합니다.

---

## 2. GitHub 협업 규칙

### 2.1 브랜치 전략

| 브랜치 | 용도 |
|---|---|
| `main` | 항상 정상 실행되는 안정 버전만 유지 |
| `develop` | 다음 기능들을 통합하는 작업용 브랜치 |
| `feature/기능명` | 개별 기능 작업 (예: `feature/player-movement`, `feature/camera-follow`) |

- 부원은 각자 `feature/` 브랜치를 만들어 작업 → `develop`으로 Pull Request → 리뷰 후 머지
- `main`은 `develop`이 충분히 안정화된 시점에만 병합 (예: MVP 완성 시점)

### 2.2 커밋 규칙
- 커밋 메시지는 `[타입] 내용` 형식 권장 (예: `[feat] 코요테 타임 구현`, `[fix] 리스폰 위치 오류 수정`)
- 타입 예시: `feat`(기능 추가), `fix`(버그 수정), `level`(레벨 데이터 수정), `docs`(문서)

### 2.3 Unity 프로젝트의 Git 설정 (필수)

Unity 프로젝트는 일반 코드 프로젝트와 달리 아래 설정을 꼭 해야 GitHub에서 충돌 없이 협업할 수 있습니다.

1. **Edit > Project Settings > Editor**
   - `Asset Serialization` → **Force Text**로 설정 (씬/프리팹 파일을 텍스트로 저장해야 Git diff가 가능)
   - `Version Control` → **Visible Meta Files**로 설정 (.meta 파일이 같이 추적되어야 함)
2. **.gitignore**
   - Unity 공식 `.gitignore` 템플릿 사용 (`Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/`, `.vs/` 등 제외)
3. **Git LFS (Large File Storage)**
   - 이미지, 오디오 등 바이너리 에셋이 늘어나면 Git LFS 사용을 권장 (`.png`, `.psd`, `.wav`, `.mp3` 등)
4. **씬/프리팹 동시 작업 주의**
   - 같은 씬 파일을 여러 명이 동시에 수정하면 충돌이 잦습니다. 가능하면 **한 씬은 한 명만 작업**하거나, 작업 전 미리 공유하는 규칙을 둡니다.

---

## 3. 프로젝트 폴더 구조

```
Assets/
 ├─ _Project/
 │   ├─ Scripts/
 │   │   ├─ Player/         # 이동, 점프, 코요테 타임 등
 │   │   ├─ Terrain/        # 발판, 즉사 장애물
 │   │   ├─ Respawn/        # 리스폰 처리
 │   │   ├─ Goal/           # 도착 지점
 │   │   ├─ Camera/         # 카메라 추적
 │   │   └─ Core/           # 이벤트, 공용 유틸
 │   ├─ Prefabs/
 │   │   ├─ Player/
 │   │   ├─ Terrain/
 │   │   └─ UI/
 │   ├─ Scenes/
 │   │   └─ Stage1.unity
 │   ├─ Tiles/              # Tilemap용 Tile 에셋 (.asset) 및 PPU 조정된 소스 스프라이트
 │   │   └─ Sources/        # 원본 타일 PNG를 PPU=16으로 재설정한 복제본
 │   ├─ Art/
 │   │   ├─ Characters/
 │   │   ├─ Environment/
 │   │   └─ Effects/
 │   └─ Audio/
 │       ├─ SFX/
 │       └─ BGM/
 └─ Plugins/ (필요한 외부 패키지)
```

> 하위 폴더의 세부 분류 기준은 `에셋_네이밍_관리규칙.md` 2장과 동일합니다.

> `_Project` 폴더로 모든 작업물을 모아두면, 외부 에셋(스토어 에셋 등)과 우리 작업물이 섞이지 않아 정리가 쉬워요.

---

## 4. 핵심 시스템 구현 방식

### 4.1 캐릭터 컨트롤러
- Unity 기본 `Rigidbody2D` 물리 시뮬레이션에 전부 맡기지 않고, **이동/점프 값은 스크립트에서 직접 계산**하는 방식을 권장합니다.
  - 이유: 플랫포머는 정밀한 조작감이 핵심인데, 물리 엔진에 맡기면 가속/감속, 점프 높이 같은 수치를 정확히 맞추기 어려워요.
  - `캐릭터컨트롤_스펙_MVP.md`에 정의된 가속/감속 시간, 점프 높이 값을 그대로 코드 상의 변수로 옮겨서 사용합니다.
- 충돌 감지는 `Rigidbody2D`(Kinematic 모드) + `Collider2D` 또는 레이캐스트 기반 충돌 감지 중 택1 (구현 난이도가 낮은 쪽으로 시작 후 필요 시 교체 가능)
- 코요테 타임, 가변 점프는 별도의 작은 타이머/상태 변수로 관리 (이동 로직과 분리된 모듈로 작성 — 확장 기능 문서의 "이동 로직과 충돌 로직 분리" 원칙 준수)
- 애니메이션 상태는 4.7절의 캐릭터 에셋(대기/이동/점프 상승/낙하) 4종을 그대로 매핑

### 4.2 지형 / 충돌 (구현 완료)
- Unity의 **Tilemap** 기능으로 발판을 구현했습니다. `Grid` → 자식 `Tilemap`(이름: Ground) 구조이며, `TilemapCollider2D` + `Rigidbody2D(Static)` + `CompositeCollider2D` 조합으로 구간별 충돌체가 자동 병합되도록 구성했습니다.
- **타일 에셋 경로**: `Assets/_Project/Tiles/*_Tile.asset` (9종, 4.7절 발판 에셋 9종에 대응)
- **PPU 조정 관련 주의사항**: CraftPix 타일 원본은 16px(스프라이트 임포트 PPU=100 기준 0.16 유닛)로, 1타일=1유닛 기준에 맞지 않습니다. Grid의 Transform 스케일로 보정하면 `TilemapRenderer`의 컬링 바운드 계산이 깨지는 문제가 있어, 대신 **원본 PNG를 `Assets/_Project/Tiles/Sources/`에 복제하고 PPU를 16으로 재설정**하여 1타일이 정확히 1유닛으로 렌더링되도록 했습니다. Tile 에셋의 `sprite` 필드는 이 복제본을 참조합니다. (원본 CraftPix 에셋의 임포트 설정은 변경하지 않았습니다.)
- 즉사 장애물(가시)은 Tilemap이 아닌 **개별 GameObject + 트리거 콜라이더**로 별도 구현 (지형과 분리, 확장 기능 문서의 "지형 타입 구분" 원칙과 별개로 처리)
- 지형 타입은 `enum TerrainType { Normal, Hazard }` 형태로 코드 상에서 구분 → 추후 이동 발판, 스프링 등 추가 시 타입만 늘리면 되도록 설계

### 4.3 리스폰 시스템
- `RespawnManager` 같은 별도 컴포넌트를 두고, "현재 리스폰 지점이 어디인지"를 관리
- MVP에서는 항상 스테이지 시작 지점을 반환하지만, 추후 체크포인트가 추가되면 이 매니저 내부 로직만 바꾸면 되도록 분리

### 4.4 레벨 데이터 관리
- 레벨 레이아웃(발판/장애물 배치)은 Unity **Tilemap + 씬 파일**로 관리 (코드에 하드코딩하지 않음) — 구현 완료 (`Level_Stage1_ver2` 씬 객체, 좌표는 `레벨디자인_MVP_스테이지_ver3.md` 기준)
- 시작 지점, 도착 지점 좌표 등은 씬 안의 빈 GameObject(마커)로 배치하고 스크립트에서 참조
- 추후 여러 스테이지로 확장될 경우를 대비해, 스테이지마다 별도 씬 파일로 분리하는 구조를 유지

### 4.5 카메라
- **Cinemachine 패키지**(Unity 공식 카메라 패키지) 사용을 권장
  - `CinemachineVirtualCamera`로 플레이어 추적 + `CinemachineConfiner2D`로 스테이지 경계에서 카메라가 멈추도록 설정
  - MVP 사양(플레이어 추적형, 좌우 스크롤만, 줌/흔들림 없음)에 맞게 **추가 기능은 비활성화 상태로 둠**
  - Cinemachine을 쓰면 추후 자동 스크롤, 화면 흔들림 등 확장 기능을 켜고 끄는 것도 비교적 수월함
  - (아직 미구현 — PlayerController 구현 후 진행)

### 4.6 이벤트 시스템
- "점프했다", "장애물에 닿았다", "도착 지점에 도달했다" 같은 동작을 C# `event` 또는 `UnityEvent`로 정의
- 지금은 UI/사운드가 없지만, 이벤트만 미리 만들어두면 나중에 누가 사운드나 UI를 추가할 때 기존 로직을 건드리지 않고 이벤트에 연결만 하면 됨
- 각 이벤트에 어떤 사운드가 필요한지는 `사운드_큐_리스트.md`에 기획 측에서 미리 정리해 두었으므로, 사운드 연동 시 이벤트 이름과 큐 리스트의 이벤트명이 일치하는지 확인

### 4.7 사용 에셋 매핑

CraftPix `SimplePlatformer2D` 에셋 팩 내에서 실제로 사용하는 원본 에셋의 경로입니다. 코드에서 `AssetDatabase.LoadAssetAtPath` 등으로 참조할 때 이 표를 기준으로 합니다.

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

> 발판 9종은 4.2절에서 설명한 대로 `Assets/_Project/Tiles/Sources/`의 PPU=16 복제본을 통해 Tile 에셋(`Assets/_Project/Tiles/*_Tile.asset`)으로 변환되어 사용됩니다. 캐릭터/가시/도착 지점은 원본 스프라이트를 그대로 참조합니다.

---

## 5. 입력 시스템

- Unity **Input System 패키지** 사용 권장 (구버전 Input Manager보다 키 재매핑 등 확장에 유리)
- 입력 액션: `Move(좌우)`, `Jump(점프, 누름/뗌 모두 감지 필요 — 가변 점프 때문)`

---

## 6. 코딩 / 네이밍 규칙

| 항목 | 규칙 |
|---|---|
| 클래스명 | PascalCase (예: `PlayerController`) |
| 변수명 | camelCase (예: `moveSpeed`) |
| 상수/설정값 | 가능하면 `[SerializeField]`로 Inspector에 노출해 코드 수정 없이 조정 가능하게 함 |
| 주석 | 핵심 수치(점프 높이 등)는 `캐릭터컨트롤_스펙_MVP.md`의 어느 항목에 해당하는지 주석으로 표기 |

---

## 7. 빌드 설정

| 항목 | 내용 |
|---|---|
| 타겟 플랫폼 | Windows (PC) 기준 1차 빌드 |
| 해상도 | 16:9 기준 (예: 1920x1080), 카메라/UI는 해당 비율 기준 설계 |
| 빌드 주기 | MVP 완료 시점에 1차 빌드, 이후 확장 기능 추가될 때마다 재빌드 |

---

## 8. 다음 단계

1. ~~GitHub 저장소 생성 + Git 설정 적용~~ → 완료
2. ~~`레벨디자인_MVP_스테이지1.md` 레이아웃을 Tilemap으로 제작~~ → 완료 (`레벨디자인_MVP_스테이지_ver3.md` 기준 좌표로 구현됨)
3. `캐릭터컨트롤_스펙_MVP.md` 수치를 기반으로 `PlayerController` 구현 (4.7절 캐릭터 에셋 4종 연동)
4. 카메라(Cinemachine) 연결 후 전체 플레이 테스트
5. 가시(Hazard) 충돌 시 리스폰 이벤트 연결, 도착 지점 트리거에 클리어 이벤트 연결
