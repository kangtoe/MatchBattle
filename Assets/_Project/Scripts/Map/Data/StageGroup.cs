using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 한 스테이지의 선택지 그룹
    /// 예: Stage 2의 [전투 A | 상점 B | 휴식 C] 3개 선택지
    /// </summary>
    [System.Serializable]
    public class StageGroup
    {
        public List<StageNode> choices;  // N개의 선택지 노드 (예: 3개)

        public StageGroup()
        {
            choices = new List<StageNode>();
        }

        /// <summary>
        /// 선택지 개수 반환
        /// </summary>
        public int GetChoiceCount()
        {
            return choices.Count;
        }

        /// <summary>
        /// 특정 인덱스의 선택지 가져오기
        /// </summary>
        public StageNode GetChoice(int index)
        {
            if (index >= 0 && index < choices.Count)
            {
                return choices[index];
            }
            return null;
        }
    }
}
