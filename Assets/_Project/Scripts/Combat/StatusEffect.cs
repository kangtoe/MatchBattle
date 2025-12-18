using System;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 상태 효과 카테고리 (타입 기반 분류)
    /// </summary>
    public enum StatusEffectCategory
    {
        Stack,      // 스택형: 수치가 효과의 크기, 영구 지속 (STR, PLATED, EXHAUSTED)
        Decrement,  // 감소형: 매 턴 효과 발동 후 수치 감소 (REGEN, POISON)
        Duration    // 지속형: 고정 효과, 매 턴 지속시간 감소 (WEAK, VULNERABLE)
    }

    /// <summary>
    /// 상태 효과 타입
    /// </summary>
    public enum StatusEffectType
    {
        STR,        // 힘 (±N, 영구, 공격력 증감)
        PLATED,     // 금속화 (받는 데미지 -N, 영구, 소모 안 됨)
        REGEN,      // 재생 (턴마다 HP 회복, 1씩 감소)
        POISON,     // 독 (턴마다 HP 감소, 1씩 감소)
        WEAK,       // 약화 (공격력 -N%)
        VULNERABLE, // 취약 (받는 데미지 +N%)
        EXHAUSTED   // 탈진 (턴 종료 시 힘 감소)
    }

    /// <summary>
    /// 상태 효과 데이터
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public int value;           // 효과 수치 (힘 ±N, 독 N, 철갑 N 등)
        public int duration;        // 지속 시간 (-1 = 영구)

        public StatusEffect(StatusEffectType type, int value, int duration = -1)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
        }

        /// <summary>
        /// 상태 효과의 카테고리 반환
        /// </summary>
        public StatusEffectCategory GetCategory()
        {
            switch (type)
            {
                case StatusEffectType.STR:
                case StatusEffectType.PLATED:
                case StatusEffectType.EXHAUSTED:
                    return StatusEffectCategory.Stack;

                case StatusEffectType.REGEN:
                case StatusEffectType.POISON:
                    return StatusEffectCategory.Decrement;

                case StatusEffectType.WEAK:
                case StatusEffectType.VULNERABLE:
                    return StatusEffectCategory.Duration;

                default:
                    return StatusEffectCategory.Stack;
            }
        }

        /// <summary>
        /// UI 표시용 아이콘
        /// </summary>
        public string GetIcon()
        {
            switch (type)
            {
                case StatusEffectType.STR:
                    return value >= 0 ? "💪" : "💪";
                case StatusEffectType.PLATED:
                    return "🛡️";
                case StatusEffectType.REGEN:
                    return "✚";
                case StatusEffectType.POISON:
                    return "💀";
                case StatusEffectType.WEAK:
                    return "↓";
                case StatusEffectType.VULNERABLE:
                    return "⚠️";
                case StatusEffectType.EXHAUSTED:
                    return "📉";
                default:
                    return "?";
            }
        }

        /// <summary>
        /// UI 표시용 문자열
        /// </summary>
        public string GetDisplayText()
        {
            string icon = GetIcon();

            switch (type)
            {
                case StatusEffectType.STR:
                    return $"{icon}{value}";
                case StatusEffectType.PLATED:
                case StatusEffectType.REGEN:
                case StatusEffectType.POISON:
                case StatusEffectType.EXHAUSTED:
                    return $"{icon}{value}";
                case StatusEffectType.WEAK:
                case StatusEffectType.VULNERABLE:
                    return duration > 0 ? $"{icon}{duration}" : $"{icon}";
                default:
                    return $"{icon}{value}";
            }
        }

        /// <summary>
        /// 상태 효과 설명
        /// </summary>
        public string GetDescription()
        {
            switch (type)
            {
                case StatusEffectType.STR:
                    return value >= 0
                        ? $"힘 +{value} (공격력 증가)"
                        : $"힘 {value} (공격력 감소)";
                case StatusEffectType.PLATED:
                    return $"금속화 {value} (받는 데미지 -{value}, 영구)";
                case StatusEffectType.REGEN:
                    return $"재생 {value} (턴마다 HP +{value}, {value}씩 감소)";
                case StatusEffectType.POISON:
                    return $"독 {value} (턴마다 HP -{value}, {value}씩 감소)";
                case StatusEffectType.WEAK:
                    return $"약화 (공격력 -{value}%, {duration}턴)";
                case StatusEffectType.VULNERABLE:
                    return $"취약 (받는 데미지 +{value}%, {duration}턴)";
                case StatusEffectType.EXHAUSTED:
                    return $"탈진 {value} (턴 종료 시 힘 -{value})";
                default:
                    return "알 수 없는 효과";
            }
        }

        public override string ToString()
        {
            return GetDisplayText();
        }
    }
}
