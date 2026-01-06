using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 전체 맵 데이터 (격자 구조)
    /// 구조: levels[levelIndex].stages[stageIndex].choices[choiceIndex]
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        // 맵 구조 (격자)
        public List<LevelData> levels;      // 모든 레벨 데이터

        // 현재 위치
        public int currentLevelIndex;       // 현재 레벨 (0부터 시작)
        public int currentStageIndex;       // 현재 스테이지 (0부터 시작)
        public int currentChoiceIndex;      // 현재 선택한 선택지 인덱스 (0부터 시작)

        // 진행 상황
        public List<StageNode> completedNodes;  // 완료한 노드 리스트 (히스토리 추적용)

        // 런 정보
        public int seed;                    // 맵 생성 시드 (재현 가능)

        public MapData()
        {
            levels = new List<LevelData>();
            currentLevelIndex = 0;
            currentStageIndex = 0;
            currentChoiceIndex = -1;  // -1 = 아직 선택 안 함
            completedNodes = new List<StageNode>();
            seed = 0;
        }

        /// <summary>
        /// 모든 노드 가져오기 (격자 순회)
        /// </summary>
        public List<StageNode> GetAllNodes()
        {
            List<StageNode> allNodes = new List<StageNode>();

            foreach (var level in levels)
            {
                foreach (var stageGroup in level.stages)
                {
                    foreach (var node in stageGroup.choices)
                    {
                        allNodes.Add(node);
                    }
                }
            }

            return allNodes;
        }

        /// <summary>
        /// 현재 스테이지의 선택지 그룹 가져오기
        /// </summary>
        public StageGroup GetCurrentStageGroup()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            {
                return levels[currentLevelIndex].GetStage(currentStageIndex);
            }
            return null;
        }

        /// <summary>
        /// 현재 선택된 노드 가져오기
        /// </summary>
        public StageNode GetCurrentSelectedNode()
        {
            StageGroup stageGroup = GetCurrentStageGroup();
            if (stageGroup != null && currentChoiceIndex >= 0 && currentChoiceIndex < stageGroup.GetChoiceCount())
            {
                return stageGroup.GetChoice(currentChoiceIndex);
            }
            return null;
        }

        /// <summary>
        /// 다음 스테이지의 선택지 그룹 가져오기
        /// </summary>
        public StageGroup GetNextStageGroup()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            {
                int nextStageIndex = currentStageIndex + 1;

                // 현재 레벨 내 다음 스테이지가 있으면 반환
                if (nextStageIndex < levels[currentLevelIndex].GetStageCount())
                {
                    return levels[currentLevelIndex].GetStage(nextStageIndex);
                }

                // 다음 레벨의 첫 스테이지 확인
                int nextLevelIndex = currentLevelIndex + 1;
                if (nextLevelIndex < levels.Count && levels[nextLevelIndex].GetStageCount() > 0)
                {
                    return levels[nextLevelIndex].GetStage(0);
                }
            }
            return null;
        }

        /// <summary>
        /// 완료된 스테이지 수 반환
        /// </summary>
        public int GetCompletedStageCount()
        {
            return completedNodes.Count;
        }
    }
}
