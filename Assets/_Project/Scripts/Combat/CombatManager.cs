using UnityEngine;

namespace MatchBattle
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        // 플레이어 스탯 (Phase 2: 간단한 로그만)
        private int playerHP = 100;
        private int playerDefense = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Phase 2: 블록 효과 적용 (로그만 출력)
        public void DealDamage(int damage)
        {
            Debug.Log($"[Combat] Dealing {damage} damage to enemy");
        }

        public void AddDefense(int defense)
        {
            playerDefense += defense;
            if (playerDefense > 30) playerDefense = 30; // 최대 30
            Debug.Log($"[Combat] Defense gained: +{defense} (Total: {playerDefense})");
        }

        public void HealPlayer(int heal)
        {
            playerHP += heal;
            Debug.Log($"[Combat] Healed: +{heal} HP (Total: {playerHP})");
        }

        // Phase 2: 상태 효과는 나중에
        public void ApplyStatusEffect(object effect)
        {
            Debug.Log($"[Combat] Status effect applied (not implemented yet)");
        }

        // Phase 2: 플레이어 턴 종료 (나중에 구현)
        public void EndPlayerTurn()
        {
            Debug.Log("[Combat] Player turn ended");
        }
    }
}
