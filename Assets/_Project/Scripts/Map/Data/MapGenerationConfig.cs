using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 단일 레벨의 조우 설정
    /// Level = 진행 단계 (1~7)
    /// Stage = 각 레벨 내의 선택지
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

        /// <summary>
        /// 스테이지 타입에 맞는 랜덤 조우 선택
        /// </summary>
        public EncounterData GetRandomEncounter(StageType stageType)
        {
            List<EncounterData> pool = stageType == StageType.Combat ? combatEncounters : eliteEncounters;

            if (pool == null || pool.Count == 0)
            {
                Debug.LogError($"[LevelEncounterConfig] No encounters available for type {stageType}!");
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
        public int totalLevels = 7;

        [Header("Stage Type Config")]
        public StageTypeConfig stageTypeConfig;

        [Header("Encounter Configs")]
        [Tooltip("레벨별 조우 설정 (인덱스 0 = Level 1, 인덱스 5 = Level 6)")]
        public LevelEncounterConfig[] levelEncounters = new LevelEncounterConfig[6];

        [Tooltip("보스 조우 데이터 (Level 7 전용)")]
        public EncounterData bossEncounter;

        /// <summary>
        /// 특정 레벨과 스테이지 타입에 맞는 조우 데이터 가져오기
        /// </summary>
        public EncounterData GetEncounterForLevel(int levelNumber, StageType stageType)
        {
            // Level 7 (보스)
            if (levelNumber == totalLevels)
            {
                if (bossEncounter == null)
                {
                    Debug.LogError("[MapConfig] Boss encounter is not set!");
                }
                return bossEncounter;
            }

            // Level 1-6: 배열 인덱스로 접근
            int index = levelNumber - 1;
            if (index < 0 || index >= levelEncounters.Length)
            {
                Debug.LogError($"[MapConfig] Invalid level number {levelNumber}! Must be 1-{totalLevels}.");
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

#if UNITY_EDITOR
        /// <summary>
        /// 인스펙터에서 값 변경 시 자동으로 레벨 라벨 업데이트
        /// </summary>
        void OnValidate()
        {
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
