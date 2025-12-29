using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MatchBattle
{
    /// <summary>
    /// 보상 관리자 (보상 생성 및 적용)
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        public static RewardManager Instance { get; private set; }

        // 보상 선택 완료 이벤트 (CombatManager에서 구독)
        public UnityEvent OnRewardCompleted = new UnityEvent();

        // 현재 생성된 보상 목록
        private List<RewardData> currentRewards;

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
        /// 스테이지 타입에 맞는 보상 생성
        /// </summary>
        public List<RewardData> GenerateRewards(StageType stageType)
        {
            currentRewards = RewardGenerator.Generate(stageType);

            Debug.Log($"[Reward] Generated {currentRewards.Count} rewards for {stageType}:");
            foreach (var reward in currentRewards)
            {
                Debug.Log($"  - {reward.displayName}: {reward.description}");
            }

            return currentRewards;
        }

        /// <summary>
        /// 선택한 보상 적용
        /// </summary>
        public void ApplyReward(RewardData reward, Player player)
        {
            if (reward == null || player == null)
            {
                Debug.LogWarning("[Reward] Invalid reward or player!");
                OnRewardCompleted?.Invoke();
                return;
            }

            switch (reward.type)
            {
                case RewardType.Gold:
                    player.AddGold(reward.value);
                    Debug.Log($"[Reward] Applied: +{reward.value} Gold");
                    break;

                case RewardType.Heal:
                    if (reward.value == -1)
                    {
                        // 전체 회복
                        int healAmount = player.MaxHP - player.CurrentHP;
                        player.Heal(healAmount);
                        Debug.Log($"[Reward] Applied: Full Heal (+{healAmount} HP)");
                    }
                    else
                    {
                        player.Heal(reward.value);
                        Debug.Log($"[Reward] Applied: +{reward.value} HP");
                    }
                    break;

                case RewardType.MaxHPUp:
                    int oldMaxHP = player.MaxHP;
                    player.MaxHP += reward.value;
                    player.CurrentHP += reward.value; // 증가분만큼 현재 HP도 증가
                    Debug.Log($"[Reward] Applied: Max HP {oldMaxHP} → {player.MaxHP}");
                    break;
            }

            currentRewards = null;
            OnRewardCompleted?.Invoke();
        }

        /// <summary>
        /// 보상 건너뛰기
        /// </summary>
        public void SkipReward()
        {
            Debug.Log("[Reward] Reward skipped by player");
            currentRewards = null;
            OnRewardCompleted?.Invoke();
        }
    }
}
