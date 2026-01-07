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

            // === OnAcquire 유물 (획득 시 즉시 발동) ===

            // 1. 생명의 씨앗 (HP +10)
            CreateRelic(path, new RelicConfig
            {
                id = "seed_of_life",
                displayName = "생명의 씨앗",
                description = "유물 획득 즉시 HP를 10 회복합니다.",
                rarity = RelicRarity.Common,
                instantEffect = new InstantEffect { healAmount = 10 }
            });

            // 2. 강인한 심장 (최대 HP +5)
            CreateRelic(path, new RelicConfig
            {
                id = "sturdy_heart",
                displayName = "강인한 심장",
                description = "유물 획득 즉시 최대 HP가 5 증가합니다.",
                rarity = RelicRarity.Common,
                instantEffect = new InstantEffect { maxHPIncrease = 5 }
            });

            // 3. 황금 주머니 (골드 +25)
            CreateRelic(path, new RelicConfig
            {
                id = "golden_pouch",
                displayName = "황금 주머니",
                description = "유물 획득 즉시 골드를 25 획득합니다.",
                rarity = RelicRarity.Common,
                instantEffect = new InstantEffect { goldGain = 25 }
            });

            // === OnBattleStart 유물 (전투 시작 시 발동) ===

            // 4. 전사의 문장 (STR +2)
            CreateRelic(path, new RelicConfig
            {
                id = "warriors_emblem",
                displayName = "전사의 문장",
                description = "전투 시작 시 공격력이 2 증가합니다.",
                rarity = RelicRarity.Common,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.STR, 2, TargetType.Self, RelicTriggerType.OnBattleStart)
                }
            });

            // 5. 철벽의 갑옷 (금속화 +10)
            CreateRelic(path, new RelicConfig
            {
                id = "iron_armor",
                displayName = "철벽의 갑옷",
                description = "전투 시작 시 금속화 10을 얻습니다 (받는 피해 -10).",
                rarity = RelicRarity.Common,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.PLATED, 10, TargetType.Self, RelicTriggerType.OnBattleStart)
                }
            });

            // 6. 회복의 부적 (HP +5)
            CreateRelic(path, new RelicConfig
            {
                id = "amulet_of_healing",
                displayName = "회복의 부적",
                description = "유물 획득 즉시 HP를 5 회복합니다.",
                rarity = RelicRarity.Common,
                instantEffect = new InstantEffect { healAmount = 5 }
            });

            // 7. 독사의 반지 (적에게 POISON +3)
            CreateRelic(path, new RelicConfig
            {
                id = "vipers_ring",
                displayName = "독사의 반지",
                description = "전투 시작 시 모든 적이 독에 걸립니다.",
                rarity = RelicRarity.Boss,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.POISON, 3, TargetType.EnemyAll, RelicTriggerType.OnBattleStart)
                }
            });

            // 8. 재생의 심장 (REGEN +3)
            CreateRelic(path, new RelicConfig
            {
                id = "heart_of_regeneration",
                displayName = "재생의 심장",
                description = "전투 시작 시 3턴간 HP가 회복됩니다.",
                rarity = RelicRarity.Boss,
                effects = new StatusEffectConfig[]
                {
                    new StatusEffectConfig(StatusEffectType.REGEN, 3, TargetType.Self, RelicTriggerType.OnBattleStart)
                }
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RelicDataCreator] MVP 유물 8종 생성 완료!");
        }

        private static void CreateRelic(string path, RelicConfig config)
        {
            RelicData relic = ScriptableObject.CreateInstance<RelicData>();

            relic.id = config.id;
            relic.displayName = config.displayName;
            relic.description = config.description;
            relic.rarity = config.rarity;
            relic.effects = config.effects ?? new StatusEffectConfig[0];
            relic.instantEffect = config.instantEffect;

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
            public StatusEffectConfig[] effects;
            public InstantEffect instantEffect;
        }
    }
}
#endif
