using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 스테이지별 조우 풀 - 특정 스테이지/타입에서 출현 가능한 조우 리스트
    /// </summary>
    [CreateAssetMenu(fileName = "EncounterPool", menuName = "MatchBattle/EncounterPool")]
    public class EncounterPool : ScriptableObject
    {
        [Header("Pool Info")]
        [Tooltip("풀 이름 (예: 'Stage 2 - Normal')")]
        public string poolName;

        [Tooltip("스테이지 번호 (1-7)")]
        [Range(1, 7)]
        public int stageNumber;

        [Tooltip("조우 타입 (Combat 또는 Elite)")]
        public StageType encounterType = StageType.Combat;

        [Header("Encounters")]
        [Tooltip("이 풀에서 출현 가능한 조우 리스트")]
        public List<EncounterData> encounters = new List<EncounterData>();

        /// <summary>
        /// 풀에서 랜덤 조우 선택
        /// </summary>
        public EncounterData GetRandomEncounter()
        {
            if (encounters == null || encounters.Count == 0)
            {
                Debug.LogError($"[EncounterPool] '{poolName}' has no encounters!");
                return null;
            }

            int randomIndex = Random.Range(0, encounters.Count);
            return encounters[randomIndex];
        }

        /// <summary>
        /// 검증: Boss 타입 풀은 생성 불가
        /// </summary>
        void OnValidate()
        {
            if (encounterType == StageType.Boss)
            {
                Debug.LogWarning($"[EncounterPool] Boss encounters should be defined separately, not in pools.");
            }

            if (encounterType != StageType.Combat && encounterType != StageType.Elite)
            {
                Debug.LogWarning($"[EncounterPool] Only Combat and Elite types should use encounter pools. Current: {encounterType}");
            }
        }
    }
}
