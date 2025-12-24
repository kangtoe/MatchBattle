# 조우 데이터 기획 문서

**프로젝트**: MatchBattle
**버전**: 1.0
**작성일**: 2025-12-24
**관련 문서**:
- [MapSystem_Design.md](MapSystem_Design.md) - 맵 시스템 (조우 풀 시스템 개념)
- [CombatSystem_Design.md](CombatSystem_Design.md) - 전투 시스템

---

## 📋 목차

1. [개요](#-개요)
2. [적 데이터 참조](#-적-데이터-참조)
3. [Stage 1 조우](#-stage-1-조우)
4. [Stage 2 조우](#-stage-2-조우)
5. [Stage 3 조우](#-stage-3-조우)
6. [Stage 4 조우](#-stage-4-조우)
7. [Stage 5 조우](#-stage-5-조우)
8. [Stage 6 조우](#-stage-6-조우)
9. [Stage 7 조우 (보스)](#-stage-7-조우-보스)

---

## 📖 개요

이 문서는 **전체 스테이지(Stage 1-7)의 조우 데이터**를 정의합니다.

### 조우 구조

```
각 조우 = 4개의 적 슬롯 (Slot 0-3)
- Slot 0: 전방 왼쪽
- Slot 1: 중앙 왼쪽
- Slot 2: 중앙 오른쪽
- Slot 3: 후방 오른쪽
- null: 빈 슬롯
```

### 조우 풀 구성

```
각 스테이지 = Normal Pool + Elite Pool (Stage 4+)
- Normal Pool: 일반 전투 조우 3-5개
- Elite Pool: 엘리트 전투 조우 2-3개 (Stage 4 이후)
```

---

## 👾 적 데이터 참조

### 기본 적

| 적 이름 | ID | HP | 공격력 | 난이도 |
|--------|----|----|--------|--------|
| 슬라임 | Slime | 30 | 6 | ★☆☆ |
| 고블린 | Goblin | 40 | 8 | ★★☆ |
| 오크 전사 | OrcWarrior | 60 | 12 | ★★★ |
| 오크 궁수 | OrcArcher | 45 | 10 | ★★☆ |

### 엘리트 적

| 적 이름 | ID | HP | 공격력 | 난이도 |
|--------|----|----|--------|--------|
| 슬라임 킹 | SlimeKing | 80 | 10 | ★★☆ |
| 고블린 샤먼 | GoblinShaman | 70 | 12 | ★★★ |
| 오크 족장 | OrcChieftain | 100 | 18 | ★★★★ |

### 보스

| 적 이름 | ID | HP | 공격력 | 난이도 |
|--------|----|----|--------|--------|
| 오크 로드 | OrcLord | 150 | 20 | ★★★★★ |

---

## 🎮 Stage 1 조우

**스테이지 레벨**: 1
**난이도**: 튜토리얼
**목표**: 기본 전투 학습

### Normal Pool (Stage 1)

#### Encounter 1-1: 슬라임 단독

```
encounterName: "슬라임 단독"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│  -   │  -   │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: null
enemySlot2: null
enemySlot3: null
```

#### Encounter 1-2: 슬라임 2마리

```
encounterName: "슬라임 듀오"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│  -   │  -   │ 슬라임│
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: null
enemySlot2: null
enemySlot3: Slime
```

#### Encounter 1-3: 고블린 단독

```
encounterName: "고블린 정찰병"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │ 고블린│  -   │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: Goblin
enemySlot2: null
enemySlot3: null
```

---

## 🎮 Stage 2 조우

**스테이지 레벨**: 2
**난이도**: 쉬움
**목표**: 다수 적 대응 학습

### Normal Pool (Stage 2)

#### Encounter 2-1: 고블린 2마리

```
encounterName: "고블린 듀오"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│  -   │  -   │ 고블린│
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: null
enemySlot2: null
enemySlot3: Goblin
```

#### Encounter 2-2: 슬라임 3마리

```
encounterName: "슬라임 무리"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│ 슬라임│  -   │ 슬라임│
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: Slime
enemySlot2: null
enemySlot3: Slime
```

#### Encounter 2-3: 고블린 + 슬라임

```
encounterName: "혼합 부대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│ 고블린│  -   │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: Goblin
enemySlot2: null
enemySlot3: null
```

#### Encounter 2-4: 고블린 3마리

```
encounterName: "고블린 소대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│  -   │ 고블린│ 고블린│
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: null
enemySlot2: Goblin
enemySlot3: Goblin
```

---

## 🎮 Stage 3 조우

**스테이지 레벨**: 3
**난이도**: 보통
**목표**: 전략적 타겟팅 필요

### Normal Pool (Stage 3)

#### Encounter 3-1: 오크 단독

```
encounterName: "오크 전사"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │ 오크 │  -   │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: OrcWarrior
enemySlot2: null
enemySlot3: null
```

#### Encounter 3-2: 오크 + 고블린

```
encounterName: "오크와 부하"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│ 오크 │  -   │ 고블린│
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: OrcWarrior
enemySlot2: null
enemySlot3: Goblin
```

#### Encounter 3-3: 고블린 4마리

```
encounterName: "고블린 전선"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│ 고블린│ 고블린│ 고블린│
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: Goblin
enemySlot2: Goblin
enemySlot3: Goblin
```

#### Encounter 3-4: 오크 궁수 + 슬라임

```
encounterName: "궁수와 방패"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│  -   │  -   │ 궁수 │
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: null
enemySlot2: null
enemySlot3: OrcArcher
```

---

## 🎮 Stage 4 조우

**스테이지 레벨**: 4
**난이도**: 어려움
**목표**: 엘리트 조우 등장

### Normal Pool (Stage 4)

#### Encounter 4-1: 오크 2마리

```
encounterName: "오크 듀오"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │  -   │  -   │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: null
enemySlot2: null
enemySlot3: OrcWarrior
```

#### Encounter 4-2: 오크 + 궁수 + 고블린

```
encounterName: "혼성 부대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│ 오크 │ 궁수 │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: OrcWarrior
enemySlot2: OrcArcher
enemySlot3: null
```

#### Encounter 4-3: 궁수 2마리 + 슬라임

```
encounterName: "궁병 사격조"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│ 궁수 │  -   │ 궁수 │
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: OrcArcher
enemySlot2: null
enemySlot3: OrcArcher
```

### Elite Pool (Stage 4)

#### Encounter 4-E1: 슬라임 킹

```
encounterName: "슬라임 킹"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │슬라임킹│  -   │  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: SlimeKing
enemySlot2: null
enemySlot3: null
```

#### Encounter 4-E2: 고블린 샤먼 + 고블린

```
encounterName: "샤먼과 신도들"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 고블린│ 샤먼 │ 샤먼 │ 고블린│
└──────┴──────┴──────┴──────┘

enemySlot0: Goblin
enemySlot1: GoblinShaman
enemySlot2: GoblinShaman
enemySlot3: Goblin
```

---

## 🎮 Stage 5 조우

**스테이지 레벨**: 5
**난이도**: 매우 어려움
**목표**: 고급 전략 요구

### Normal Pool (Stage 5)

#### Encounter 5-1: 오크 3마리

```
encounterName: "오크 전선"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │ 오크 │  -   │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: OrcWarrior
enemySlot2: null
enemySlot3: OrcWarrior
```

#### Encounter 5-2: 오크 + 궁수 2마리

```
encounterName: "전사와 궁병"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 궁수 │ 오크 │ 오크 │ 궁수 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcArcher
enemySlot1: OrcWarrior
enemySlot2: OrcWarrior
enemySlot3: OrcArcher
```

#### Encounter 5-3: 오크 4마리

```
encounterName: "오크 돌격대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │ 오크 │ 오크 │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: OrcWarrior
enemySlot2: OrcWarrior
enemySlot3: OrcWarrior
```

### Elite Pool (Stage 5)

#### Encounter 5-E1: 슬라임 킹 + 슬라임

```
encounterName: "슬라임 왕국"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 슬라임│슬라임킹│슬라임킹│ 슬라임│
└──────┴──────┴──────┴──────┘

enemySlot0: Slime
enemySlot1: SlimeKing
enemySlot2: SlimeKing
enemySlot3: Slime
```

#### Encounter 5-E2: 오크 족장 + 오크

```
encounterName: "족장과 전사들"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │ 족장 │  -   │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: OrcChieftain
enemySlot2: null
enemySlot3: OrcWarrior
```

#### Encounter 5-E3: 고블린 샤먼 3마리

```
encounterName: "샤먼 의회"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │ 샤먼 │ 샤먼 │ 샤먼 │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: GoblinShaman
enemySlot2: GoblinShaman
enemySlot3: GoblinShaman
```

---

## 🎮 Stage 6 조우

**스테이지 레벨**: 6
**난이도**: 극악
**목표**: 보스 직전 최종 관문

### Normal Pool (Stage 6)

#### Encounter 6-1: 궁수 4마리

```
encounterName: "궁병 부대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 궁수 │ 궁수 │ 궁수 │ 궁수 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcArcher
enemySlot1: OrcArcher
enemySlot2: OrcArcher
enemySlot3: OrcArcher
```

#### Encounter 6-2: 오크 2마리 + 궁수 2마리

```
encounterName: "완전 편제"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │ 궁수 │ 궁수 │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: OrcArcher
enemySlot2: OrcArcher
enemySlot3: OrcWarrior
```

### Elite Pool (Stage 6)

#### Encounter 6-E1: 오크 족장 + 궁수 2마리

```
encounterName: "족장의 호위대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 궁수 │ 족장 │ 족장 │ 궁수 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcArcher
enemySlot1: OrcChieftain
enemySlot2: OrcChieftain
enemySlot3: OrcArcher
```

#### Encounter 6-E2: 족장 + 샤먼 + 오크

```
encounterName: "최강 연합"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │ 족장 │ 샤먼 │ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: OrcChieftain
enemySlot2: GoblinShaman
enemySlot3: OrcWarrior
```

---

## 🎮 Stage 7 조우 (보스)

**스테이지 레벨**: 7
**난이도**: 보스
**목표**: 런 종료 보스전

### Boss Pool (Stage 7)

#### Encounter 7-B1: 오크 로드 단독

```
encounterName: "오크 로드"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │  -   │오크로드│  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: null
enemySlot2: OrcLord
enemySlot3: null
```

#### Encounter 7-B2: 오크 로드 + 족장

```
encounterName: "로드와 족장"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│  -   │ 족장 │오크로드│  -   │
└──────┴──────┴──────┴──────┘

enemySlot0: null
enemySlot1: OrcChieftain
enemySlot2: OrcLord
enemySlot3: null
```

#### Encounter 7-B3: 오크 로드 + 호위대

```
encounterName: "로드와 호위대"

┌──────┬──────┬──────┬──────┐
│ 슬롯0 │ 슬롯1 │ 슬롯2 │ 슬롯3 │
├──────┼──────┼──────┼──────┤
│ 오크 │  -   │오크로드│ 오크 │
└──────┴──────┴──────┴──────┘

enemySlot0: OrcWarrior
enemySlot1: null
enemySlot2: OrcLord
enemySlot3: OrcWarrior
```

---

**작성일**: 2025-12-24
**버전**: 1.0
**담당**: 게임 디자인
