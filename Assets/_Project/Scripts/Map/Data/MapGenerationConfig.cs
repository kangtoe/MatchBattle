using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 단일 레벨의 조우 설정
    ///
    /// 구조:
    /// - Level = 진행 단계 (1, 2, 3...)
    /// - 각 레벨은 여러 스테이지로 구성 (전투/상점/휴식 → 보스)
    /// - 레벨 내 모든 전투 스테이지는 동일한 조우 풀 사용
    /// </summary>
    [System.Serializable]
    public class LevelEncounterConfig
    {
        [Tooltip("레벨 표시 (자동 설정됨)")]
        public string levelLabel = "Level ?";

        [Tooltip("일반 전투 조우 리스트")]
        public List<EncounterData> combatEncounters = new List<EncounterData>();

        [Tooltip("엘리트 전투 조우 리스트")]
        public List<EncounterData> eliteEncounters = new List<EncounterData>();

        [Tooltip("이 레벨의 보스 조우")]
        public EncounterData bossEncounter;

        /// <summary>
        /// 스테이지 타입에 맞는 랜덤 조우 선택
        /// </summary>
        public EncounterData GetRandomEncounter(StageType stageType)
        {
            // 보스는 별도 처리
            if (stageType == StageType.Boss)
            {
                if (bossEncounter == null)
                {
                    Debug.LogError($"[LevelEncounterConfig] Boss encounter is not set for {levelLabel}!");
                }
                return bossEncounter;
            }

            // 일반/엘리트 전투
            List<EncounterData> pool = stageType == StageType.Elite ? eliteEncounters : combatEncounters;

            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning($"[LevelEncounterConfig] No encounters for {stageType} in {levelLabel}, using combat pool");
                pool = combatEncounters;
            }

            if (pool == null || pool.Count == 0)
            {
                Debug.LogError($"[LevelEncounterConfig] No encounters available in {levelLabel}!");
                return null;
            }

            int randomIndex = Random.Range(0, pool.Count);
            return pool[randomIndex];
        }
    }

    /// <summary>
    /// 맵 생성 설정
    /// </summary>
    [CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "MatchBattle/MapGenerationConfig")]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("Map Structure")]
        [Tooltip("총 레벨 수")]
        public int totalLevels = 3;

        [Tooltip("레벨당 스테이지 수 (보스 제외)")]
        [Range(1, 10)]
        public int stagesPerLevel = 4;

        [Header("Stage Type Config")]
        public StageTypeConfig stageTypeConfig;

        [Header("Encounter Configs")]
        [Tooltip("레벨별 조우 설정 (인덱스 0 = Level 1)")]
        public LevelEncounterConfig[] levelEncounters = new LevelEncounterConfig[3];

        /// <summary>
        /// 특정 레벨과 스테이지 타입에 맞는 조우 데이터 가져오기
        /// </summary>
        public EncounterData GetEncounterForLevel(int levelNumber, StageType stageType)
        {
            // 유효성 검사
            int index = levelNumber - 1;
            if (index < 0 || index >= levelEncounters.Length)
            {
                Debug.LogError($"[MapConfig] Invalid level number {levelNumber}! Must be 1-{levelEncounters.Length}.");
                return null;
            }

            LevelEncounterConfig config = levelEncounters[index];
            if (config == null)
            {
                Debug.LogError($"[MapConfig] Level {levelNumber} encounter config is null! Please configure it in Inspector.");
                return null;
            }

            return config.GetRandomEncounter(stageType);
        }

        /// <summary>
        /// 해당 레벨의 스테이지 수 반환 (보스 제외, 모든 레벨 동일)
        /// </summary>
        public int GetStageCountForLevel(int levelNumber)
        {
            return stagesPerLevel;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 인스펙터에서 값 변경 시 자동으로 레벨 라벨 업데이트
        /// </summary>
        void OnValidate()
        {
            // 레벨 수 제한
            if (totalLevels < 1) totalLevels = 1;
            if (totalLevels > 10) totalLevels = 10;

            // 스테이지 수 유효성
            if (stagesPerLevel < 1) stagesPerLevel = 1;

            // 레벨 라벨 업데이트
            if (levelEncounters != null)
            {
                for (int i = 0; i < levelEncounters.Length; i++)
                {
                    if (levelEncounters[i] != null)
                    {
                        levelEncounters[i].levelLabel = $"Level {i + 1}";
                    }
                }
            }
        }
#endif
    }
}
