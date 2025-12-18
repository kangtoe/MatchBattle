using System;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 플레이어 데이터 및 상태 관리
    /// </summary>
    [Serializable]
    public class Player : Character
    {
        // 플레이어 전용 스탯
        [SerializeField] private int _gold;

        // 플레이어 전용 프로퍼티
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

        // 플레이어 전용 이벤트
        public UnityEvent<int> OnGoldChanged = new UnityEvent<int>();

        // 생성자
        public Player(int maxHP = 100, int maxDefense = 30, int startingGold = 0)
            : base("플레이어", maxHP, maxDefense)
        {
            _gold = startingGold;
        }

        // 플레이어 전용 메서드
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            Gold += amount;
            Debug.Log($"[Player] Gained {amount} gold (Gold: {Gold})");
        }

        // 상태 로그 오버라이드 (골드 포함)
        public override void LogStatus()
        {
            string statusText = "";
            foreach (var effect in statusEffects)
            {
                statusText += $"{effect.GetDisplayText()} ";
            }

            Debug.Log($"[Player Status] HP: {CurrentHP}/{MaxHP}, Defense: {Defense}/{MaxDefense}, Attack: {CurrentAttackPower} (STR), Gold: {Gold}, Effects: {statusText}");
        }
    }
}
