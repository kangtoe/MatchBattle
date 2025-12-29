using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 보상 생성기 (스테이지 타입별 보상 3개 생성)
    /// </summary>
    public static class RewardGenerator
    {
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

            // 3개 뽑기 (중복 없이)
            for (int i = 0; i < 3 && availableTypes.Count > 0; i++)
            {
                int index = Random.Range(0, availableTypes.Count);
                RewardType type = availableTypes[index];
                availableTypes.RemoveAt(index);

                rewards.Add(CreateReward(type, stageType));
            }

            return rewards;
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
