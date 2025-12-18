using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 전투 캐릭터 베이스 클래스 (플레이어/적 공통)
    /// </summary>
    [Serializable]
    public class Character
    {
        // 기본 정보
        [SerializeField] protected string _name;
        [SerializeField] protected int _currentHP;
        [SerializeField] protected int _maxHP;
        [SerializeField] protected int _defense;
        [SerializeField] protected int _maxDefense;

        // 상태 효과 시스템
        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        // ===========================================
        // 프로퍼티
        // ===========================================

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public int CurrentHP
        {
            get => _currentHP;
            set
            {
                int oldValue = _currentHP;
                _currentHP = Mathf.Clamp(value, 0, _maxHP);

                if (oldValue != _currentHP)
                {
                    OnHPChanged?.Invoke(_currentHP);

                    if (_currentHP <= 0)
                    {
                        OnDeath?.Invoke();
                    }
                }
            }
        }

        public int MaxHP
        {
            get => _maxHP;
            set => _maxHP = Mathf.Max(1, value);
        }

        public int Defense
        {
            get => _defense;
            set
            {
                int oldValue = _defense;
                _defense = Mathf.Clamp(value, 0, _maxDefense);

                if (oldValue != _defense)
                {
                    OnDefenseChanged?.Invoke(_defense);
                }
            }
        }

        public int MaxDefense
        {
            get => _maxDefense;
            set => _maxDefense = Mathf.Max(0, value);
        }

        // 현재 공격력 (STR만 사용)
        public int CurrentAttackPower
        {
            get => GetSTR();
        }

        // ===========================================
        // 이벤트
        // ===========================================

        public UnityEvent<int> OnHPChanged = new UnityEvent<int>();
        public UnityEvent<int> OnDefenseChanged = new UnityEvent<int>();
        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent<StatusEffect> OnStatusEffectAdded = new UnityEvent<StatusEffect>();
        public UnityEvent<StatusEffect> OnStatusEffectRemoved = new UnityEvent<StatusEffect>();
        public UnityEvent OnStatusEffectsUpdated = new UnityEvent(); // 상태 효과 값 변경 시

        // ===========================================
        // 생성자
        // ===========================================

        public Character(string name, int maxHP, int maxDefense = 0)
        {
            _name = name;
            _maxHP = maxHP;
            _currentHP = maxHP;
            _maxDefense = maxDefense;
            _defense = 0;

            statusEffects = new List<StatusEffect>();
        }

        // ===========================================
        // 상태 효과 관리
        // ===========================================

        /// <summary>
        /// 상태 효과 추가
        /// </summary>
        public void AddStatusEffect(StatusEffect effect)
        {
            var existing = statusEffects.FirstOrDefault(e => e.type == effect.type);

            if (existing != null)
            {
                // 동일한 타입이 이미 존재하면 스택
                switch (effect.type)
                {
                    case StatusEffectType.STR:
                    case StatusEffectType.PLATED:
                    case StatusEffectType.REGEN:
                    case StatusEffectType.POISON:
                    case StatusEffectType.EXHAUSTED:
                        // 수치 누적
                        existing.value += effect.value;
                        break;

                    case StatusEffectType.WEAK:
                    case StatusEffectType.VULNERABLE:
                        // 지속시간 증가
                        existing.duration += effect.duration;
                        break;
                }

                Debug.Log($"[{Name}] Status effect stacked: {existing.GetDisplayText()}");
            }
            else
            {
                // 새 상태 효과 추가
                statusEffects.Add(effect);
                Debug.Log($"[{Name}] Status effect added: {effect.GetDisplayText()} - {effect.GetDescription()}");
                OnStatusEffectAdded?.Invoke(effect);
            }
        }

        /// <summary>
        /// 상태 효과 제거
        /// </summary>
        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (statusEffects.Remove(effect))
            {
                Debug.Log($"[{Name}] Status effect removed: {effect.GetDisplayText()}");
                OnStatusEffectRemoved?.Invoke(effect);
            }
        }

        /// <summary>
        /// 현재 힘(STR) 값 가져오기
        /// </summary>
        public int GetSTR()
        {
            var strEffect = statusEffects.FirstOrDefault(e => e.type == StatusEffectType.STR);
            return strEffect?.value ?? 0;
        }

        /// <summary>
        /// 현재 금속화(PLATED) 값 가져오기
        /// </summary>
        public int GetPLATED()
        {
            var platedEffect = statusEffects.FirstOrDefault(e => e.type == StatusEffectType.PLATED);
            return platedEffect?.value ?? 0;
        }

        /// <summary>
        /// 약화(WEAK) 상태인지 확인
        /// </summary>
        public bool HasWEAK()
        {
            return statusEffects.Any(e => e.type == StatusEffectType.WEAK && e.duration > 0);
        }

        /// <summary>
        /// 취약(VULNERABLE) 상태인지 확인
        /// </summary>
        public bool HasVULNERABLE()
        {
            return statusEffects.Any(e => e.type == StatusEffectType.VULNERABLE && e.duration > 0);
        }

        /// <summary>
        /// 카테고리별 상태 효과 필터링
        /// </summary>
        protected IEnumerable<StatusEffect> GetEffectsByCategory(StatusEffectCategory category)
        {
            return statusEffects.Where(e => e.GetCategory() == category);
        }

        /// <summary>
        /// 힘(STR) 추가
        /// </summary>
        public void AddSTR(int amount)
        {
            if (amount == 0) return;

            AddStatusEffect(new StatusEffect(StatusEffectType.STR, amount, -1));
            Debug.Log($"[{Name}] STR {(amount > 0 ? "+" : "")}{amount} (Total: {GetSTR()})");
        }

        /// <summary>
        /// 금속화(PLATED) 추가
        /// </summary>
        public void AddPLATED(int amount)
        {
            if (amount <= 0) return;

            AddStatusEffect(new StatusEffect(StatusEffectType.PLATED, amount, -1));
            Debug.Log($"[{Name}] PLATED +{amount} (Total: {GetPLATED()})");
        }

        /// <summary>
        /// 방어력(Defense) 추가
        /// </summary>
        public void AddDefense(int amount)
        {
            if (amount <= 0) return;

            int oldDefense = Defense;
            Defense += amount;
            int actualGain = Defense - oldDefense;

            Debug.Log($"[{Name}] Gained {actualGain} defense (Defense: {Defense}/{MaxDefense})");
        }

        // ===========================================
        // 데미지 및 회복
        // ===========================================

        /// <summary>
        /// 데미지 받기 (PLATED → Defense → HP 순서)
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            int originalDamage = damage;

            // Step 1: PLATED 적용 (영구, 소모 안 됨)
            int plated = GetPLATED();
            if (plated > 0)
            {
                int blocked = Mathf.Min(damage, plated);
                damage -= plated;
                damage = Mathf.Max(0, damage);
                Debug.Log($"[{Name}] PLATED {plated} blocked {blocked} damage");
            }

            // Step 2: Defense 소모 (임시, 소모됨)
            if (Defense > 0 && damage > 0)
            {
                if (Defense >= damage)
                {
                    // Defense로 완전히 막음
                    Defense -= damage;
                    Debug.Log($"[{Name}] Defense absorbed {damage} damage (Defense: {Defense}/{MaxDefense})");
                    damage = 0;
                }
                else
                {
                    // Defense 전부 소모하고 남은 데미지는 HP로
                    int defenseUsed = Defense;
                    damage -= Defense;
                    Defense = 0;
                    Debug.Log($"[{Name}] Defense absorbed {defenseUsed} damage (Defense: 0/{MaxDefense})");
                }
            }

            // Step 3: HP 감소
            if (damage > 0)
            {
                CurrentHP -= damage;
                Debug.Log($"[{Name}] Took {damage} HP damage (HP: {CurrentHP}/{MaxHP})");
            }
            else
            {
                Debug.Log($"[{Name}] No HP damage! (Original: {originalDamage}, PLATED: {plated}, Defense: {Defense})");
            }
        }

        /// <summary>
        /// 회복
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int oldHP = CurrentHP;
            CurrentHP += amount;
            int actualHealed = CurrentHP - oldHP;

            if (actualHealed > 0)
            {
                Debug.Log($"[{Name}] Healed {actualHealed} HP (HP: {CurrentHP}/{MaxHP})");
            }
        }

        /// <summary>
        /// 생존 확인
        /// </summary>
        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        // ===========================================
        // 턴 처리
        // ===========================================

        /// <summary>
        /// 턴 시작 시 상태 효과 처리
        /// </summary>
        public void ProcessTurnStart()
        {
            var effectsToRemove = new List<StatusEffect>();

            // 1. 감소형 효과 처리 (REGEN, POISON)
            foreach (var effect in GetEffectsByCategory(StatusEffectCategory.Decrement).ToList())
            {
                // 효과 발동
                if (effect.type == StatusEffectType.REGEN)
                {
                    Heal(effect.value);
                }
                else if (effect.type == StatusEffectType.POISON)
                {
                    TakeDamage(effect.value);
                }

                // 수치 감소
                effect.value--;
                if (effect.value <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }

            // 2. 지속형 효과 처리 (WEAK, VULNERABLE)
            foreach (var effect in GetEffectsByCategory(StatusEffectCategory.Duration).ToList())
            {
                effect.duration--;
                if (effect.duration <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }

            // 제거할 효과 처리
            foreach (var effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }

            // UI 업데이트를 위한 이벤트 발생
            OnStatusEffectsUpdated?.Invoke();
        }

        /// <summary>
        /// 턴 종료 시 상태 효과 처리
        /// </summary>
        public void ProcessTurnEnd()
        {
            var effectsToRemove = new List<StatusEffect>();

            // 스택형 중 특수 처리: EXHAUSTED (턴 종료 시 발동)
            var exhaustedEffect = statusEffects.FirstOrDefault(e => e.type == StatusEffectType.EXHAUSTED);
            if (exhaustedEffect != null)
            {
                AddSTR(-exhaustedEffect.value); // 힘 감소
                effectsToRemove.Add(exhaustedEffect);
                Debug.Log($"[{Name}] EXHAUSTED triggered: STR -{exhaustedEffect.value} (STR: {GetSTR()})");
            }

            // 제거할 효과 처리
            foreach (var effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }

            // UI 업데이트를 위한 이벤트 발생
            OnStatusEffectsUpdated?.Invoke();
        }

        // ===========================================
        // 디버그
        // ===========================================

        /// <summary>
        /// 상태 로그 출력
        /// </summary>
        public virtual void LogStatus()
        {
            string statusText = "";
            foreach (var effect in statusEffects)
            {
                statusText += $"{effect.GetDisplayText()} ";
            }

            Debug.Log($"[{Name} Status] HP: {CurrentHP}/{MaxHP}, Defense: {Defense}/{MaxDefense}, Attack: {CurrentAttackPower} (STR), Effects: {statusText}");
        }
    }
}
