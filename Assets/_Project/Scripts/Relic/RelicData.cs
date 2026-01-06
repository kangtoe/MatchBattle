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

        [Header("패시브 효과 (OnBattleStart/OnTurnStart)")]
        [Tooltip("전투 중 반복 발동하는 상태 효과들")]
        public StatusEffectConfig[] effects = new StatusEffectConfig[0];

        [Header("즉각 효과 (OnAcquire)")]
        [Tooltip("획득 시 즉시 발동하는 효과")]
        public InstantEffect instantEffect;

        /// <summary>
        /// 유물 패시브 효과 발동 (OnBattleStart/OnTurnStart)
        /// </summary>
        public void ApplyEffect(Player player, Enemy[] enemies)
        {
            Debug.Log($"[Relic] {displayName} 효과 발동!");

            // 즉시 효과 (전투 시작 시 HP 회복 등)
            if (instantEffect != null && instantEffect.HasEffect())
            {
                instantEffect.Apply(player, $"Relic: {displayName}");
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
