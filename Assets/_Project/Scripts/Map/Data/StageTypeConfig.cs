using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 스테이지 타입별 생성 설정
    /// </summary>
    [System.Serializable]
    public class StageTypeSpawnConfig
    {
        public StageType stageType;
        [Range(0, 100)]
        public int spawnWeight;             // 생성 가중치 (백분률, 0-100)
        public int minLevelIndex = 1;       // 최소 출현 레벨 (1, 2, 3...)
    }

    [CreateAssetMenu(fileName = "StageTypeConfig", menuName = "MatchBattle/StageTypeConfig")]
    public class StageTypeConfig : ScriptableObject
    {
        [Header("Spawn Weights")]
        [Tooltip("스테이지 타입별 생성 확률 설정")]
        public List<StageTypeSpawnConfig> spawnConfigs = new List<StageTypeSpawnConfig>();

        [Header("Probability Info (Auto-Updated)")]
        [SerializeField, TextArea(5, 10)]
        private string probabilityInfo = "";

        void OnValidate()
        {
            UpdateProbabilityInfo();
        }

        private void UpdateProbabilityInfo()
        {
            if (spawnConfigs == null || spawnConfigs.Count == 0)
            {
                probabilityInfo = "설정된 스테이지 타입이 없습니다.";
                return;
            }

            int total = spawnConfigs.Sum(c => c.spawnWeight);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"■ 총 가중치 합계: {total}");
            sb.AppendLine();

            if (total == 100)
            {
                sb.AppendLine("✓ 권장 합계(100) - 최적 상태");
            }
            else if (total > 0)
            {
                sb.AppendLine($"⚠ 현재 합계 {total} (권장: 100)");
                foreach (var config in spawnConfigs)
                {
                    float probability = (float)config.spawnWeight / total * 100f;
                    sb.AppendLine($"  {config.stageType}: {config.spawnWeight}/{total} = {probability:F1}%");
                }
            }
            else
            {
                sb.AppendLine("✗ 오류: 총 가중치가 0입니다!");
            }

            probabilityInfo = sb.ToString();
        }

        void Reset()
        {
            // 기본값 설정
            spawnConfigs = new List<StageTypeSpawnConfig>
            {
                new StageTypeSpawnConfig { stageType = StageType.Combat, spawnWeight = 60, minLevelIndex = 1 },
                new StageTypeSpawnConfig { stageType = StageType.Elite, spawnWeight = 15, minLevelIndex = 2 },
                new StageTypeSpawnConfig { stageType = StageType.Shop, spawnWeight = 10, minLevelIndex = 1 },
                new StageTypeSpawnConfig { stageType = StageType.Rest, spawnWeight = 10, minLevelIndex = 1 },
                new StageTypeSpawnConfig { stageType = StageType.Event, spawnWeight = 5, minLevelIndex = 1 }
            };
        }

        /// <summary>
        /// 특정 레벨에서 사용 가능한 스테이지 타입 중 하나를 랜덤 선택
        /// </summary>
        /// <param name="levelIndex">현재 레벨 (1, 2, 3...)</param>
        public StageType GetRandomStageType(int levelIndex)
        {
            // 해당 레벨에서 생성 가능한 타입만 필터링
            var availableConfigs = spawnConfigs
                .Where(c => levelIndex >= c.minLevelIndex)
                .ToList();

            if (availableConfigs.Count == 0)
            {
                Debug.LogWarning($"[StageTypeConfig] No available stage types for level {levelIndex}. Using Combat as fallback.");
                return StageType.Combat;
            }

            // 가중치 기반 랜덤 선택
            int totalWeight = availableConfigs.Sum(c => c.spawnWeight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var config in availableConfigs)
            {
                currentWeight += config.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return config.stageType;
                }
            }

            // 폴백 (도달하지 않아야 함)
            return availableConfigs[0].stageType;
        }
    }
}
