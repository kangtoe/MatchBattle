using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "MatchBattle/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Enemy Info")]
        public string displayName;
        public Sprite sprite;

        [Header("Stats")]
        public int maxHP;
        public int maxDefense = 99;

        [Header("Initial Status Effects")]
        public StatusEffectConfig[] initialEffects;

        [Header("AI Pattern")]
        [Tooltip("적의 행동 패턴 (순환)")]
        public EnemyAction[] actionPattern;

        /// <summary>
        /// EnemyData로부터 Enemy 인스턴스를 생성합니다.
        /// </summary>
        public Enemy CreateEnemy()
        {
            // 행동 패턴 리스트 생성
            List<EnemyAction> actions = null;
            if (actionPattern != null && actionPattern.Length > 0)
            {
                actions = new List<EnemyAction>(actionPattern);
            }

            // Enemy 생성
            Enemy enemy = new Enemy(displayName, maxHP, maxDefense, actions);

            // 초기 상태 효과 적용
            if (initialEffects != null)
            {
                foreach (var effectConfig in initialEffects)
                {
                    enemy.AddStatusEffect(effectConfig.ToStatusEffect());
                }
            }

            return enemy;
        }
    }
}
