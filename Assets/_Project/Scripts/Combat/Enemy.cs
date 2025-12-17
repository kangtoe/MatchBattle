using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 적 데이터 및 AI 관리
    /// </summary>
    [Serializable]
    public class Enemy
    {
        // 기본 정보
        [SerializeField] private string _enemyName;
        [SerializeField] private int _currentHP;
        [SerializeField] private int _maxHP;
        [SerializeField] private int _defense;

        // AI 패턴
        public List<EnemyAction> actionPool = new List<EnemyAction>();
        public EnemyAction nextAction;
        public EnemyAction currentAction;

        // 특수 능력 (Enrage)
        public bool hasEnragePhase = false;
        public int enrageBonus = 0;
        public bool isEnraged = false;

        // 프로퍼티
        public string EnemyName
        {
            get => _enemyName;
            set => _enemyName = value;
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
                _defense = Mathf.Max(0, value);

                if (oldValue != _defense)
                {
                    OnDefenseChanged?.Invoke(_defense);
                }
            }
        }

        // 이벤트
        public UnityEvent<int> OnHPChanged = new UnityEvent<int>();
        public UnityEvent<int> OnDefenseChanged = new UnityEvent<int>();
        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent OnEnraged = new UnityEvent();

        // 생성자
        public Enemy(string name, int maxHP, List<EnemyAction> actions = null, bool hasEnrage = false, int enrageBonus = 0)
        {
            _enemyName = name;
            _maxHP = maxHP;
            _currentHP = maxHP;
            _defense = 0;

            if (actions != null)
            {
                actionPool = new List<EnemyAction>(actions);
            }

            this.hasEnragePhase = hasEnrage;
            this.enrageBonus = enrageBonus;
            this.isEnraged = false;
        }

        // 헬퍼 메서드
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            // 방어력 먼저 소모
            if (Defense >= damage)
            {
                // 방어력으로 완전히 막음
                Defense -= damage;
                Debug.Log($"[{EnemyName}] Blocked {damage} damage with defense (Defense: {Defense})");
            }
            else
            {
                // 방어력 먼저 소모하고 남은 데미지는 HP에서 차감
                int remainingDamage = damage - Defense;

                if (Defense > 0)
                {
                    Debug.Log($"[{EnemyName}] Defense absorbed {Defense} damage");
                }

                Defense = 0;
                CurrentHP -= remainingDamage;

                Debug.Log($"[{EnemyName}] Took {remainingDamage} damage (HP: {CurrentHP}/{MaxHP})");
            }
        }

        public void AddDefense(int amount)
        {
            if (amount <= 0) return;

            Defense += amount;
            Debug.Log($"[{EnemyName}] Gained {amount} defense (Defense: {Defense})");
        }

        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        public bool ShouldEnrage()
        {
            return hasEnragePhase && !isEnraged && CurrentHP <= MaxHP / 2;
        }

        public void ApplyEnrage()
        {
            if (isEnraged) return;

            isEnraged = true;

            // 모든 공격 행동의 데미지 증가
            foreach (var action in actionPool)
            {
                if (action.type == EnemyActionType.Attack || action.type == EnemyActionType.HeavyAttack)
                {
                    action.value += enrageBonus;
                }
            }

            Debug.Log($"[{EnemyName}] ENRAGED! All attacks increased by {enrageBonus}");

            // 분노 이벤트 발생
            OnEnraged?.Invoke();
        }

        // AI - 행동 선택
        public EnemyAction SelectNextAction()
        {
            if (actionPool.Count == 0)
            {
                Debug.LogWarning($"[{EnemyName}] No actions available!");
                return new EnemyAction(EnemyActionType.Attack, 5, 1f, false, "기본 공격 5");
            }

            // Enrage 체크
            if (ShouldEnrage())
            {
                ApplyEnrage();
            }

            // 가중치 기반 랜덤 선택
            float totalWeight = 0f;
            foreach (var action in actionPool)
            {
                totalWeight += action.weight;
            }

            float rand = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var action in actionPool)
            {
                cumulative += action.weight;
                if (rand <= cumulative)
                {
                    nextAction = action;
                    Debug.Log($"[{EnemyName}] Next action selected: {action.description}");
                    return action;
                }
            }

            // Fallback
            nextAction = actionPool[0];
            return nextAction;
        }

        // 디버그용
        public void LogStatus()
        {
            Debug.Log($"[{EnemyName} Status] HP: {CurrentHP}/{MaxHP}, Defense: {Defense}, Enraged: {isEnraged}");
        }
    }
}
