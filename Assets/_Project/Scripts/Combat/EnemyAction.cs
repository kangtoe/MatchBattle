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

        public EnemyAction(EnemyActionType type, int value, float weight = 1f, string designNote = "")
        {
            this.type = type;
            this.value = value;
            this.weight = weight;
            this.designNote = designNote;
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
                    return "버프";
                case EnemyActionType.Debuff:
                    return "디버프";
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
