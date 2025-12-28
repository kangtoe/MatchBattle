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
        Boss        // 보스 👹 (각 레벨의 마지막)
    }

    /// <summary>
    /// 맵의 개별 스테이지 노드
    ///
    /// 구조:
    /// - Level = 진행 단계 (1, 2, 3...)
    /// - Stage = 레벨 내 순서 (1, 2, 3... → 마지막은 보스)
    /// - 같은 레벨 내 모든 전투는 동일한 조우 풀 사용
    /// </summary>
    [System.Serializable]
    public class StageNode
    {
        // 기본 정보
        public int levelIndex;          // 레벨 번호 (1, 2, 3...)
        public int stageIndex;          // 레벨 내 스테이지 순서 (1, 2, 3...)
        public StageType stageType;     // 스테이지 타입

        // 상태
        public bool isCompleted;        // 완료 여부

        // 연결 정보
        public List<StageNode> nextNodes;  // 다음 선택지 노드 리스트 (1-3개)

        public StageNode(int levelIndex, int stageIndex, StageType stageType)
        {
            this.levelIndex = levelIndex;
            this.stageIndex = stageIndex;
            this.stageType = stageType;
            this.isCompleted = false;
            this.nextNodes = new List<StageNode>();
        }

        /// <summary>
        /// 노드 ID (예: "Lv.1 Stage 3 - Combat")
        /// </summary>
        public string GetNodeID()
        {
            return $"Lv.{levelIndex} Stage {stageIndex} - {stageType}";
        }

        /// <summary>
        /// 짧은 표시용 ID (예: "1-3 전투")
        /// </summary>
        public string GetShortID()
        {
            return $"{levelIndex}-{stageIndex} {GetStageTypeName()}";
        }

        /// <summary>
        /// 스테이지 타입의 한글 이름 반환
        /// </summary>
        public string GetStageTypeName()
        {
            switch (stageType)
            {
                case StageType.Combat: return "전투";
                case StageType.Elite: return "엘리트";
                case StageType.Shop: return "상점";
                case StageType.Rest: return "휴식";
                case StageType.Event: return "이벤트";
                case StageType.Boss: return "보스";
                default: return stageType.ToString();
            }
        }

        /// <summary>
        /// 이 스테이지가 레벨의 보스인지 확인
        /// </summary>
        public bool IsLevelBoss()
        {
            return stageType == StageType.Boss;
        }
    }
}
