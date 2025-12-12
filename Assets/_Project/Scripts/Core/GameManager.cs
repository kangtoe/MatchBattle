using UnityEngine;

namespace MatchBattle
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // 골드 (Phase 2: 간단한 로그만)
        private int gold = 0;

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

        public void AddGold(int amount)
        {
            gold += amount;
            Debug.Log($"[Game] Gold gained: +{amount} (Total: {gold})");
        }
    }
}
