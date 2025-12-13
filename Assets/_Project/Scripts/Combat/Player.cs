using System;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 플레이어 데이터 및 상태 관리
    /// </summary>
    [Serializable]
    public class Player
    {
        // 기본 스탯
        [SerializeField] private int _currentHP;
        [SerializeField] private int _maxHP;
        [SerializeField] private int _defense;
        [SerializeField] private int _maxDefense;
        [SerializeField] private int _gold;

        // 프로퍼티
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

        public int Gold
        {
            get => _gold;
            set
            {
                int oldValue = _gold;
                _gold = Mathf.Max(0, value);

                if (oldValue != _gold)
                {
                    OnGoldChanged?.Invoke(_gold);
                }
            }
        }

        // 이벤트
        public UnityEvent<int> OnHPChanged = new UnityEvent<int>();
        public UnityEvent<int> OnDefenseChanged = new UnityEvent<int>();
        public UnityEvent<int> OnGoldChanged = new UnityEvent<int>();
        public UnityEvent OnDeath = new UnityEvent();

        // 생성자
        public Player(int maxHP = 100, int maxDefense = 30, int startingGold = 0)
        {
            _maxHP = maxHP;
            _maxDefense = maxDefense;
            _currentHP = maxHP;
            _defense = 0;
            _gold = startingGold;
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
                Debug.Log($"[Player] Blocked {damage} damage with defense (Defense: {Defense})");
            }
            else
            {
                // 방어력 먼저 소모하고 남은 데미지는 HP에서 차감
                int remainingDamage = damage - Defense;

                if (Defense > 0)
                {
                    Debug.Log($"[Player] Defense absorbed {Defense} damage");
                }

                Defense = 0;
                CurrentHP -= remainingDamage;

                Debug.Log($"[Player] Took {remainingDamage} damage (HP: {CurrentHP}/{MaxHP})");
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int actualHeal = Mathf.Min(amount, MaxHP - CurrentHP);
            CurrentHP += actualHeal;

            Debug.Log($"[Player] Healed {actualHeal} HP (HP: {CurrentHP}/{MaxHP})");
        }

        public void AddDefense(int amount)
        {
            if (amount <= 0) return;

            int oldDefense = Defense;
            Defense += amount;
            int actualGain = Defense - oldDefense;

            Debug.Log($"[Player] Gained {actualGain} defense (Defense: {Defense}/{MaxDefense})");
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            Gold += amount;
            Debug.Log($"[Player] Gained {amount} gold (Gold: {Gold})");
        }

        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        // 디버그용
        public void LogStatus()
        {
            Debug.Log($"[Player Status] HP: {CurrentHP}/{MaxHP}, Defense: {Defense}/{MaxDefense}, Gold: {Gold}");
        }
    }
}
