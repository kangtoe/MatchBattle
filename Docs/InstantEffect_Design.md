# 즉시 효과 시스템 설계 (Instant Effect System Design)

**프로젝트**: MatchBattle
**버전**: 1.0
**작성일**: 2026-01-05
**관련 문서**:
- [StatusEffects.md](StatusEffects.md) - 상태 효과 시스템
- [RelicSystem_Design.md](RelicSystem_Design.md) - 유물 시스템
- [RewardSystem_Design.md](RewardSystem_Design.md) - 보상 시스템

---

## 📋 목차

1. [개요](#-개요)
2. [상태 효과와의 차이점](#-상태-효과와의-차이점)
3. [즉시 효과 목록](#-즉시-효과-목록)
4. [사용처](#-사용처)
5. [MVP 범위](#-mvp-범위)

---

## 📖 개요

### 목적
**한 번 발동하고 즉시 완료**되는 효과 시스템

### 핵심 원칙
1. **즉시 적용**: 발동 시 바로 효과 적용
2. **잔여 상태 없음**: 적용 후 저장/관리 불필요
3. **기존 메서드 활용**: Character.Heal(), MaxHP 등 재사용

---

## ⚖️ 상태 효과와의 차이점

```
┌─────────────────┬──────────────────┬──────────────────┐
│                 │ 상태 효과         │ 즉시 효과         │
│                 │ (StatusEffect)   │ (InstantEffect)  │
├─────────────────┼──────────────────┼──────────────────┤
│ 지속 시간       │ 턴 기반 지속      │ 없음 (1회성)      │
│ 저장 필요       │ O (리스트 관리)   │ X                │
│ 턴 처리         │ O (매 턴 처리)    │ X                │
│ 예시            │ STR, POISON 등   │ HP 회복, 골드 등  │
└─────────────────┴──────────────────┴──────────────────┘
```

---

## 🎯 즉시 효과 목록

### MVP 효과

#### HEAL - HP 회복
```
ID: HEAL
효과: 현재 HP +N 회복
대상: 플레이어
구현: Character.Heal(N)

예시: HEAL(10) → HP +10 회복
```

#### MAX_HP_UP - 최대 HP 증가
```
ID: MAX_HP_UP
효과: 최대 HP +N 증가 (현재 HP도 함께 증가)
대상: 플레이어
구현: Character.MaxHP += N; Character.CurrentHP += N;

예시: MAX_HP_UP(5) → 최대 HP +5, 현재 HP +5
```

#### GOLD_GAIN - 골드 획득
```
ID: GOLD_GAIN
효과: 골드 +N 획득
대상: 플레이어
구현: PlayerData.AddGold(N)

예시: GOLD_GAIN(25) → 골드 +25
```

### Post-MVP 효과

#### MAX_HP_DOWN - 최대 HP 감소
```
ID: MAX_HP_DOWN
효과: 최대 HP -N 감소
대상: 플레이어
구현: Character.MaxHP -= N

예시: MAX_HP_DOWN(5) → 최대 HP -5
용도: 강력한 유물의 페널티
```

#### HP_LOSS - 현재 HP 손실
```
ID: HP_LOSS
효과: 현재 HP -N 손실 (방어력 무시)
대상: 플레이어
구현: Character.CurrentHP -= N

예시: HP_LOSS(10) → 현재 HP -10
용도: 유물/이벤트의 대가
```

#### DEFENSE_GAIN - 방어력 획득
```
ID: DEFENSE_GAIN
효과: 방어력 +N 획득
대상: 플레이어
구현: Character.AddDefense(N)

예시: DEFENSE_GAIN(5) → 방어력 +5
```

#### DAMAGE - 즉시 데미지
```
ID: DAMAGE
효과: 대상에게 N 데미지
대상: 적
구현: Character.TakeDamage(N)

예시: DAMAGE(10) → 적에게 10 데미지
```

---

## 🔧 사용처

### 유물 획득 시 (OnAcquire)
```
생명의 씨앗: HEAL(10)
강인한 심장: MAX_HP_UP(5)
황금 주머니: GOLD_GAIN(25)
```

### 보상 선택 시
```
HP 회복 보상: HEAL(15~25)
최대 HP 증가 보상: MAX_HP_UP(5)
골드 보상: GOLD_GAIN(15~20)
```

### 전투 시작 시 (OnBattleStart)
```
회복의 부적: HEAL(5) - 전투 시작 시 HP +5
```

---

## 📦 MVP 범위

### MVP 포함
```
✅ 포함:
- HEAL (HP 회복)
- MAX_HP_UP (최대 HP 증가)
- GOLD_GAIN (골드 획득)

❌ 제외 (Post-MVP):
- MAX_HP_DOWN (최대 HP 감소) - 강력한 유물의 페널티
- HP_LOSS (현재 HP 손실) - 유물/이벤트의 대가
- DEFENSE_GAIN (방어력 획득)
- DAMAGE (즉시 데미지)
```

---

**작성일**: 2026-01-05
**버전**: 1.0
**담당**: 게임 디자인
