# BasePlatformer

2D 사이드스크롤 플랫포머 장르의 **공통 핵심 기능만 최소한으로 구현**하는 것을 목표로 하는 프로젝트입니다.

- **엔진**: Unity
- **협업 툴**: GitHub
- **현재 상태**: 핵심 기능(MVP) 1인 제작 및 플레이 테스트(T-01~T-09) **완료** — 동아리 부원 확장 기능 단계 준비 중 → 완료 후 동아리 부원들이 확장 기능을 자율적으로 추가하는 단계로 이어질 예정

---

## 📂 문서 목록

모든 기획/기술 문서는 `Docs/` 폴더 아래에 역할별로 정리되어 있습니다.

### Design (기획)
- [핵심 기능 기획서 (MVP)](Docs/Design/Core/플랫포머_게임_기획서.md) — 반드시 구현해야 하는 5가지 핵심 기능 정의
- [확장 기능 기획서](Docs/Design/PlusContents/플랫포머_게임_기획서_확장기능.md) — 동아리 부원이 자율적으로 선택해 추가하는 기능 목록
- [레벨 디자인 문서](Docs/Design/LevelDesign/레벨디자인_MVP_스테이지.md) — MVP 스테이지 1의 구간별 레이아웃
- [캐릭터 컨트롤 스펙](Docs/Design/CharacterControls/캐릭터컨트롤_스펙.md) — 이동/점프/코요테 타임 수치
- [사운드 큐 리스트](Docs/Design/SoundQueue/사운드_큐_리스트.md) — 어떤 상황에 어떤 사운드가 필요한지 정리

### Technical (기술)
- [기술 기획서 (Unity)](Docs/Technical/TechnicalPlan/기술기획서.md) — 엔진 구현 방식, GitHub 협업 규칙
- [에셋 네이밍/관리 규칙](Docs/Technical/AssetNaming/에셋_네이밍_관리규칙.md) — 파일명, 폴더 구조 규칙
- [에셋 출처/라이선스 관리](Docs/Technical/License/에셋_출처_라이선스_관리.md) — 외부 에셋 소스 추천, 라이선스 기록표
- [스크립트 설명서](Docs/Technical/ScriptDocs/스크립트_설명서.md) — 각 스크립트의 역할과 동작 방식 설명

### Art (아트)
- [아트 스타일 가이드](Docs/Art/ArtStyle/아트스타일_가이드.md) — 색상, 해상도, 캐릭터/지형 디자인 기준

### Production (운영)
- [일정/마일스톤](Docs/Production/Milestone/일정_마일스톤.md) — 핵심 기능 제작 일정
- [역할 분담 문서](Docs/Production/DivisionOfRoles/역할분담_문서.md) — PM/프로그래밍/기획/아트 역할별 가이드
- [QA/테스트 계획서](Docs/Production/QATest/QA_테스트_계획서.md) — 테스트 케이스, 버그 리포트 양식
- [ToDoList](Docs/Production/ToDoList/ToDoList.md) — 현재 해야 할 일을 단계별/역할별로 모아 추적
- [문서 작성 가이드라인](Docs/Production/DocGuideline/문서작성_가이드라인.md) — 문서 폴더 구조, 버전 관리, 네이밍 규칙

---

## 📌 새 기능 기획 전 참고 문서

아래 문서들은 회장(PM)이 **혼자 구현한 MVP(핵심 기능) 단계**의 산출물입니다.
동아리 부원들과 새로운 기능을 논의하기 전에, 먼저 아래 문서를 읽고 기존에 어떤 수치/구조가 이미 정해져 있는지 파악해 주세요.

- [게임 기획서](Docs/Design/Core/플랫포머_게임_기획서.md) — 전체 게임 방향성과 핵심 기능 정의
- [캐릭터컨트롤 스펙](Docs/Design/CharacterControls/캐릭터컨트롤_스펙.md) — 이동/점프/코요테 타임 등 기존 조작 수치
- [레벨디자인 문서](Docs/Design/LevelDesign/레벨디자인_MVP_스테이지.md) — 기존 스테이지 구조
- [기술기획서](Docs/Technical/TechnicalPlan/기술기획서.md) — 기존 코드/폴더 구조

---

## 🚀 시작하기

1. 이 저장소를 클론합니다.
2. Unity Hub에서 프로젝트에 명시된 버전으로 Unity를 설치하고 엽니다. (`Docs/Technical/TechnicalPlan/기술기획서.md` 참고)
3. Unity 에디터에서 아래 설정이 적용되어 있는지 확인합니다.
   - `Edit > Project Settings > Editor` → Asset Serialization: **Force Text**, Version Control: **Visible Meta Files**
4. 처음 합류했다면 본인 역할에 맞는 문서부터 읽습니다. (`Docs/Production/DivisionOfRoles/역할분담_문서.md` 참고)

---

## 🗂 저장소 구조 (요약)

```
BasePlatformer/
 ├─ Assets/        # Unity 프로젝트 본체
 ├─ Docs/          # 기획/기술/아트/운영 문서 (위 목록 참고)
 ├─ ProjectSettings/
 └─ README.md
```
