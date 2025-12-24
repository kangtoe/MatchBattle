using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 스테이지 타입
    /// </summary>
    public enum StageType
    {
        Combat,     // 일반 전투 ⚔️
        Elite,      // 엘리트 전투 💀
        Shop,       // 상점 🛒
        Rest,       // 휴식 💤
        Event,      // 이벤트 ❓
        Boss        // 보스 👹
    }

    /// <summary>
    /// 맵의 개별 스테이지 노드
    /// </summary>
    [System.Serializable]
    public class StageNode
    {
        // 기본 정보
        public int stageIndex;          // 단계 번호 (1-7)
        public StageType stageType;     // 스테이지 타입

        // 상태
        public bool isCompleted;        // 완료 여부

        // 연결 정보
        public List<StageNode> nextNodes;  // 다음 선택지 노드 리스트 (1-3개)

        public StageNode(int stageIndex, StageType stageType)
        {
            this.stageIndex = stageIndex;
            this.stageType = stageType;
            this.isCompleted = false;
            this.nextNodes = new List<StageNode>();
        }

        /// <summary>
        /// 노드 ID (예: "Stage 3 - Combat")
        /// </summary>
        public string GetNodeID()
        {
            return $"Stage {stageIndex} - {stageType}";
        }

        /// <summary>
        /// 스테이지 타입의 한글 이름 반환
        /// </summary>
        public string GetStageTypeName()
        {
            switch (stageType)
            {
                case StageType.Combat: return "일반 전투";
                case StageType.Elite: return "엘리트 전투";
                case StageType.Shop: return "상점";
                case StageType.Rest: return "휴식";
                case StageType.Event: return "이벤트";
                case StageType.Boss: return "보스";
                default: return stageType.ToString();
            }
        }
    }
}
