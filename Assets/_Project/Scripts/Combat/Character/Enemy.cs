using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 적 데이터 및 AI 관리
    /// </summary>
    [Serializable]
    public class Enemy : Character
    {
        // AI 패턴
        public List<EnemyAction> actionPool = new List<EnemyAction>();
        public EnemyAction nextAction;
        public EnemyAction currentAction;

        // 편의를 위한 EnemyName 프로퍼티 (베이스 Name과 동일)
        public string EnemyName
        {
            get => Name;
            set => Name = value;
        }

        // 생성자
        public Enemy(string name, int maxHP, int maxDefense = 0, List<EnemyAction> actions = null)
            : base(name, maxHP, maxDefense)
        {
            if (actions != null)
            {
                actionPool = new List<EnemyAction>(actions);
            }
        }

        // ===========================================
        // AI - 행동 선택
        // ===========================================

        /// <summary>
        /// 다음 행동 선택
        /// </summary>
        public EnemyAction SelectNextAction()
        {
            if (actionPool.Count == 0)
            {
                Debug.LogWarning($"[{EnemyName}] No actions available!");
                return new EnemyAction(EnemyActionType.Attack, 5, 1f, "Fallback action");
            }

            // 가중치 기반 랜덤 선택
            float totalWeight = 0f;
            foreach (var action in actionPool)
            {
                totalWeight += action.weight;
            }

            float rand = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var action in actionPool)
            {
                cumulative += action.weight;
                if (rand <= cumulative)
                {
                    nextAction = action;
                    return action;
                }
            }

            // Fallback
            nextAction = actionPool[0];
            return nextAction;
        }
    }
}
