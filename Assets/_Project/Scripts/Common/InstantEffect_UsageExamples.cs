using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// InstantEffect 사용 예시 (향후 확장용 참고 코드)
    /// </summary>
    public class InstantEffect_UsageExamples : MonoBehaviour
    {
        /*
        ===============================================
        예시 1: 보상 시스템 (RewardData)
        ===============================================
        전투 승리 보상으로 즉시 효과 제공
        */

        [System.Serializable]
        public class RewardData_Example
        {
            public string rewardName;
            public InstantEffect instantEffect;  // 즉시 효과 재사용!

            public void GiveReward(Player player)
            {
                Debug.Log($"[Reward] {rewardName} 보상 지급");

                if (instantEffect != null && instantEffect.HasEffect())
                {
                    instantEffect.Apply(player, $"Reward: {rewardName}");
                }
            }
        }

        /*
        ===============================================
        예시 2: 이벤트 선택지 (EventChoiceData)
        ===============================================
        이벤트에서 선택지마다 다른 즉시 효과
        */

        [System.Serializable]
        public class EventChoiceData_Example
        {
            public string choiceText;           // "열린다"
            public InstantEffect reward;        // 골드 +50
            public InstantEffect cost;          // HP -10 (음수도 가능)

            public void Select(Player player)
            {
                Debug.Log($"[Event] 선택: {choiceText}");

                // 비용 지불 (음수 효과)
                if (cost != null && cost.HasEffect())
                {
                    cost.Apply(player, "Event Cost");
                }

                // 보상 획득
                if (reward != null && reward.HasEffect())
                {
                    reward.Apply(player, "Event Reward");
                }
            }
        }

        /*
        ===============================================
        예시 3: 상점 아이템 (ShopItemData)
        ===============================================
        상점에서 구매 시 즉시 효과 제공
        */

        [System.Serializable]
        public class ShopItemData_Example
        {
            public string itemName;             // "체력 물약"
            public int price;                   // 25 골드
            public InstantEffect effect;        // HP +30 회복

            public bool Purchase(Player player)
            {
                if (player.Gold < price)
                {
                    Debug.LogWarning($"[Shop] 골드 부족: {price} 필요");
                    return false;
                }

                player.AddGold(-price);  // 골드 차감

                if (effect != null && effect.HasEffect())
                {
                    effect.Apply(player, $"Shop: {itemName}");
                }

                return true;
            }
        }

        /*
        ===============================================
        예시 4: 복합 보상 (MultiReward)
        ===============================================
        여러 InstantEffect를 조합하여 사용
        */

        public void GiveMultipleRewards_Example(Player player)
        {
            // 보상 1: 골드 +50
            var goldReward = new InstantEffect { goldGain = 50 };

            // 보상 2: HP +20
            var healReward = new InstantEffect { healAmount = 20 };

            // 보상 3: 최대 HP +10
            var maxHPReward = new InstantEffect { maxHPIncrease = 10 };

            // 모두 적용
            goldReward.Apply(player, "보상 1");
            healReward.Apply(player, "보상 2");
            maxHPReward.Apply(player, "보상 3");

            // 또는 Combine 사용
            var combinedReward = InstantEffect.Combine(
                InstantEffect.Combine(goldReward, healReward),
                maxHPReward
            );
            combinedReward.Apply(player, "복합 보상");
        }

        /*
        ===============================================
        예시 5: UI 표시
        ===============================================
        InstantEffect 설명 텍스트 생성
        */

        public void ShowEffectUI_Example()
        {
            var effect = new InstantEffect
            {
                healAmount = 20,
                maxHPIncrease = 5,
                goldGain = 30
            };

            string description = effect.GetDescriptionText();
            // 결과: "HP +20 회복, 최대 HP +5, 골드 +30"

            Debug.Log($"[UI] 효과: {description}");
        }

        /*
        ===============================================
        향후 확장 가능한 효과들
        ===============================================
        InstantEffect 클래스에 추가 가능한 필드들:

        public int blockGain = 0;              // 블록 추가 획득
        public int relicGain = 0;              // 유물 추가 획득
        public int maxDefenseIncrease = 0;     // 최대 방어력 증가
        public int strengthGain = 0;           // 영구 힘 증가
        public BlockData specificBlock;        // 특정 블록 획득
        public RelicData specificRelic;        // 특정 유물 획득
        */
    }
}
