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
        HeavyAttack,    // 강공격 (예고 필요)
        Defend,         // 방어
        Buff,           // 버프
        Debuff          // 디버프
    }

    /// <summary>
    /// 적 행동 데이터
    /// </summary>
    [Serializable]
    public class EnemyAction
    {
        public EnemyActionType type;
        public int value;               // 데미지 or 방어력 등
        public bool needsTelegraph;     // 예고 필요 여부
        public float weight;            // 선택 확률 가중치
        public string description;      // UI 표시용

        public EnemyAction(EnemyActionType type, int value, float weight = 1f, bool needsTelegraph = false, string description = "")
        {
            this.type = type;
            this.value = value;
            this.weight = weight;
            this.needsTelegraph = needsTelegraph;
            this.description = string.IsNullOrEmpty(description) ? GetDefaultDescription() : description;
        }

        private string GetDefaultDescription()
        {
            switch (type)
            {
                case EnemyActionType.Attack:
                    return $"공격 {value}";
                case EnemyActionType.HeavyAttack:
                    return $"⚠️ 강공격 {value}";
                case EnemyActionType.Defend:
                    return $"방어 +{value}";
                case EnemyActionType.Buff:
                    return "버프";
                case EnemyActionType.Debuff:
                    return "디버프";
                default:
                    return "알 수 없음";
            }
        }

        public override string ToString()
        {
            return description;
        }
    }
}
