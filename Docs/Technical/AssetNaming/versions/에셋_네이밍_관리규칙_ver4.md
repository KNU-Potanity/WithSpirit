# 에셋 네이밍 / 관리 규칙

## 0. 문서 목적

이 문서는 이미지, 사운드, 프리팹, 씬 등 **에셋 파일의 이름을 어떻게 지을지, 어디에 둘지**를 정리합니다.

- 부원이 여러 명이 되면 각자 다른 이름 규칙으로 파일을 만들어서 같은 종류의 에셋이 중복되거나, 어떤 파일이 최신인지 헷갈리는 문제가 자주 생겨요.
- `기술기획서_MVP_Unity.md`의 폴더 구조, `아트스타일_가이드_MVP.md`의 분류를 기준으로 통일된 이름 규칙을 정합니다.
- 외부에서 에셋을 가져올 때의 출처/라이선스 기록은 `에셋_출처_라이선스_관리.md`에서 별도로 관리합니다.

---

## 1. 기본 원칙

| 원칙 | 내용 |
|---|---|
| 언어 | 영문 사용 (한글 파일명은 OS/Git 환경에 따라 깨질 수 있음) |
| 형식 | 스프라이트/사운드/텍스처 → `snake_case` (예: `char_player_idle_01`) |
| 형식 | 프리팹/씬 → `PascalCase`, 접두사 포함 (예: `PF_Player`) |
| 띄어쓰기 | 사용하지 않음 (공백 대신 `_` 사용) |
| 번호 | 2자리 숫자로 통일 (`01`, `02`, ... `09`, `10`) |

---

## 2. 폴더 구조 (재확인)

`기술기획서_MVP_Unity.md`의 구조를 기준으로, 에셋은 아래 위치에 둡니다.

```
Assets/_Project/
 ├─ Art/
 │   ├─ Characters/
 │   ├─ Environment/
 │   └─ Effects/
 ├─ Audio/
 │   ├─ SFX/
 │   └─ BGM/
 ├─ Prefabs/
 │   ├─ Player/
 │   ├─ Terrain/
 │   └─ UI/
 ├─ Tilemaps/
 └─ Scenes/
```

> 새로운 종류의 에셋이 생기면, 기존 폴더 안에 하위 폴더를 추가하는 방식으로 확장합니다 (폴더 구조 자체를 자주 바꾸지 않기).

---

## 3. 타입별 네이밍 규칙

### 3.1 스프라이트 (이미지)
형식: `분류_이름_상태_번호.png`

| 분류 접두사 | 의미 | 예시 |
|---|---|---|
| `char_` | 캐릭터 | `char_player_idle_01.png`, `char_player_jump_01.png` |
| `enemy_` | 적 (확장 기능) | `enemy_slime_walk_01.png` |
| `env_` | 지형/환경 | `env_platform_normal_01.png`, `env_hazard_spike_01.png` |
| `obj_` | 오브젝트 (목표 지점 등) | `obj_goal_flag_01.png` |
| `bg_` | 배경 | `bg_sky_layer1.png` |
| `ui_` | UI 요소 (확장 기능) | `ui_button_pause.png` |
| `fx_` | 이펙트 | `fx_jump_dust_01.png` |

### 3.2 애니메이션 / 애니메이터
형식: `anim_캐릭터또는오브젝트_동작.anim`
- 예: `anim_player_idle.anim`, `anim_player_jump.anim`, `anim_player_fall.anim`

### 3.3 프리팹
형식: `PF_이름` (필요시 `_세부` 추가)
- 예: `PF_Player`, `PF_Platform_Normal`, `PF_Hazard_Spike`, `PF_Goal`, `PF_Camera`

### 3.4 사운드
형식: `분류_이름.확장자`

| 분류 접두사 | 의미 | 예시 |
|---|---|---|
| `sfx_` | 효과음 | `sfx_jump.wav`, `sfx_hit.wav`, `sfx_clear.wav` |
| `bgm_` | 배경음악 | `bgm_stage1.mp3` |

### 3.5 씬 / 타일맵
- 씬: `Stage1`, `Stage2`, `MainMenu` (확장 기능), `TestScene_이름` (테스트용은 `TestScene_` 접두사로 구분)
- 타일맵: `Tilemap_Stage1_Ground`, `Tilemap_Stage1_Hazard` (스테이지+레이어 구분)

---

## 4. 임시(플레이스홀더) 에셋 표시 규칙

- MVP 단계에서 정식 아트 전에 사용하는 임시 에셋은 파일명에 `_temp` 접미사를 붙입니다.
  - 예: `env_platform_normal_temp.png`
- 정식 에셋으로 교체되면 `_temp` 파일은 삭제하고, 정식 파일은 접미사 없이 등록합니다.
- 코드/프리팹에서 임시 에셋을 참조하고 있다면, 교체 시 참조가 깨지지 않았는지 확인이 필요합니다.

---

## 5. 버전/배리에이션 관리

- 같은 에셋의 색상/형태 변형이 필요하면 번호 대신 의미 있는 접미사 사용
  - 예: `char_player_idle_01.png` (기본), `char_player_idle_01_red.png` (색상 변형)
- 작업 중 파일을 덮어쓰지 않고 새 버전을 만들 경우 `_v2`, `_v3` 형식 사용 후, 최종 확정되면 버전 표시 없는 이름으로 정리
  - 예: `char_player_idle_v2.png` → 확정 시 `char_player_idle_01.png`로 리네임

---

## 6. Git / LFS 관련 주의점

- 이미지(`.png`, `.psd`), 오디오(`.wav`, `.mp3`) 파일을 추가하기 전에 `기술기획서_MVP_Unity.md`에서 안내한 **Git LFS 설정**이 되어 있는지 먼저 확인합니다.
- 같은 파일을 여러 명이 동시에 수정하면 충돌이 발생하므로, 작업 전 해당 파일을 누가 수정 중인지 간단히 공유합니다 (예: 동아리 채팅방에 "지금 player 스프라이트 작업 중" 정도로 공유).

---

## 7. 체크리스트 (에셋 추가 전 확인)

- [ ] 올바른 폴더(`Art/Characters`, `Audio/SFX` 등)에 넣었는가
- [ ] 분류 접두사(`char_`, `env_`, `sfx_` 등)를 붙였는가
- [ ] 띄어쓰기, 한글, 대문자/소문자 혼용 없이 규칙을 지켰는가
- [ ] 임시 에셋이면 `_temp`를 붙였는가
- [ ] Git LFS 추적 대상 확장자에 포함되어 있는가
