using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
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

        [Header("Encounter Pools")]
        [Tooltip("스테이지별 조우 풀 리스트 (Combat, Elite용)")]
        public List<EncounterPool> encounterPools = new List<EncounterPool>();

        [Tooltip("보스 조우 데이터 (Stage 7 전용)")]
        public EncounterData bossEncounter;

        /// <summary>
        /// 특정 스테이지와 타입에 맞는 조우 풀 찾기
        /// </summary>
        public EncounterPool GetEncounterPool(int stageNumber, StageType stageType)
        {
            foreach (var pool in encounterPools)
            {
                if (pool.stageNumber == stageNumber && pool.encounterType == stageType)
                {
                    return pool;
                }
            }

            Debug.LogWarning($"[MapConfig] No encounter pool found for Stage {stageNumber} - {stageType}");
            return null;
        }
    }
}
