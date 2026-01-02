#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MatchBattle
{
    /// <summary>
    /// MVP 유물 에셋 생성 유틸리티 (에디터 전용)
    /// </summary>
    public static class RelicDataCreator
    {
        [MenuItem("MatchBattle/Create MVP Relics")]
        public static void CreateMVPRelics()
        {
            string path = "Assets/_Project/Data/Relics/";

            // 폴더 확인
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Relics"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Relics");
            }

            // 1. 전사의 문장 (STR +2)
            CreateRelic(path, new RelicConfig
            {
                id = "warriors_emblem",
                displayName = "전사의 문장",
                description = "전투 시작 시 공격력이 2 증가합니다.",
                rarity = RelicRarity.Common,
                triggerType = RelicTriggerType.OnBattleStart,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.STR, 2, TargetType.Self)
                }
            });

            // 2. 철벽의 갑옷 (PLATED +5)
            CreateRelic(path, new RelicConfig
            {
                id = "iron_armor",
                displayName = "철벽의 갑옷",
                description = "전투 시작 시 받는 데미지가 5 감소합니다.",
                rarity = RelicRarity.Common,
                triggerType = RelicTriggerType.OnBattleStart,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.PLATED, 5, TargetType.Self)
                }
            });

            // 3. 회복의 부적 (HP +5)
            CreateRelic(path, new RelicConfig
            {
                id = "amulet_of_healing",
                displayName = "회복의 부적",
                description = "전투 시작 시 HP를 5 회복합니다.",
                rarity = RelicRarity.Common,
                triggerType = RelicTriggerType.OnBattleStart,
                healAmount = 5
            });

            // 4. 독사의 반지 (적에게 POISON +3)
            CreateRelic(path, new RelicConfig
            {
                id = "vipers_ring",
                displayName = "독사의 반지",
                description = "전투 시작 시 모든 적이 독에 걸립니다.",
                rarity = RelicRarity.Boss,
                triggerType = RelicTriggerType.OnBattleStart,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.POISON, 3, TargetType.EnemyAll)
                }
            });

            // 5. 재생의 심장 (REGEN +3)
            CreateRelic(path, new RelicConfig
            {
                id = "heart_of_regeneration",
                displayName = "재생의 심장",
                description = "전투 시작 시 3턴간 HP가 회복됩니다.",
                rarity = RelicRarity.Boss,
                triggerType = RelicTriggerType.OnBattleStart,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.REGEN, 3, TargetType.Self)
                }
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RelicDataCreator] MVP 유물 5종 생성 완료!");
        }

        private static void CreateRelic(string path, RelicConfig config)
        {
            RelicData relic = ScriptableObject.CreateInstance<RelicData>();

            relic.id = config.id;
            relic.displayName = config.displayName;
            relic.description = config.description;
            relic.rarity = config.rarity;
            relic.triggerType = config.triggerType;
            relic.effects = config.effects ?? new StatusEffectConfig[0];
            relic.healAmount = config.healAmount;

            string assetPath = $"{path}{config.id}.asset";
            AssetDatabase.CreateAsset(relic, assetPath);

            Debug.Log($"[RelicDataCreator] Created: {config.displayName} at {assetPath}");
        }

        private struct RelicConfig
        {
            public string id;
            public string displayName;
            public string description;
            public RelicRarity rarity;
            public RelicTriggerType triggerType;
            public StatusEffectConfig[] effects;
            public int healAmount;
        }
    }
}
#endif
