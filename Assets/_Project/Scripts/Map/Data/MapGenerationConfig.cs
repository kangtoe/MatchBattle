using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 단일 스테이지의 조우 설정
    /// </summary>
    [System.Serializable]
    public class StageEncounterConfig
    {
        [Tooltip("스테이지 표시 (자동 설정됨)")]
        public string stageLabel = "Stage ?";

        [Tooltip("전투 조우 리스트")]
        public List<EncounterData> combatEncounters = new List<EncounterData>();

        [Tooltip("엘리트 조우 리스트")]
        public List<EncounterData> eliteEncounters = new List<EncounterData>();

        /// <summary>
        /// 타입에 맞는 랜덤 조우 선택
        /// </summary>
        public EncounterData GetRandomEncounter(StageType stageType)
        {
            List<EncounterData> pool = stageType == StageType.Combat ? combatEncounters : eliteEncounters;

            if (pool == null || pool.Count == 0)
            {
                Debug.LogError($"[StageEncounterConfig] No encounters available for type {stageType}!");
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
        [Tooltip("총 단계 수")]
        public int totalStages = 7;

        [Header("Stage Type Config")]
        public StageTypeConfig stageTypeConfig;

        [Header("Encounter Configs")]
        [Tooltip("스테이지별 조우 설정 (인덱스 0 = Stage 1, 인덱스 5 = Stage 6)")]
        public StageEncounterConfig[] stageEncounters = new StageEncounterConfig[6];

        [Tooltip("보스 조우 데이터 (Stage 7 전용)")]
        public EncounterData bossEncounter;

        /// <summary>
        /// 특정 스테이지와 타입에 맞는 조우 데이터 가져오기
        /// </summary>
        public EncounterData GetEncounterForStage(int stageNumber, StageType stageType)
        {
            // Stage 7 (보스)
            if (stageNumber == totalStages)
            {
                if (bossEncounter == null)
                {
                    Debug.LogError("[MapConfig] Boss encounter is not set!");
                }
                return bossEncounter;
            }

            // Stage 1-6: 배열 인덱스로 접근
            int index = stageNumber - 1;
            if (index < 0 || index >= stageEncounters.Length)
            {
                Debug.LogError($"[MapConfig] Invalid stage number {stageNumber}! Must be 1-{totalStages}.");
                return null;
            }

            StageEncounterConfig config = stageEncounters[index];
            if (config == null)
            {
                Debug.LogError($"[MapConfig] Stage {stageNumber} encounter config is null! Please configure it in Inspector.");
                return null;
            }

            return config.GetRandomEncounter(stageType);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 인스펙터에서 값 변경 시 자동으로 스테이지 라벨 업데이트
        /// </summary>
        void OnValidate()
        {
            if (stageEncounters != null)
            {
                for (int i = 0; i < stageEncounters.Length; i++)
                {
                    if (stageEncounters[i] != null)
                    {
                        stageEncounters[i].stageLabel = $"Stage {i + 1}";
                    }
                }
            }
        }
#endif
    }
}
