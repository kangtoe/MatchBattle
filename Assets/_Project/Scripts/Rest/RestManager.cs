using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 휴식 관리자 (HP 회복)
    /// </summary>
    public class RestManager : Singleton<RestManager>
    {
        // 회복량 상수
        private const int REST_HEAL_AMOUNT = 30;

        // 휴식 완료 이벤트
        public UnityEvent OnRestCompleted = new UnityEvent();

        /// <summary>
        /// HP 회복
        /// </summary>
        public void Rest(Player player)
        {
            if (player == null)
            {
                Debug.LogWarning("[Rest] Player is null!");
                OnRestCompleted?.Invoke();
                return;
            }

            int healAmount = REST_HEAL_AMOUNT;
            int actualHeal = Mathf.Min(healAmount, player.MaxHP - player.CurrentHP);

            player.Heal(healAmount);
            Debug.Log($"[Rest] Player rested: +{actualHeal} HP (HP: {player.CurrentHP}/{player.MaxHP})");

            OnRestCompleted?.Invoke();
        }

        /// <summary>
        /// 건너뛰기
        /// </summary>
        public void Skip()
        {
            Debug.Log("[Rest] Rest skipped by player");
            OnRestCompleted?.Invoke();
        }

        /// <summary>
        /// 회복량 반환 (UI 표시용)
        /// </summary>
        public int GetRestHealAmount()
        {
            return REST_HEAL_AMOUNT;
        }
    }
}
