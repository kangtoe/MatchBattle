using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 유물 관리자 (보유 유물 관리 및 효과 발동)
    /// </summary>
    public class RelicManager : MonoBehaviour
    {
        public static RelicManager Instance { get; private set; }

        [Header("유물 풀")]
        [SerializeField] private List<RelicData> commonRelicPool = new List<RelicData>();
        [SerializeField] private List<RelicData> bossRelicPool = new List<RelicData>();

        // 현재 보유 유물
        private List<RelicData> ownedRelics = new List<RelicData>();

        // 유물 획득 이벤트
        public UnityEvent<RelicData> OnRelicAcquired = new UnityEvent<RelicData>();

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

        /// <summary>
        /// 유물 획득
        /// </summary>
        public bool AddRelic(RelicData relic)
        {
            if (relic == null)
            {
                Debug.LogWarning("[RelicManager] Cannot add null relic!");
                return false;
            }

            // 중복 체크
            if (HasRelic(relic))
            {
                Debug.LogWarning($"[RelicManager] Already own relic: {relic.displayName}");
                return false;
            }

            ownedRelics.Add(relic);
            Debug.Log($"[RelicManager] Acquired relic: {relic.displayName}");
            OnRelicAcquired?.Invoke(relic);
            return true;
        }

        /// <summary>
        /// 유물 보유 여부 확인
        /// </summary>
        public bool HasRelic(RelicData relic)
        {
            return ownedRelics.Contains(relic);
        }

        /// <summary>
        /// ID로 유물 보유 여부 확인
        /// </summary>
        public bool HasRelic(string relicId)
        {
            return ownedRelics.Exists(r => r.id == relicId);
        }

        /// <summary>
        /// 보유 유물 목록 반환
        /// </summary>
        public List<RelicData> GetOwnedRelics()
        {
            return new List<RelicData>(ownedRelics);
        }

        /// <summary>
        /// 보유 유물 수
        /// </summary>
        public int GetRelicCount()
        {
            return ownedRelics.Count;
        }

        /// <summary>
        /// 전투 시작 시 유물 효과 발동
        /// </summary>
        public void TriggerOnBattleStart(Player player, Enemy[] enemies)
        {
            foreach (var relic in ownedRelics)
            {
                if (relic.triggerType == RelicTriggerType.OnBattleStart)
                {
                    relic.ApplyEffect(player, enemies);
                }
            }
        }

        /// <summary>
        /// 턴 시작 시 유물 효과 발동 (Post-MVP)
        /// </summary>
        public void TriggerOnTurnStart(Player player, Enemy[] enemies)
        {
            foreach (var relic in ownedRelics)
            {
                if (relic.triggerType == RelicTriggerType.OnTurnStart)
                {
                    relic.ApplyEffect(player, enemies);
                }
            }
        }

        /// <summary>
        /// 미보유 일반 유물 중 랜덤 반환
        /// </summary>
        public RelicData GetRandomCommonRelic()
        {
            var available = commonRelicPool.FindAll(r => !HasRelic(r));
            if (available.Count == 0)
            {
                Debug.LogWarning("[RelicManager] No common relics available!");
                return null;
            }
            return available[Random.Range(0, available.Count)];
        }

        /// <summary>
        /// 미보유 보스 유물 중 랜덤 반환
        /// </summary>
        public RelicData GetRandomBossRelic()
        {
            var available = bossRelicPool.FindAll(r => !HasRelic(r));
            if (available.Count == 0)
            {
                Debug.LogWarning("[RelicManager] No boss relics available!");
                return null;
            }
            return available[Random.Range(0, available.Count)];
        }

        /// <summary>
        /// 런 종료 시 유물 초기화
        /// </summary>
        public void ResetRelics()
        {
            ownedRelics.Clear();
            Debug.Log("[RelicManager] Relics reset for new run");
        }
    }
}
