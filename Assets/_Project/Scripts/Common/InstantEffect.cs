using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 즉시 발동 효과 (유물, 보상, 이벤트 등에서 재사용 가능)
    /// </summary>
    [System.Serializable]
    public class InstantEffect
    {
        [Header("HP 관련")]
        [Tooltip("즉시 HP 회복량")]
        public int healAmount = 0;

        [Tooltip("최대 HP 증가량 (현재 HP도 함께 증가)")]
        public int maxHPIncrease = 0;

        [Header("자원")]
        [Tooltip("골드 획득량")]
        public int goldGain = 0;

        // TODO: 향후 확장 가능한 효과들
        // public int blockGain = 0;          // 블록 획득
        // public int relicGain = 0;          // 유물 획득
        // public int maxDefenseIncrease = 0; // 최대 방어력 증가

        /// <summary>
        /// 즉시 효과 적용
        /// </summary>
        public void Apply(Player player, string source = "")
        {
            if (!HasEffect())
                return;

            string logPrefix = string.IsNullOrEmpty(source) ? "[InstantEffect]" : $"[{source}]";

            // HP 회복
            if (healAmount > 0)
            {
                player.Heal(healAmount);
                Debug.Log($"{logPrefix} HP +{healAmount} 회복 (현재: {player.CurrentHP}/{player.MaxHP})");
            }

            // 최대 HP 증가
            if (maxHPIncrease > 0)
            {
                player.IncreaseMaxHP(maxHPIncrease);
                Debug.Log($"{logPrefix} 최대 HP +{maxHPIncrease} (현재: {player.MaxHP})");
            }

            // 골드 획득
            if (goldGain > 0)
            {
                player.AddGold(goldGain);
                Debug.Log($"{logPrefix} 골드 +{goldGain} (현재: {player.Gold})");
            }
        }

        /// <summary>
        /// 효과가 하나라도 있는지 확인
        /// </summary>
        public bool HasEffect()
        {
            return healAmount > 0 || maxHPIncrease > 0 || goldGain > 0;
        }

        /// <summary>
        /// 효과 설명 텍스트 생성 (UI용)
        /// </summary>
        public string GetDescriptionText()
        {
            if (!HasEffect())
                return "";

            var effects = new System.Collections.Generic.List<string>();

            if (healAmount > 0)
                effects.Add($"HP +{healAmount} 회복");

            if (maxHPIncrease > 0)
                effects.Add($"최대 HP +{maxHPIncrease}");

            if (goldGain > 0)
                effects.Add($"골드 +{goldGain}");

            return string.Join(", ", effects);
        }

        /// <summary>
        /// 두 효과 합치기 (여러 효과를 누적할 때 사용)
        /// </summary>
        public static InstantEffect Combine(InstantEffect a, InstantEffect b)
        {
            if (a == null) return b;
            if (b == null) return a;

            return new InstantEffect
            {
                healAmount = a.healAmount + b.healAmount,
                maxHPIncrease = a.maxHPIncrease + b.maxHPIncrease,
                goldGain = a.goldGain + b.goldGain
            };
        }
    }
}
