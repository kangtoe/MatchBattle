using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 유물 데이터 (ScriptableObject)
    /// StatusEffectConfig를 재사용하여 다중 효과 지원
    /// </summary>
    [CreateAssetMenu(fileName = "New Relic", menuName = "MatchBattle/Relic Data")]
    public class RelicData : ScriptableObject
    {
        [Header("기본 정보")]
        public string id;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("등급")]
        public RelicRarity rarity = RelicRarity.Common;

        [Header("트리거")]
        public RelicTriggerType triggerType = RelicTriggerType.OnBattleStart;

        [Header("효과 목록")]
        [Tooltip("유물이 발동할 효과들 (다중 효과 지원)")]
        public StatusEffectConfig[] effects = new StatusEffectConfig[0];

        [Header("HP 회복 (추가 효과)")]
        [Tooltip("0보다 크면 HP 회복 효과도 발동")]
        public int healAmount = 0;

        /// <summary>
        /// 유물 효과 발동
        /// </summary>
        public void ApplyEffect(Player player, Enemy[] enemies)
        {
            Debug.Log($"[Relic] {displayName} 효과 발동!");

            // HP 회복 효과
            if (healAmount > 0)
            {
                player.Heal(healAmount);
                Debug.Log($"[Relic] {displayName}: HP +{healAmount} 회복");
            }

            // 상태 효과들 적용
            if (effects == null || effects.Length == 0)
                return;

            foreach (var effectConfig in effects)
            {
                ApplyStatusEffect(effectConfig, player, enemies);
            }
        }

        /// <summary>
        /// 개별 상태 효과 적용
        /// </summary>
        private void ApplyStatusEffect(StatusEffectConfig config, Player player, Enemy[] enemies)
        {
            StatusEffect effect = config.ToStatusEffect();

            switch (config.target)
            {
                case TargetType.Self:
                    player.AddStatusEffect(effect);
                    Debug.Log($"[Relic] {displayName}: 플레이어에게 {config.type}({config.value}) 적용");
                    break;

                case TargetType.EnemyFront:
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (enemies[i] != null && enemies[i].IsAlive())
                        {
                            enemies[i].AddStatusEffect(config.ToStatusEffect());
                            Debug.Log($"[Relic] {displayName}: {enemies[i].Name}에게 {config.type}({config.value}) 적용");
                            break;
                        }
                    }
                    break;

                case TargetType.EnemyBack:
                    for (int i = enemies.Length - 1; i >= 0; i--)
                    {
                        if (enemies[i] != null && enemies[i].IsAlive())
                        {
                            enemies[i].AddStatusEffect(config.ToStatusEffect());
                            Debug.Log($"[Relic] {displayName}: {enemies[i].Name}에게 {config.type}({config.value}) 적용");
                            break;
                        }
                    }
                    break;

                case TargetType.EnemyRandom:
                    var livingEnemies = new List<Enemy>();
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null && enemy.IsAlive())
                            livingEnemies.Add(enemy);
                    }
                    if (livingEnemies.Count > 0)
                    {
                        var randomEnemy = livingEnemies[Random.Range(0, livingEnemies.Count)];
                        randomEnemy.AddStatusEffect(config.ToStatusEffect());
                        Debug.Log($"[Relic] {displayName}: {randomEnemy.Name}에게 {config.type}({config.value}) 적용");
                    }
                    break;

                case TargetType.EnemyAll:
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null && enemy.IsAlive())
                        {
                            enemy.AddStatusEffect(config.ToStatusEffect());
                            Debug.Log($"[Relic] {displayName}: {enemy.Name}에게 {config.type}({config.value}) 적용");
                        }
                    }
                    break;

                case TargetType.Player:
                    // 유물은 플레이어 전용이므로 Player 타겟은 사용하지 않음
                    Debug.LogWarning($"[Relic] {displayName}: 유물에서 Player 타겟은 지원하지 않습니다. Self를 사용하세요.");
                    break;
            }
        }
    }
}
