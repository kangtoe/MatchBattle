# 맵 시스템 기술 문서

**목적**: 랜덤 맵 생성 및 진행 시스템 구현 가이드
**대상**: 프로그래머
**관련 문서**: [MapSystem_Design.md](MapSystem_Design.md)

---

## 🏗️ 시스템 아키텍처

### 핵심 컴포넌트
```
MapManager (싱글톤)
    ├─ MapData 보유 및 관리
    ├─ 스테이지 완료 처리
    ├─ 다음 선택지 제공
    └─ 진행 내역 관리

MapGenerator (Static Utility)
    ├─ 랜덤 맵 생성 (시드 기반)
    └─ 노드 연결 처리 (각 노드당 1-3개)

StageSelectionUI
    ├─ 다음 스테이지 선택지 표시 (1-3개 카드)
    └─ 선택 처리

StageHistoryUI (Post-MVP)
    └─ 진행한 스테이지 내역 표시

ScriptableObjects
    ├─ StageTypeConfig (스테이지 타입 확률)
    ├─ MapGenerationConfig (맵 생성 설정, 조우 풀)
    ├─ EncounterData (전투 조우 - 4개 슬롯 적 배치)
    └─ EncounterPool (스테이지별 조우 풀)
```

---

## 📐 핵심 데이터 구조

### StageNode (스테이지 노드)
```
역할: 맵의 개별 스테이지 표현

주요 데이터:
- stageIndex (단계 번호: 1-7)
- stageType (전투/상점/휴식/등)
- nextNodes (다음 선택지 노드 리스트: 1-3개)
- isCompleted (완료 여부)

참고: 적 데이터는 노드에 저장하지 않음 (조우 풀 시스템 사용)
```

### MapData (맵 전체 데이터)
```
역할: 전체 맵 구조 및 진행 상태

주요 데이터:
- rootNode (시작 노드)
- currentNode (현재 위치)
- completedNodes (완료한 노드 리스트 - 내역 추적용)
- seed (재현 가능한 맵 생성)
```

### StageType (Enum)
```
Combat, Elite, Shop, Rest, Event, Boss
```

### EncounterData (조우 데이터)
```
역할: 전투 시 4개 슬롯에 적 배치 정보 정의

주요 데이터:
- encounterName (조우 이름)
- enemySlot0 ~ enemySlot3 (4개 고정 슬롯, null 허용)

메서드:
- GetEnemySlots(): 배열로 반환
- GetEnemyCount(): 비어있지 않은 슬롯 개수
```

### EncounterPool (조우 풀)
```
역할: 특정 스테이지/타입에서 출현 가능한 조우 리스트

주요 데이터:
- poolName (풀 이름)
- stageNumber (스테이지 번호: 1-7)
- encounterType (Combat 또는 Elite)
- encounters (EncounterData 리스트)

메서드:
- GetRandomEncounter(): 풀에서 랜덤 조우 선택
```

---

## 🎲 맵 생성 알고리즘

### 생성 흐름 (트리 구조)
```
1. 시드 초기화
2. Stage 1 시작 노드 생성 (Combat 고정)
3. 재귀적으로 다음 노드 생성
   - 현재 stageIndex에 따라 1-3개 다음 노드 생성
   - Stage 6 → Stage 7 (Boss): 1개만 생성
   - Stage 7: 다음 노드 없음 (종료)
4. 각 노드의 타입을 확률 기반으로 결정
```

### 노드 연결 규칙
```
- Stage 1-5: 각 노드가 1-3개의 다음 노드를 가짐
- Stage 6 → Stage 7: 1개 (보스로 직행)
- Stage 7 (Boss): 다음 노드 없음

선택지 개수 결정:
- 랜덤하게 1-3 중 선택
- 또는 단계별 고정 (예: Stage 1-2는 3개, Stage 3-4는 2개, Stage 5-6은 1-2개)
```

### 스테이지 타입 선택 (확률 기반)
```
StageTypeConfig (ScriptableObject)
- 각 타입별 spawnWeight (확률)
- minStageIndex (출현 시작 단계)

선택 로직:
1. 현재 stageIndex에서 생성 가능한 타입 필터링 (minStageIndex 이상)
2. 가중치 합산
3. 랜덤 값으로 가중치 기반 선택
```

---

## ⚔️ 조우 풀 시스템

### 개념
```
맵 생성 시점: 스테이지 타입만 결정 (적은 미리 할당하지 않음)
전투 진입 시점: 해당 스테이지에 맞는 조우 풀에서 랜덤 선택
```

### 조우 선택 흐름
```
1. 플레이어가 Combat/Elite 스테이지 진입
2. stageNumber + stageType 확인 (예: Stage 3 - Combat)
3. MapGenerationConfig.GetEncounterPool(3, Combat) 호출
4. 해당 EncounterPool의 encounters 리스트에서 랜덤 선택
5. 선택된 EncounterData의 4개 슬롯 정보로 적 생성
6. CombatManager에 적 배치 후 전투 시작
```

### 데이터 구성 예시
```
EncounterPool: "Stage 2 - Combat"
├─ stageNumber: 2
├─ encounterType: Combat
└─ encounters:
    ├─ "슬라임 단독" (슬롯0: Slime, 나머지: null)
    ├─ "고블린 듀오" (슬롯0: Goblin, 슬롯3: Goblin)
    └─ "슬라임 트리오" (슬롯0,1,2: Slime)

전투 진입 시 → 위 3개 중 랜덤 선택
```

### 장점
```
1. 맵 생성 단순화: 적 할당 로직 제거
2. 다양성: 같은 Stage 2 Combat이라도 다른 조우 가능
3. 밸런싱: 스테이지별로 조우 난이도 세밀 조정
4. 확장성: 새로운 조우를 SO로 쉽게 추가
```

---

## 🔄 시스템 플로우

### 런 시작
```
MainMenu
    ↓
MapManager.StartNewRun()
    ↓
MapGenerator.GenerateMap(config, seed)
    ↓
첫 번째 노드(Stage 1 Combat)로 즉시 진입
    ↓
CombatScene 로드
```

### 스테이지 완료 및 선택
```
스테이지 완료 (전투 승리 / 상점 종료 / 휴식 완료)
    ↓
MapManager.CompleteCurrentStage()
    ├─ 현재 노드를 completedNodes에 추가
    └─ 다음 선택지 노드 리스트 반환 (1-3개)
    ↓
StageSelectionUI 표시
    ├─ 각 선택지를 카드로 표시
    ├─ 스테이지 타입, 아이콘, 간단한 정보
    └─ 보스 스테이지는 1개만 (필수 선택)
    ↓
플레이어가 선택지 클릭
    ↓
MapManager.SelectNextStage(node)
    ├─ currentNode 업데이트
    └─ 해당 스테이지로 즉시 진입
    ↓
스테이지 타입에 따라 처리
    ├─ Combat/Elite/Boss → CombatScene 로드
    ├─ Shop → Shop UI (임시: 바로 완료 → 선택지)
    └─ Rest → Rest UI (임시: 바로 완료 → 선택지)
```

### 런 종료
```
Boss 스테이지 완료
    ↓
다음 선택지 없음 (nextNodes 비어있음)
    ↓
MapManager.CompleteRun()
    ↓
승리 화면 표시
```

---

## 🔧 기존 시스템 통합

### CombatManager 수정
```
전투 시작 시:
1. MapManager에서 currentNode 정보 가져오기
2. stageNumber와 stageType으로 EncounterPool 찾기
   - config.GetEncounterPool(stageNumber, stageType)
3. EncounterPool에서 랜덤 조우 선택
   - pool.GetRandomEncounter()
4. EncounterData의 4개 슬롯 정보로 적 생성
   - encounter.GetEnemySlots()
5. 전투 초기화

Boss 전투 시:
- EncounterPool 대신 config.bossEncounter 직접 사용

전투 승리 시:
1. 보상 선택 UI 표시 (임시: "보상 받기" 버튼)
2. 보상 선택 완료 후:
   - nextNodes = MapManager.CompleteCurrentStage()
   - if (nextNodes.Count > 0):
       StageSelectionUI.Show(nextNodes)
     else:
       MapManager.CompleteRun() // 보스 클리어
```

### Player 데이터 영속성
```
- MapManager를 DontDestroyOnLoad로 유지
- Player 데이터도 MapManager에서 참조 보관
- 씬 전환 시에도 Player 상태 유지
```

---

## 📦 MVP 구현 우선순위

### Phase 1: 기본 구조 + 선택 시스템 (1일)
```
✅ 구현:
- StageNode, MapData, StageType (Enum)
- MapManager (싱글톤, 기본 로직)
- MapGenerator (트리 구조 랜덤 생성)
- StageTypeConfig, MapGenerationConfig (SO)
- StageSelectionUI (1-3개 선택지 카드)
- 보상 플레이스홀더 ("보상 받기" 버튼)

테스트:
- 콘솔 로그로 맵 트리 생성 확인
- 전투 → 선택지 → 다음 전투 플로우 테스트
```

### Phase 2: 완전한 통합 (Post-MVP)
```
✅ 구현:
- StageHistoryUI (진행 내역 확인)
- Shop, Rest, Event 시스템
- 실제 보상 선택 시스템
- 런 통계 및 승리/패배 화면

테스트:
- 전체 런 플레이 테스트
```

---

## 🎯 주요 고려사항

### 1. 데이터 구조
- MapData는 트리 구조 (rootNode 기반)
- completedNodes로 진행 내역 추적 → 향후 히스토리 UI에 활용
- StageNode는 참조로 관리 (복사 주의)

### 2. 씬 관리
- MapManager는 DontDestroyOnLoad
- CombatScene 내에서 StageSelectionUI 표시
- MapScene은 MVP에서 제외 (향후 히스토리 전용으로 추가 가능)

### 3. 선택지 생성
- 각 노드의 nextNodes는 생성 시점에 결정
- 동적 생성 아님 (전체 맵을 런 시작 시 생성)
- 플레이어는 미리 생성된 경로 중 선택

### 4. 확장성
- completedNodes로 진행 내역 기록 → StageHistoryUI 추가 가능
- 트리 구조라 분기 시각화 용이
- 이벤트/상점/휴식 시스템 추가 여유

---

**작성일**: 2025-12-22
**버전**: 1.1
**작성자**: Claude Code
