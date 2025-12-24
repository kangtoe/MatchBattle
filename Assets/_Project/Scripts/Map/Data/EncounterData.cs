using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 전투 조우 데이터 - 4개 슬롯에 적 배치 정보
    /// </summary>
    [CreateAssetMenu(fileName = "Encounter", menuName = "MatchBattle/EncounterData")]
    public class EncounterData : ScriptableObject
    {
        [Header("Encounter Info")]
        [Tooltip("조우 이름 (예: '슬라임 단독', '고블린 듀오')")]
        public string encounterName;

        [Header("Enemy Slots (4개 고정 슬롯)")]
        [Tooltip("슬롯 0 - 가장 왼쪽/전방")]
        public EnemyData enemySlot0;

        [Tooltip("슬롯 1")]
        public EnemyData enemySlot1;

        [Tooltip("슬롯 2")]
        public EnemyData enemySlot2;

        [Tooltip("슬롯 3 - 가장 오른쪽/후방")]
        public EnemyData enemySlot3;

        /// <summary>
        /// 슬롯 배열로 반환 (인덱스로 접근 가능)
        /// </summary>
        public EnemyData[] GetEnemySlots()
        {
            return new EnemyData[] { enemySlot0, enemySlot1, enemySlot2, enemySlot3 };
        }

        /// <summary>
        /// 비어있지 않은 슬롯의 개수
        /// </summary>
        public int GetEnemyCount()
        {
            int count = 0;
            if (enemySlot0 != null) count++;
            if (enemySlot1 != null) count++;
            if (enemySlot2 != null) count++;
            if (enemySlot3 != null) count++;
            return count;
        }
    }
}
