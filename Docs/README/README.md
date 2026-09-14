# BasePlatformer

## 🎮 게임 소개

| 항목 | 내용 |
|---|---|
| 게임명 | (가제) |
| 장르 | 2D 사이드스크롤 플랫포머 |
| 한 줄 소개 | 목숨을 구해준 정령의 부탁으로, 숲을 오염시킨 원흉을 찾아 마을 → 변방 → 숲으로 나아가는 이야기 |
| 핵심 플레이 | 플레이어는 **이동·점프만** 가능, 공격과 발판 생성은 마우스로 대상을 가리켜 **정령이 대신 수행** |
| 스테이지 구조 | 테마 1(마을·튜토리얼) → 테마 2(변방, 추가 정령 등장) → 테마 3(숲, 최종 보스) |
| 정령 구성 | 기본 정령 1개(공격+발판 겸용) + 추가 정령 6종(공격: 폭발/화상/빙결, 발판: 점프력강화/이속증가/장막제공) 중 3개 선택 |
| 클리어 조건 | 각 스테이지 도착 지점 도달 → 최종 보스(밤 나무) 처치 |

> 스토리 전체 내용은 [`스토리.md`](Docs/Design/Story/스토리.md), 정령 시스템 상세는 [`정령_시스템_기획서.md`](Docs/Design/Spirits/정령_시스템_기획서.md)를 참고하세요.

## 프로젝트 정보

- **엔진**: Unity
- **협업 툴**: GitHub
- **현재 상태**: 핵심 기능(MVP) **완료** + 확장 기능(정령 시스템·몬스터 7종(보스 포함)·발판 4종·체력 시스템) **대부분 구현 완료**. 스테이지 1~3(마을/변방/숲) 배치, 스테이지 간 전환, 시작 화면 UI까지 구현되어 처음부터 끝까지 플레이 가능한 상태. 현재는 `스탯_기획서.md` 확정 수치와 실제 데이터 에셋 값을 맞추는 **수치 정합 작업**, 남은 버그(`QA_테스트_계획서.md` 10장) 해결이 주요 과제

---

## 📂 문서 목록

모든 기획/기술 문서는 `Docs/` 폴더 아래에 역할별로 정리되어 있습니다. 각 문서는 항상 최신 내용을 담은 단일 파일이며, 과거 변경 이력은 GitHub 커밋 히스토리로 확인할 수 있습니다.

### Design (기획)
- [핵심 기능 기획서 (MVP)](Docs/Design/Core/플랫포머_게임_기획서.md) — 반드시 구현해야 하는 핵심 기능 정의
- [확장 기능 기획서](Docs/Design/PlusContents/플랫포머_게임_기획서_확장기능.md) — 확장 기능 후보/채택 현황
- [레벨 디자인 문서](Docs/Design/LevelDesign/레벨디자인_MVP_스테이지.md) — MVP 스테이지의 구간별 레이아웃
- [캐릭터 컨트롤 스펙](Docs/Design/CharacterControls/캐릭터컨트롤_스펙.md) — 이동/점프/코요테 타임 수치, 조작키
- [사운드 큐 리스트](Docs/Design/SoundQueue/사운드_큐_리스트.md) — 어떤 상황에 어떤 사운드가 필요한지 정리

### Design — 확장 설계 (정령 시스템)
- [정령 시스템 기획서](Docs/Design/Spirits/정령_시스템_기획서.md) — 기본/추가 정령 종류, 상호작용 방식, 세계관
- [몬스터 기획서](Docs/Design/Monsters/몬스터_기획서.md) — 몬스터 분류, 테마별 배치
- [발판 확장 기획서](Docs/Design/Platforms/발판_확장_기획서.md) — 이동/무너짐/스탯변화 발판
- [스탯 기획서](Docs/Design/Stats/스탯_기획서.md) — 플레이어/정령/몬스터 스탯 항목
- [월드/스테이지 구성 기획서](Docs/Design/WorldStages/월드_스테이지_구성.md) — 테마, 스토리 개요, 클리어/게임오버 조건
- [스토리](Docs/Design/Story/스토리.md) — 로그라인, 테마별 스토리 비트, 정령·보스의 서사적 의미, 엔딩 개요

### Technical (기술)
- [기술 기획서 (Unity)](Docs/Technical/TechnicalPlan/기술기획서.md) — 엔진 구현 방식, GitHub 협업 규칙
- [에셋 네이밍/관리 규칙](Docs/Technical/AssetNaming/에셋_네이밍_관리규칙.md) — 파일명, 폴더 구조 규칙
- [에셋 출처/라이선스 관리](Docs/Technical/License/에셋_출처_라이선스_관리.md) — 외부 에셋 소스, 라이선스 기록표
- [스크립트 설명서](Docs/Technical/ScriptDocs/스크립트_설명서.md) — 각 스크립트의 역할과 동작 방식 설명

### Art (아트)
- [아트 스타일 가이드](Docs/Art/ArtStyle/아트스타일_가이드.md) — 색상, 해상도, 캐릭터/지형 디자인 기준

### Production (운영)
- [일정/마일스톤](Docs/Production/Milestone/일정_마일스톤.md) — 핵심 기능 제작 일정
- [역할 분담 문서](Docs/Production/DivisionOfRoles/역할분담_문서.md) — PM/프로그래밍/기획/아트 역할별 가이드
- [QA/테스트 계획서](Docs/Production/QATest/QA_테스트_계획서.md) — 테스트 케이스, 버그 리포트 양식
- [ToDoList](Docs/Production/ToDoList/ToDoList.md) — 현재 해야 할 일을 단계별/역할별로 모아 추적
- [문서 작성 가이드라인](Docs/Production/DocGuideline/문서작성_가이드라인.md) — 문서 폴더 구조, 네이밍 규칙 (PM 소유)

---

## 📌 새 기능 기획 전 참고 문서

아래 문서들은 회장(PM)이 **혼자 구현한 MVP(핵심 기능) 단계**의 산출물입니다.
동아리 부원들과 새로운 기능을 논의하기 전에, 먼저 아래 문서를 읽고 기존에 어떤 수치/구조가 이미 정해져 있는지 파악해 주세요.

- [게임 기획서](Docs/Design/Core/플랫포머_게임_기획서.md) — 전체 게임 방향성과 핵심 기능 정의
- [캐릭터컨트롤 스펙](Docs/Design/CharacterControls/캐릭터컨트롤_스펙.md) — 이동/점프/코요테 타임 등 기존 조작 수치
- [레벨디자인 문서](Docs/Design/LevelDesign/레벨디자인_MVP_스테이지.md) — 기존 스테이지 구조
- [기술기획서](Docs/Technical/TechnicalPlan/기술기획서.md) — 기존 코드/폴더 구조

정령 시스템을 포함한 확장 단계 설계는 이미 위 [확장 설계(정령 시스템)](#design--확장-설계-정령-시스템) 문서들로 정리되어 있으니, 확장 기능 작업에 합류하는 부원은 해당 문서부터 확인해 주세요.

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
 ├─ Docs/          # 기획/기술/아트/운영 문서
 │   └─ {분류}/{세부주제}/
 │       └─ {문서명}.md       # 최신 내용 (위 목록이 가리키는 파일, 변경 이력은 GitHub 커밋으로 확인)
 ├─ ProjectSettings/
 └─ README.md
```
