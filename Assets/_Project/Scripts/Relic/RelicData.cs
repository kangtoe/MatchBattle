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
        [Tooltip("Inspector 하단의 'Generate Description' 버튼으로 자동 생성 가능")]
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

        /// <summary>
        /// 효과 기반 설명 반환 (런타임용)
        /// </summary>
        public string GetDescription()
        {
            // description 필드가 비어있지 않으면 그대로 반환 (커스텀 설명 또는 OnValidate로 생성된 설명)
            if (!string.IsNullOrWhiteSpace(description))
                return description;

            // 런타임에서 description이 비어있는 경우 (빌드 후 등)
            return GenerateDescription();
        }

        /// <summary>
        /// 트리거 타입 설명
        /// </summary>
        private string GetTriggerText()
        {
            switch (triggerType)
            {
                case RelicTriggerType.OnAcquire:
                    return "[획득 시]";
                case RelicTriggerType.OnBattleStart:
                    return "[전투 시작 시]";
                case RelicTriggerType.OnTurnStart:
                    return "[턴 시작 시]";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 타겟 타입 설명
        /// </summary>
        private string GetTargetText(TargetType target)
        {
            switch (target)
            {
                case TargetType.Self:
                    return "자신";
                case TargetType.EnemyFront:
                    return "전방 적";
                case TargetType.EnemyBack:
                    return "후방 적";
                case TargetType.EnemyRandom:
                    return "랜덤 적";
                case TargetType.EnemyAll:
                    return "모든 적";
                case TargetType.Player:
                    return "플레이어";
                default:
                    return "대상";
            }
        }

        /// <summary>
        /// 효과 기반 설명 생성 (Editor 버튼에서 호출)
        /// </summary>
        public string GenerateDescription()
        {
            var parts = new System.Collections.Generic.List<string>();

            // 트리거 시점
            string trigger = GetTriggerText();
            if (!string.IsNullOrEmpty(trigger))
                parts.Add(trigger);

            // 즉각 효과
            if (instantEffect != null && instantEffect.HasEffect())
            {
                parts.Add(instantEffect.GetDescriptionText());
            }

            // 패시브 효과들
            if (effects != null && effects.Length > 0)
            {
                foreach (var effectConfig in effects)
                {
                    StatusEffect tempEffect = effectConfig.ToStatusEffect();
                    string effectDesc = tempEffect.GetDescription();
                    string targetDesc = GetTargetText(effectConfig.target);

                    parts.Add($"{effectDesc} ({targetDesc})");
                }
            }

            return parts.Count > 0 ? string.Join("\n", parts) : "효과 없음";
        }
    }
}
