using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 전체 맵 데이터 (트리 구조)
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        // 맵 구조
        public StageNode rootNode;          // 시작 노드 (Stage 1)
        public StageNode currentNode;       // 현재 위치

        // 진행 상황
        public List<StageNode> completedNodes;  // 완료한 노드 리스트 (히스토리 추적용)

        // 런 정보
        public int seed;                    // 맵 생성 시드 (재현 가능)

        public MapData()
        {
            rootNode = null;
            currentNode = null;
            completedNodes = new List<StageNode>();
            seed = 0;
        }

        /// <summary>
        /// 모든 노드 가져오기 (DFS 순회)
        /// </summary>
        public List<StageNode> GetAllNodes()
        {
            List<StageNode> allNodes = new List<StageNode>();
            if (rootNode != null)
            {
                CollectNodesRecursive(rootNode, allNodes);
            }
            return allNodes;
        }

        private void CollectNodesRecursive(StageNode node, List<StageNode> collection)
        {
            if (node == null || collection.Contains(node))
                return;

            collection.Add(node);

            foreach (var nextNode in node.nextNodes)
            {
                CollectNodesRecursive(nextNode, collection);
            }
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
