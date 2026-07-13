# 에셋 출처 / 라이선스 관리

## 0. 문서 목적

이 문서는 외부에서 가져오는 그림, 사운드, 폰트 등의 에셋을 **어디서 구할지, 그리고 가져온 에셋의 출처/라이선스를 어떻게 기록할지** 정리한 문서입니다.

- 동아리 프로젝트라도 외부 에셋을 무단으로 사용하면 추후 배포/공개 시 저작권 문제가 생길 수 있어, 미리 규칙을 정해둡니다.
- `에셋_네이밍_관리규칙.md`(파일명/폴더 규칙), `사운드_큐_리스트.md`(어떤 사운드가 필요한지)와 함께 사용하는 문서입니다.
- **ver2 변경 사항**: 실제로 사용 중인 CraftPix `SimplePlatformer2D` 에셋을 4장 기록 표에 추가.
- **2026-07-09 참고**: 정령/몬스터/신규 발판용 에셋은 아직 출처가 확정되지 않았습니다. 확보되는 대로 아래 4장 표에 행을 추가합니다 (`에셋_네이밍_관리규칙.md`의 `spirit_`, `monster_` 접두사 규칙 적용).

---

## 1. 기본 원칙

1. **출처가 불분명한 에셋은 사용하지 않습니다.** (어디서 받았는지 기억 안 남 상태의 파일은 추가하지 않기)
2. **라이선스 조건(출처 표기 필요 여부, 상업적 이용 가능 여부 등)을 반드시 확인하고 기록합니다.**
3. **저작권이 있는 캐릭터/로고/상표는 절대 사용하지 않습니다.**
4. 직접 제작한 에셋은 원칙적으로 문제 없지만, **누가 제작했는지는 기록**해둡니다.

---

## 2. 추천 에셋 소스 (무료/오픈 라이선스 중심)

| 종류 | 사이트 예시 | 비고 |
|---|---|---|
| 스프라이트/타일셋 | Kenney.nl, OpenGameArt.org, itch.io, **CraftPix.net** | CC0 에셋이 많음. CraftPix 무료 에셋은 상업적 사용 가능 |
| 사운드 효과음(SFX) | Freesound.org, Kenney.nl | Freesound는 에셋마다 라이선스가 다르므로 개별 확인 필요 |
| 배경음악(BGM) | OpenGameArt.org, itch.io, Incompetech | 출처 표기(CC-BY) 조건이 붙는 경우가 많음 |
| 폰트 | Google Fonts, itch.io | 상업적 이용 가능 여부 확인 |

---

## 3. 라이선스 종류 간단 정리

| 라이선스 | 의미 | 출처 표기 필요? |
|---|---|---|
| CC0 / Public Domain | 저작권 없음, 자유 사용 | 불필요 (그래도 기록은 해두는 게 안전) |
| CC-BY | 자유 사용 가능, 단 제작자 출처 표기 필요 | 필요 |
| CC-BY-SA | 출처 표기 + 동일 라이선스로 재배포 필요 | 필요 (조건 더 엄격) |
| **CraftPix Royalty-Free** | 무제한 프로젝트 사용 가능, 상업적 이용 및 게임 판매/배포 가능 | 불필요 (권장은 함) |
| All Rights Reserved | 사용 불가 (별도 허락 없이) | 사용 금지 |

---

## 4. 에셋 출처 기록 표

### 현재 사용 중인 외부 에셋

| 에셋명 | 제작자/출처 | 다운로드 링크 | 라이선스 | 출처 표기 필요 | Unity 내 경로 | 비고 |
|---|---|---|---|---|---|---|
| Free Simple Platformer Game Kit Pixel Art | CraftPix.net | https://craftpix.net/freebies/free-simple-platformer-game-kit-pixel-art/ | CraftPix Royalty-Free (무료) | 불필요 (권장) | `Assets/CraftPix/SimplePlatformer2D/` | 캐릭터, 타일, 트랩, 체크포인트 등 포함 |

> 정령/몬스터/신규 발판(이동·무너짐·스탯변화) 에셋은 아직 이 표에 없습니다 — 소스가 정해지는 대로 행을 추가합니다.

### 사용 중인 구체적인 에셋 목록 (SimplePlatformer2D 내)

| 용도 | 파일 경로 |
|---|---|
| 캐릭터 대기 | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Idle` |
| 캐릭터 이동 | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Run` |
| 캐릭터 점프 상승 | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Jump` |
| 캐릭터 낙하 | `Assets/CraftPix/SimplePlatformer2D/1 Main Characters/2/Fall` |
| 발판 (위쪽 왼쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_01` |
| 발판 (위쪽 중간) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_02` |
| 발판 (위쪽 오른쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_03` |
| 발판 (중간 왼쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_15` |
| 발판 (중간 중간) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_16` |
| 발판 (중간 오른쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_17` |
| 발판 (아래쪽 왼쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_29` |
| 발판 (아래쪽 중간) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_30` |
| 발판 (아래쪽 오른쪽) | `Assets/CraftPix/SimplePlatformer2D/2 Locations/Tiles/Tile_31` |
| 즉사 장애물 (가시) | `Assets/CraftPix/SimplePlatformer2D/6 Traps/4_2` |
| 도착 지점 | `Assets/CraftPix/SimplePlatformer2D/3 Objects/Checkpoints/End_Idle` |

> 위 경로는 `기술기획서.md` 4.7절과 동일합니다.

---

## 5. CraftPix 라이선스 요약

CraftPix.net의 무료(Freebies) 에셋은 아래 조건으로 사용할 수 있습니다.

- 개인 및 **상업적 프로젝트** 모두 사용 가능
- **무제한 프로젝트**에 사용 가능
- 해당 에셋을 사용한 **게임 판매 및 배포** 가능
- 출처 표기 의무는 없으나, 권장됨
- 에셋 자체를 단독으로 재판매하는 것은 불가

> 라이선스 상세: https://craftpix.net/file-licenses/ (원문 확인 권장)

---

## 6. 출처 표기가 필요한 경우, 어디에 표기할까

- CC-BY 등 출처 표기가 필요한 에셋이 있다면, 게임 내 **크레딧 화면**(추후 확장 기능 단계) 또는 README 하단에 모아서 표기합니다.
- CraftPix 에셋은 표기 의무는 없지만, 크레딧 화면에 포함하는 것을 권장합니다.

---

## 7. 추후 에셋 추가 시 워크플로우

1. `사운드_큐_리스트.md` 또는 `아트스타일_가이드.md`(9장 확장 아트 가이드 포함)를 참고해 어떤 에셋이 필요한지 확인
2. 위 2장의 추천 소스(또는 직접 제작)에서 에셋 확보
3. 라이선스 확인 후 4장의 표에 기록
4. `에셋_네이밍_관리규칙.md` 규칙(정령은 `spirit_`, 몬스터는 `monster_` 접두사 포함)에 맞게 파일명 변경 후 해당 폴더에 저장
5. 출처 표기가 필요한 에셋이면 추후 크레딧 화면에 포함될 수 있도록 별도 표시

> 정령·몬스터·신규 발판 에셋도 이 워크플로우를 동일하게 따릅니다.
