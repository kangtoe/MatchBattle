# 전투 플로우 상세 설계

**목적**: 전투 시스템의 모든 단계를 기획 → 기술 레벨로 구체화
**관련 문서**: [CombatSystem_Design.md](CombatSystem_Design.md), [EnemyDesign.md](EnemyDesign.md)

---

## 📋 목차

1. [전투 플로우 개요](#-전투-플로우-개요)
2. [Phase별 상세 설계](#-phase별-상세-설계)
3. [상태 효과 처리 타이밍](#-상태-효과-처리-타이밍)
4. [기술적 구현 매핑](#-기술적-구현-매핑)
5. [승리/패배 처리](#-승리패배-처리)
6. [진행 시스템](#-진행-시스템)

---

## 🎮 전투 플로우 개요

### 전체 흐름도

```
┌─────────────────────────────────────────────────────────┐
│                    [게임 시작]                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│               [전투 초기화 Phase]                        │
│  - 적 생성                                               │
│  - 플레이어 상태 확인                                     │
│  - 보드 생성                                             │
│  - UI 초기화                                             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│              [전투 시작 Phase]                           │
│  - 적 첫 행동 결정                                        │
│  - 적 행동 예고 표시                                      │
│  - 플레이어 턴 시작                                       │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
         ┌───────────────────────┐
         │                       │
         │   [턴 사이클 반복]    │ ← ─ ─ ─ ─ ─ ┐
         │                       │              │
         └───────┬───────────────┘              │
                 │                              │
                 ↓                              │
    ┌─────────────────────────┐                │
    │   [플레이어 턴 Phase]    │                │
    │  - 상태 효과 처리        │                │
    │  - 보드 입력 활성화      │                │
    │  - 블록 매치 대기        │                │
    │  - 효과 적용             │                │
    └───────┬─────────────────┘                │
            │                                  │
            ↓                                  │
    ┌─────────────────────────┐                │
    │    [적 턴 Phase]         │                │
    │  - 상태 효과 처리        │                │
    │  - 예고된 행동 실행      │                │
    │  - 다음 행동 결정        │                │
    │  - 다음 행동 예고        │                │
    └───────┬─────────────────┘                │
            │                                  │
            ↓                                  │
    ┌─────────────────────────┐                │
    │   [승패 판정 Phase]      │                │
    │  - 적 HP 확인            │                │
    │  - 플레이어 HP 확인      │                │
    └───────┬─────────────────┘                │
            │                                  │
            ├─ 전투 계속 ─────────────────────┘
            │
            ↓
    ┌─────────────────────────┐
    │   [전투 종료 Phase]      │
    │  - 승리 or 패배          │
    └───────┬─────────────────┘
            │
            ↓
    ┌─────────────────────────┐
    │   [승리] → 보상 선택     │
    │   [패배] → 게임 오버     │
    └─────────────────────────┘
```

---

## 🔄 Phase별 상세 설계

### Phase 1: 전투 초기화 (Combat Initialization)

**기획 의도**:
- 전투에 필요한 모든 요소를 준비
- 플레이어와 적의 초기 상태 설정
- UI와 보드를 전투 모드로 전환

**구현 단계**:

1. 상태 초기화 (turnCount, currentState, currentEnemy)
2. 플레이어 상태 확인 (HP, Defense, Gold 현재값 유지)
3. 적 생성 완료 확인 (HP, 상태 효과, 행동 풀)
4. 보드 생성 (초기 블록 배치)
5. UI 초기화 (플레이어/적 스탯 표시, 결과 패널 숨김)
6. 적 첫 행동 결정
7. 적 행동 예고 UI 표시
8. 플레이어 턴 시작

**주요 결정 사항**:
- ✅ 플레이어 Defense는 전투 간 유지됨 (이전 전투에서 쌓은 방어력 보존)
- ✅ 적은 매번 새로 생성 (깨끗한 상태)
- ✅ 보드는 전투마다 새로 생성 (랜덤 블록 배치)

---

### Phase 2: 플레이어 턴 (Player Turn)

**턴 시작 시퀀스**:

```
[플레이어 턴 시작]
    ↓
1. 턴 카운트 증가
    ↓
2. 상태 효과 처리 (Turn Start)
   - REGEN 발동 → HP 회복
   - POISON 발동 → HP 데미지
   - Duration 효과 지속시간 -1
    ↓
3. 보드 입력 활성화
   - BoardInputHandler 활성화
   - 플레이어가 블록 연결 가능
    ↓
4. 플레이어 블록 연결 대기
   - 드래그 입력 처리
   - 경로 시각화
    ↓
5. 경로 완료 (OnPathCompleted 이벤트)
   - 블록 개수 확인
   - 효과 배수 계산 (3개 이상: 100%, 1-2개: 50%)
    ↓
6. 블록 색상별 효과 적용
   - Red → 적 공격
   - Blue → 방어력 추가
   - Yellow → 골드 획득
   - Brown → HP 회복
   - Purple → 와일드카드 (임시: 공격)
    ↓
7. 보드 업데이트
   - 사용된 블록 제거
   - 새 블록 생성 및 드롭
    ↓
8. UI 업데이트
   - 데미지/방어력/회복 팝업 표시
   - 플레이어/적 스탯 갱신
    ↓
9. 상태 효과 처리 (Turn End)
   - EXHAUSTED 발동 (있을 경우)
   - Duration 효과 지속시간 -1
    ↓
10. 턴 종료
    → 적 턴으로 전환
```

**타이밍 다이어그램**:

```
Time  │  Player Turn Events
──────┼────────────────────────────────────────────
 0.0s │  StartPlayerTurn()
      │  ├─ State: PlayerTurn
      │  ├─ turnCount++
      │  └─ Process player status effects (Turn Start)
      │
 0.1s │  Board input enabled
      │
  ... │  [User drags blocks...]
      │
 5.2s │  OnPathCompleted event
      │  ├─ Validate path (color, count)
      │  ├─ Calculate multiplier
      │  └─ ApplyBlockEffect()
      │      ├─ Update enemy/player stats
      │      └─ Show UI popups
      │
 5.3s │  Board update
      │  ├─ Remove matched blocks
      │  └─ Drop new blocks
      │
 5.5s │  Process player status effects (Turn End)
      │  └─ EXHAUSTED, Duration effects
      │
 5.6s │  Check victory condition
      │  └─ If enemy alive → EndPlayerTurn()
      │
 5.7s │  Transition to Enemy Turn
```

---

### Phase 3: 적 턴 (Enemy Turn)

**턴 시작 시퀀스**:

```
[적 턴 시작]
    ↓
1. 상태 효과 처리 (Turn Start)
   - REGEN 발동 → HP 회복
   - POISON 발동 → HP 데미지
   - Duration 효과 지속시간 -1
    ↓
2. 예고된 행동 실행
   - nextAction 가져오기
   - 행동 타입별 처리
    ↓
3. 행동별 효과 적용
   - Attack → 플레이어 공격
   - HeavyAttack → 플레이어 강공격
   - Defend → ARMOR 추가
   - Buff → 상태 효과 추가
   - Debuff → 플레이어 디버프
    ↓
4. UI 업데이트
   - 데미지/방어력 팝업 표시
   - 플레이어/적 스탯 갱신
    ↓
5. 상태 효과 처리 (Turn End)
   - EXHAUSTED 발동 (있을 경우)
   - Duration 효과 지속시간 -1
    ↓
6. 다음 행동 결정
   - SelectNextAction() (가중치 기반 랜덤)
    ↓
7. 다음 행동 예고 UI 표시
   - ShowEnemyIntent()
    ↓
8. 승패 판정
   - 플레이어 HP 확인
   - 적 HP 확인
    ↓
9. 턴 종료
   - 승리 → HandleVictory()
   - 패배 → HandleDefeat()
   - 계속 → 플레이어 턴으로 전환
```

**타이밍 다이어그램**:

```
Time  │  Enemy Turn Events
──────┼────────────────────────────────────────────
 0.0s │  StartEnemyTurn()
      │  ├─ State: EnemyTurn
      │  └─ currentEnemy.ProcessTurnStart()
      │      ├─ REGEN → Heal()
      │      ├─ POISON → TakeDamage()
      │      └─ Duration effects -1
      │
 0.5s │  ExecuteEnemyAction(nextAction)
      │  ├─ Attack → DealDamageToPlayer()
      │  │   ├─ player.TakeDamage()
      │  │   │   ├─ Defense reduction
      │  │   │   └─ HP reduction
      │  │   └─ combatUI.ShowDamage()
      │  │
      │  ├─ Defend → currentEnemy.AddARMOR()
      │  └─ Buff → currentEnemy.AddSTR()
      │
 1.0s │  currentEnemy.ProcessTurnEnd()
      │  ├─ EXHAUSTED → AddSTR(-value)
      │  └─ Duration effects -1
      │
 1.5s │  SelectNextAction()
      │  └─ Weighted random from actionPool
      │
 2.0s │  ShowEnemyIntent(nextAction)
      │  └─ Display next turn action preview
      │
 2.5s │  Check victory/defeat
      │  ├─ player.CurrentHP <= 0 → HandleDefeat()
      │  ├─ enemy.CurrentHP <= 0 → HandleVictory()
      │  └─ else → StartPlayerTurn()
```

---

## ⏱️ 상태 효과 처리 타이밍

### 타이밍 개요

```
┌─────────────────────────────────────────────────────────┐
│                   Turn N (Player Turn)                   │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  [Turn Start]                                            │
│   └─ Player.ProcessTurnStart()                           │
│      ├─ REGEN: HP 회복                                   │
│      ├─ POISON: HP 데미지                                │
│      └─ WEAK/VULNERABLE: duration -1                     │
│                                                          │
│  [Player Action]                                         │
│   └─ 블록 매치 → 효과 적용                                │
│                                                          │
│  [Turn End]                                              │
│   └─ Player.ProcessTurnEnd()                             │
│      ├─ EXHAUSTED: STR 감소                              │
│      └─ WEAK/VULNERABLE: duration -1                     │
│                                                          │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                   Turn N (Enemy Turn)                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  [Turn Start]                                            │
│   └─ Enemy.ProcessTurnStart()                            │
│      ├─ REGEN: HP 회복                                   │
│      ├─ POISON: HP 데미지                                │
│      └─ WEAK/VULNERABLE: duration -1                     │
│                                                          │
│  [Enemy Action]                                          │
│   └─ 예고된 행동 실행 → 효과 적용                         │
│                                                          │
│  [Turn End]                                              │
│   └─ Enemy.ProcessTurnEnd()                              │
│      ├─ EXHAUSTED: STR 감소                              │
│      └─ WEAK/VULNERABLE: duration -1                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 상태 효과별 처리 위치

| 상태 효과 | 카테고리 | Turn Start | Turn End | 특징 |
|-----------|----------|------------|----------|------|
| **STR** | Stack | - | - | 영구, 공격력 증가 |
| **ARMOR** | Stack | - | - | 영구, 데미지 감소 |
| **REGEN** | Decrement | ✅ HP 회복, value -1 | - | 턴마다 회복 후 감소 |
| **POISON** | Decrement | ✅ HP 데미지, value -1 | - | 턴마다 피해 후 감소 |
| **WEAK** | Duration | ✅ duration -1 | - | 공격력 감소, 지속시간 감소 |
| **VULNERABLE** | Duration | ✅ duration -1 | - | 받는 데미지 증가, 지속시간 감소 |
| **EXHAUSTED** | Stack | - | ✅ STR 감소, 제거 | 턴 종료 시 힘 회수 |

### MVP 범위 (현재 구현)

**적 (Enemy)**:
- ✅ STR: 영구 공격력 증가
- ✅ ARMOR: 영구 데미지 감소
- ✅ REGEN: 턴 시작마다 HP 회복
- ✅ POISON: 턴 시작마다 HP 데미지
- ✅ WEAK: 공격력 감소 (지속시간)
- ✅ VULNERABLE: 받는 데미지 증가 (지속시간)
- ✅ EXHAUSTED: 턴 종료 시 STR 감소

**플레이어 (Player)**:
- ❌ 상태 효과 미구현 (MVP에서는 단순화)
- ✅ Defense: 고정 방어력 시스템 (상태 효과 아님)

### Post-MVP: 플레이어 상태 효과

플레이어도 동일한 상태 효과 시스템 적용 예정:
- STR: 공격력 증가 (버프 블록)
- REGEN: HP 회복 (포션 블록)
- WEAK: 공격력 감소 (적 디버프)
- VULNERABLE: 받는 데미지 증가 (적 디버프)

---

## 🛠️ 기술적 구현 매핑

### 클래스별 역할

```
┌─────────────────────────────────────────────────────────┐
│                      CombatManager                       │
│  - 전투 전체 흐름 관리                                    │
│  - 턴 순서 제어                                          │
│  - 승패 판정                                             │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   StartCombat(Enemy)       : 전투 초기화                  │
│   StartPlayerTurn()        : 플레이어 턴 시작             │
│   EndPlayerTurn()          : 플레이어 턴 종료             │
│   StartEnemyTurn()         : 적 턴 시작                  │
│   ExecuteEnemyAction()     : 적 행동 실행                │
│   HandleVictory()          : 승리 처리                   │
│   HandleDefeat()           : 패배 처리                   │
│   HandlePathCompleted()    : 보드 입력 처리              │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                      BoardManager                        │
│  - 보드 생성 및 관리                                      │
│  - 블록 매칭 로직                                         │
│  - 블록 드롭 및 리필                                      │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   GenerateBoard()          : 보드 생성                   │
│   RemoveBlocks()           : 매칭된 블록 제거             │
│   DropBlocks()             : 블록 드롭                   │
│   RefillBoard()            : 새 블록 생성                │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                   BoardInputHandler                      │
│  - 플레이어 입력 처리                                     │
│  - 드래그 경로 추적                                       │
│  - 경로 유효성 검사                                       │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   OnBlockDragStart()       : 드래그 시작                 │
│   OnBlockDragMove()        : 드래그 이동                 │
│   OnBlockDragEnd()         : 드래그 종료                 │
│   ValidatePath()           : 경로 유효성 확인             │
│                                                          │
│  이벤트:                                                  │
│   OnPathCompleted          : 경로 완료 이벤트             │
│     → CombatManager.HandlePathCompleted() 호출           │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                        Player                            │
│  - 플레이어 스탯 관리                                     │
│  - 데미지/회복 처리                                       │
│  - Defense 시스템                                        │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   TakeDamage(int)          : 데미지 받기                 │
│   Heal(int)                : HP 회복                     │
│   AddDefense(int)          : 방어력 추가 (최대 30)        │
│   AddGold(int)             : 골드 획득                   │
│                                                          │
│  프로퍼티:                                                │
│   CurrentHP, MaxHP         : 체력                        │
│   Defense, MaxDefense      : 방어력 (최대 30)            │
│   Gold                     : 골드                        │
│                                                          │
│  이벤트:                                                  │
│   OnHPChanged              : HP 변경 → UI 업데이트        │
│   OnDefenseChanged         : Defense 변경 → UI 업데이트   │
│   OnGoldChanged            : Gold 변경 → UI 업데이트      │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                         Enemy                            │
│  - 적 스탯 관리                                           │
│  - 상태 효과 시스템                                       │
│  - AI 행동 선택                                          │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   TakeDamage(int)          : 데미지 받기 (ARMOR 적용)     │
│   Heal(int)                : HP 회복                     │
│   AddSTR(int)              : 힘 추가                     │
│   AddARMOR(int)            : 철갑 추가                   │
│   GetSTR()                 : 현재 힘 조회                │
│   GetARMOR()               : 현재 철갑 조회              │
│   SelectNextAction()       : 다음 행동 선택              │
│   ProcessTurnStart()       : 턴 시작 처리                │
│   ProcessTurnEnd()         : 턴 종료 처리                │
│                                                          │
│  프로퍼티:                                                │
│   CurrentHP, MaxHP         : 체력                        │
│   BaseAttackPower          : 기본 공격력                 │
│   CurrentAttackPower       : 현재 공격력 (기본 + STR)     │
│   nextAction               : 다음 행동                   │
│                                                          │
│  이벤트:                                                  │
│   OnHPChanged              : HP 변경 → UI 업데이트        │
│   OnStatusEffectAdded      : 상태 효과 추가 → UI 업데이트 │
│   OnStatusEffectRemoved    : 상태 효과 제거 → UI 업데이트 │
│   OnDeath                  : 사망 → 승리 처리            │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                       CombatUI                           │
│  - 전투 UI 표시                                          │
│  - 이벤트 기반 업데이트                                   │
│  - 팝업 및 결과 화면                                      │
├─────────────────────────────────────────────────────────┤
│  핵심 메서드:                                             │
│   SetupBattle()            : 전투 UI 초기화              │
│   ShowEnemyIntent()        : 적 행동 예고 표시            │
│   ShowDamage()             : 데미지 팝업                 │
│   ShowHeal()               : 회복 팝업                   │
│   ShowDefenseGain()        : 방어력 팝업                 │
│   ShowVictoryScreen()      : 승리 화면                   │
│   ShowDefeatScreen()       : 패배 화면                   │
│                                                          │
│  이벤트 구독:                                             │
│   Player.OnHPChanged       → UpdatePlayerHP()            │
│   Player.OnDefenseChanged  → UpdatePlayerDefense()       │
│   Player.OnGoldChanged     → UpdatePlayerGold()          │
│   Enemy.OnHPChanged        → UpdateEnemyHP()             │
│   Enemy.OnStatusEffectAdded/Removed → UpdateEnemyArmor() │
└─────────────────────────────────────────────────────────┘
```

### 이벤트 흐름

```
[플레이어가 블록 드래그]
    ↓
BoardInputHandler.OnBlockDragEnd()
    ↓
ValidatePath()
    ↓
OnPathCompleted 이벤트 발생
    │
    └──→ CombatManager.HandlePathCompleted()
           │
           ├─ ApplyBlockEffect()
           │   │
           │   ├─ Red: DealDamage()
           │   │   └──→ Enemy.TakeDamage()
           │   │        └──→ Enemy.OnHPChanged 이벤트
           │   │             └──→ CombatUI.UpdateEnemyHP()
           │   │
           │   ├─ Blue: AddDefense()
           │   │   └──→ Player.AddDefense()
           │   │        └──→ Player.OnDefenseChanged 이벤트
           │   │             └──→ CombatUI.UpdatePlayerDefense()
           │   │
           │   └─ Brown: HealPlayer()
           │       └──→ Player.Heal()
           │            └──→ Player.OnHPChanged 이벤트
           │                 └──→ CombatUI.UpdatePlayerHP()
           │
           └─ EndPlayerTurn()
               └──→ StartEnemyTurn()
                    │
                    ├─ ExecuteEnemyAction()
                    │   └──→ DealDamageToPlayer()
                    │        └──→ Player.TakeDamage()
                    │             └──→ Player.OnHPChanged/OnDefenseChanged
                    │                  └──→ CombatUI.UpdatePlayerHP/Defense()
                    │
                    └─ StartPlayerTurn()
```

---

## 🏆 승리/패배 처리

### 승패 판정 타이밍

```
판정 시점:
1. 플레이어 턴 종료 후 (블록 효과 적용 직후)
2. 적 턴 종료 후 (적 행동 실행 직후)
```

### 승리 시퀀스

```
[적 HP 0 도달]
    ↓
1. 상태 전환
   currentState = CombatState.Victory
    ↓
2. 골드 보상 계산
   goldReward = Mathf.Max(1, enemy.MaxHP / 10)
   player.AddGold(goldReward)
    ↓
3. 승리 화면 표시
   combatUI.ShowVictoryScreen(goldReward)
   - "승리!" 메시지
   - 획득 골드 표시
   - 현재 골드 표시
    ↓
4. 보상 선택 화면 이동 (Post-MVP)
   - 3가지 보상 중 1개 선택
   - 유물, 블록 업그레이드, 골드 등
    ↓
5. 다음 지역 이동 (Post-MVP)
   - 맵 화면으로 복귀
   - 다음 전투 or 이벤트 선택
```

### 패배 시퀀스

```
[플레이어 HP 0 도달]
    ↓
1. 상태 전환
   currentState = CombatState.Defeat
    ↓
2. 패배 화면 표시
   combatUI.ShowDefeatScreen()
   - "패배..." 메시지
   - 재시작 버튼
   - 메인 메뉴 버튼
    ↓
3. 게임 오버 처리 (Post-MVP)
   - 런 통계 표시
   - 달성 업적 표시
   - 메타 진행도 업데이트
    ↓
4. 재시작 or 메인 메뉴
   - 재시작 → 새 게임 시작
   - 메인 메뉴 → 타이틀로 복귀
```

---

## 🗺️ 진행 시스템 (Post-MVP)

### 맵 시스템

```
[전투 승리]
    ↓
[보상 선택]
  - 유물
  - 블록 업그레이드
  - 골드
    ↓
[맵 화면]
    ↓
[노드 선택]
  - ⚔️ 일반 전투
  - 👹 엘리트 전투
  - 💰 상점
  - ❓ 이벤트
  - 🔥 휴식 (HP 회복)
  - 👑 보스
    ↓
[선택된 노드 진입]
    ↓
[전투 or 이벤트 실행]
    ↓
... 반복 ...
    ↓
[보스 처치]
    ↓
[다음 지역 or 게임 클리어]
```

### 전투 간 연속성

**유지되는 요소**:
- ✅ Player HP (현재 체력)
- ✅ Player Defense (방어력)
- ✅ Player Gold (골드)
- ✅ 획득한 유물
- ✅ 블록 덱 (업그레이드 포함)

**초기화되는 요소**:
- ❌ Enemy HP (매 전투마다 새 적)
- ❌ Enemy 상태 효과 (깨끗한 상태)
- ❌ 보드 배치 (랜덤 생성)

**예시**:

```
전투 1 종료:
  Player HP: 85/100
  Player Defense: 20/30
  Player Gold: 15

상점 방문:
  골드 15 소비 → 유물 구매
  Player Gold: 0

전투 2 시작:
  Player HP: 85/100 (유지)
  Player Defense: 20/30 (유지)
  Player Gold: 0 (유지)
  Enemy HP: 80/80 (새 적)
```

### 난이도 곡선

```
지역 1 (초급):
  - 슬라임 (HP 30, 공격 5)
  - 고블린 (HP 50, 공격 8)
  - 보스: 오크 (HP 150, 공격 15)

지역 2 (중급):
  - 적 HP +50%
  - 적 공격력 +30%
  - 새로운 행동 패턴 추가

지역 3 (고급):
  - 적 HP +100%
  - 적 공격력 +60%
  - 복합 행동 (Attack + Buff)
  - 2체 동시 전투

보스 (최종):
  - 멀티 페이즈
  - 특수 메커니즘
```

---

## 📝 요약: 구현 체크리스트

### MVP (현재)

**전투 초기화**:
- ✅ 적 생성 및 초기화
- ✅ 플레이어 상태 유지 (HP, Defense, Gold)
- ✅ 보드 생성
- ✅ UI 초기화
- ✅ 적 첫 행동 결정 및 예고

**플레이어 턴**:
- ✅ 보드 입력 처리
- ✅ 블록 효과 적용 (Red, Blue, Yellow, Brown)
- ✅ UI 업데이트 (팝업, 스탯)
- ❌ 플레이어 상태 효과 처리 (Post-MVP)

**적 턴**:
- ✅ 적 상태 효과 처리 (REGEN, POISON, STR, ARMOR 등)
- ✅ 예고된 행동 실행 (Attack, Defend, Buff)
- ✅ 다음 행동 결정 및 예고
- ✅ UI 업데이트

**승패 판정**:
- ✅ 적 HP 0 → 승리
- ✅ 플레이어 HP 0 → 패배
- ✅ 골드 보상
- ✅ 승리/패배 화면 표시

### Post-MVP

**전투 시스템**:
- ❌ 플레이어 상태 효과 시스템
- ❌ 다중 적 전투 (2-4체)
- ❌ 복합 행동 (Attack + Buff)
- ❌ 특수 메커니즘 (소환, 변신)

**진행 시스템**:
- ❌ 맵 시스템
- ❌ 보상 선택 화면
- ❌ 상점
- ❌ 이벤트
- ❌ 지역 시스템
- ❌ 난이도 곡선

**메타 시스템**:
- ❌ 런 통계
- ❌ 업적
- ❌ 메타 진행도 (영구 업그레이드)

---

**작성일**: 2025-12-18
**버전**: 1.0
**담당**: 게임 디자인
