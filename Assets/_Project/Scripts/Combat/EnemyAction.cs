using System;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 적 행동 타입
    /// </summary>
    public enum EnemyActionType
    {
        Attack,         // 일반 공격
        Defend,         // 방어
        Buff,           // 버프
        Debuff          // 디버프
    }

    /// <summary>
    /// 상태 효과 설정 (직렬화 가능)
    /// 단일 숫자 원칙: 하나의 효과는 숫자를 하나만 가짐
    /// </summary>
    [Serializable]
    public class StatusEffectConfig
    {
        public StatusEffectType type;

        [Tooltip("효과 수치 (타입에 따라 의미가 다름: Stack/Decrement=값, Duration=턴수)")]
        public int value;

        public StatusEffectConfig(StatusEffectType type, int value)
        {
            this.type = type;
            this.value = value;
        }

        public StatusEffect ToStatusEffect()
        {
            // 타입에 따라 value와 duration 자동 결정
            switch (type)
            {
                case StatusEffectType.STR:
                case StatusEffectType.PLATED:
                case StatusEffectType.EXHAUSTED:
                case StatusEffectType.REGEN:
                case StatusEffectType.POISON:
                    // 스택형/감소형: value = 효과 수치, duration = -1 (영구 또는 자동 감소)
                    return new StatusEffect(type, value, -1);

                case StatusEffectType.WEAK:
                case StatusEffectType.VULNERABLE:
                    // 지속형: value = 턴수, StatusEffect의 value는 0 (효과는 고정)
                    return new StatusEffect(type, 0, value);

                default:
                    return new StatusEffect(type, value, -1);
            }
        }
    }

    /// <summary>
    /// 적 행동 데이터 (모든 행동은 플레이어에게 예고됨)
    /// </summary>
    [Serializable]
    public class EnemyAction
    {
        public EnemyActionType type;
        public int value;               // 데미지 or 방어력 등
        public float weight;            // 선택 확률 가중치

        [Tooltip("개발자/기획자용 메모 (UI 표시와 무관)")]
        public string designNote;       // 개발자/기획자용 메모

        // 상태 효과 리스트 (Buff/Debuff 타입일 때 사용)
        [Tooltip("Buff/Debuff 행동 시 적용할 상태 효과들")]
        public StatusEffectConfig[] statusEffects;

        // 기본 생성자 (Attack, Defend용)
        public EnemyAction(EnemyActionType type, int value, float weight = 1f, string designNote = "")
        {
            this.type = type;
            this.value = value;
            this.weight = weight;
            this.designNote = designNote;
            this.statusEffects = new StatusEffectConfig[0];
        }

        // 상태 효과 생성자 - 단일 효과
        public EnemyAction(EnemyActionType type, StatusEffectType effectType, int effectValue, float weight = 1f, string designNote = "")
        {
            this.type = type;
            this.value = effectValue; // UI 표시용
            this.weight = weight;
            this.designNote = designNote;
            this.statusEffects = new StatusEffectConfig[]
            {
                new StatusEffectConfig(effectType, effectValue)
            };
        }

        // 상태 효과 생성자 - 다중 효과 (일시적 강화 등)
        public EnemyAction(EnemyActionType type, StatusEffectConfig[] effects, float weight = 1f, string designNote = "")
        {
            this.type = type;
            this.value = 0; // 다중 효과는 value 없음
            this.weight = weight;
            this.designNote = designNote;
            this.statusEffects = effects;
        }

        /// <summary>
        /// UI에 표시할 텍스트 (type + value 자동 생성)
        /// </summary>
        public string GetDisplayText()
        {
            switch (type)
            {
                case EnemyActionType.Attack:
                    return $"공격 {value}";
                case EnemyActionType.Defend:
                    return $"방어 +{value}";
                case EnemyActionType.Buff:
                case EnemyActionType.Debuff:
                    {
                        if (statusEffects == null || statusEffects.Length == 0)
                        {
                            return type == EnemyActionType.Buff ? "버프" : "디버프";
                        }

                        // 모든 상태 효과 아이콘 표시
                        string text = "";
                        foreach (var effectConfig in statusEffects)
                        {
                            var tempEffect = effectConfig.ToStatusEffect();
                            text += $"{tempEffect.GetIcon()}{effectConfig.value} ";
                        }
                        return text.TrimEnd();
                    }
                default:
                    return "알 수 없음";
            }
        }

        public override string ToString()
        {
            return GetDisplayText();
        }
    }
}
