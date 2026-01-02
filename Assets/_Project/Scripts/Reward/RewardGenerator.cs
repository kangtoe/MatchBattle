using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 보상 생성기 (스테이지 타입별 보상 3개 생성)
    /// </summary>
    public static class RewardGenerator
    {
        // 유물 출현 확률 (기획 문서 기준)
        private const float RELIC_CHANCE_COMBAT = 0.15f;  // 일반 전투: 15%
        private const float RELIC_CHANCE_ELITE = 0.40f;   // 엘리트: 40%
        // 보스: 100% (확정)

        /// <summary>
        /// 스테이지 타입에 따라 보상 3개 생성
        /// </summary>
        public static List<RewardData> Generate(StageType stageType)
        {
            List<RewardData> rewards = new List<RewardData>();
            List<RewardType> availableTypes = new List<RewardType>
            {
                RewardType.Gold,
                RewardType.Heal,
                RewardType.MaxHPUp
            };

            // 보스 전투: 유물 확정 1개
            if (stageType == StageType.Boss)
            {
                RewardData relicReward = TryCreateRelicReward(true);
                if (relicReward != null)
                {
                    rewards.Add(relicReward);
                }
            }
            // 일반/엘리트 전투: 확률적 유물
            else if (stageType == StageType.Combat || stageType == StageType.Elite)
            {
                float relicChance = stageType == StageType.Elite ? RELIC_CHANCE_ELITE : RELIC_CHANCE_COMBAT;
                if (Random.value < relicChance)
                {
                    RewardData relicReward = TryCreateRelicReward(false);
                    if (relicReward != null)
                    {
                        rewards.Add(relicReward);
                    }
                }
            }

            // 나머지 슬롯을 기본 보상으로 채움 (총 3개)
            while (rewards.Count < 3 && availableTypes.Count > 0)
            {
                int index = Random.Range(0, availableTypes.Count);
                RewardType type = availableTypes[index];
                availableTypes.RemoveAt(index);

                rewards.Add(CreateReward(type, stageType));
            }

            return rewards;
        }

        /// <summary>
        /// 유물 보상 생성 시도
        /// </summary>
        private static RewardData TryCreateRelicReward(bool isBossRelic)
        {
            if (RelicManager.Instance == null)
            {
                Debug.LogWarning("[RewardGenerator] RelicManager not found!");
                return null;
            }

            RelicData relic = isBossRelic
                ? RelicManager.Instance.GetRandomBossRelic()
                : RelicManager.Instance.GetRandomCommonRelic();

            if (relic == null)
            {
                Debug.Log("[RewardGenerator] No available relics to offer");
                return null;
            }

            return new RewardData(relic);
        }

        /// <summary>
        /// 보상 타입과 스테이지 타입에 따라 보상 데이터 생성
        /// </summary>
        private static RewardData CreateReward(RewardType rewardType, StageType stageType)
        {
            switch (rewardType)
            {
                case RewardType.Gold:
                    return CreateGoldReward(stageType);
                case RewardType.Heal:
                    return CreateHealReward(stageType);
                case RewardType.MaxHPUp:
                    return CreateMaxHPUpReward(stageType);
                default:
                    return CreateGoldReward(stageType);
            }
        }

        /// <summary>
        /// 골드 보상 생성
        /// </summary>
        private static RewardData CreateGoldReward(StageType stageType)
        {
            int minGold, maxGold;

            switch (stageType)
            {
                case StageType.Elite:
                    minGold = 25;
                    maxGold = 40;
                    break;
                case StageType.Boss:
                    minGold = 50;
                    maxGold = 75;
                    break;
                default: // Combat
                    minGold = 15;
                    maxGold = 20;
                    break;
            }

            int goldAmount = Random.Range(minGold, maxGold + 1);
            return new RewardData(
                RewardType.Gold,
                goldAmount,
                $"{goldAmount} 골드",
                "골드 획득"
            );
        }

        /// <summary>
        /// HP 회복 보상 생성
        /// </summary>
        private static RewardData CreateHealReward(StageType stageType)
        {
            int minHeal, maxHeal;
            string description;

            switch (stageType)
            {
                case StageType.Elite:
                    minHeal = 25;
                    maxHeal = 35;
                    description = "HP 회복";
                    break;
                case StageType.Boss:
                    // 보스는 전체 회복 (value = -1로 표시)
                    return new RewardData(
                        RewardType.Heal,
                        -1,
                        "전체 회복",
                        "HP 완전 회복"
                    );
                default: // Combat
                    minHeal = 15;
                    maxHeal = 25;
                    description = "HP 회복";
                    break;
            }

            int healAmount = Random.Range(minHeal, maxHeal + 1);
            return new RewardData(
                RewardType.Heal,
                healAmount,
                $"+{healAmount} HP",
                description
            );
        }

        /// <summary>
        /// 최대 HP 증가 보상 생성
        /// </summary>
        private static RewardData CreateMaxHPUpReward(StageType stageType)
        {
            int increaseAmount;

            switch (stageType)
            {
                case StageType.Elite:
                    increaseAmount = 8;
                    break;
                case StageType.Boss:
                    increaseAmount = 10;
                    break;
                default: // Combat
                    increaseAmount = 5;
                    break;
            }

            return new RewardData(
                RewardType.MaxHPUp,
                increaseAmount,
                $"최대 HP +{increaseAmount}",
                "최대 HP 영구 증가"
            );
        }
    }
}
