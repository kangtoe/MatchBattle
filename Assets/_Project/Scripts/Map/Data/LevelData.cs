using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 한 레벨의 모든 스테이지 그룹들
    /// 예: Level 1 = [Stage 1 선택지, Stage 2 선택지, Stage 3 선택지, Boss]
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public int levelIndex;              // 레벨 번호 (1, 2, 3...)
        public List<StageGroup> stages;     // 각 스테이지별 선택지 그룹

        public LevelData(int levelIndex)
        {
            this.levelIndex = levelIndex;
            this.stages = new List<StageGroup>();
        }

        /// <summary>
        /// 스테이지 개수 반환
        /// </summary>
        public int GetStageCount()
        {
            return stages.Count;
        }

        /// <summary>
        /// 특정 스테이지의 선택지 그룹 가져오기
        /// </summary>
        public StageGroup GetStage(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < stages.Count)
            {
                return stages[stageIndex];
            }
            return null;
        }
    }
}
