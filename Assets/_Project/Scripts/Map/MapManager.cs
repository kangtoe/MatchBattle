using UnityEngine;
using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 맵 시스템 관리자 (싱글톤)
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        // 싱글톤
        public static MapManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private MapGenerationConfig config;

        [Header("Current Map (Inspector에서 확인 가능)")]
        [SerializeField] private MapData currentMap;

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [SerializeField] private int editorTestSeed = 12345;
#endif

        // 이벤트
        public System.Action<StageNode> OnStageCompleted;
        public System.Action<MapData> OnMapGenerated;
        public System.Action OnRunCompleted;
        public System.Action OnRunFailed;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 새로운 런 시작 (새 맵 생성)
        /// </summary>
        public void StartNewRun()
        {
            int seed = Random.Range(0, int.MaxValue);
            currentMap = MapGenerator.GenerateMap(config, seed);

            // 첫 번째 노드를 현재 위치로 설정
            if (currentMap.rootNode != null)
            {
                currentMap.currentNode = currentMap.rootNode;
            }

            OnMapGenerated?.Invoke(currentMap);
            Debug.Log($"[Map] New run started with seed: {seed}");
            Debug.Log($"[Map] Starting at: {currentMap.currentNode?.GetNodeID()}");
        }

        /// <summary>
        /// 다음 스테이지 선택
        /// </summary>
        public void SelectNextStage(StageNode node)
        {
            if (node == null)
            {
                Debug.LogError("[Map] Cannot select null stage!");
                return;
            }

            currentMap.currentNode = node;
            Debug.Log($"[Map] Selected stage: {node.GetNodeID()}");

            // TODO: 씬 전환 또는 스테이지 시작
            // 현재는 로그만 출력
        }

        /// <summary>
        /// 현재 스테이지 완료 처리
        /// </summary>
        /// <returns>다음 선택지 노드 리스트</returns>
        public List<StageNode> CompleteCurrentStage()
        {
            if (currentMap.currentNode == null)
            {
                Debug.LogError("[Map] No current node to complete!");
                return new List<StageNode>();
            }

            StageNode completedNode = currentMap.currentNode;
            completedNode.isCompleted = true;
            currentMap.completedNodes.Add(completedNode);

            Debug.Log($"[Map] Stage completed: {completedNode.GetNodeID()}");
            Debug.Log($"[Map] Total completed stages: {currentMap.GetCompletedStageCount()}");

            OnStageCompleted?.Invoke(completedNode);

            // 보스 완료 시 런 클리어
            if (completedNode.stageType == StageType.Boss)
            {
                CompleteRun();
                return new List<StageNode>();
            }

            // 다음 선택지 노드 리스트 반환
            return completedNode.nextNodes;
        }

        /// <summary>
        /// 런 클리어
        /// </summary>
        private void CompleteRun()
        {
            Debug.Log("[Map] Run completed! Victory!");
            OnRunCompleted?.Invoke();
        }

        /// <summary>
        /// 런 실패 (플레이어 사망)
        /// </summary>
        public void FailRun()
        {
            Debug.Log("[Map] Run failed! Defeat!");
            OnRunFailed?.Invoke();
        }

        // Getters
        public MapData GetCurrentMap() => currentMap;
        public StageNode GetCurrentNode() => currentMap?.currentNode;
        public MapGenerationConfig GetConfig() => config;

#if UNITY_EDITOR
        // ===== Editor Debug Methods =====

        [ContextMenu("Debug: Generate Map (Editor Test Seed)")]
        private void DebugGenerateMapWithTestSeed()
        {
            if (config == null)
            {
                Debug.LogError("[Map] MapGenerationConfig is not assigned!");
                return;
            }

            currentMap = MapGenerator.GenerateMap(config, editorTestSeed);
            if (currentMap.rootNode != null)
            {
                currentMap.currentNode = currentMap.rootNode;
            }
            Debug.Log($"[Map] Debug map generated - Seed: {editorTestSeed}, Total Nodes: {currentMap.GetAllNodes().Count}");
        }

        [ContextMenu("Debug: Generate Map (Random Seed)")]
        private void DebugGenerateMapRandomSeed()
        {
            if (config == null)
            {
                Debug.LogError("[Map] MapGenerationConfig is not assigned!");
                return;
            }

            editorTestSeed = Random.Range(0, int.MaxValue);
            DebugGenerateMapWithTestSeed();
        }

        [ContextMenu("Debug: Print Map as JSON")]
        private void DebugPrintMapAsJSON()
        {
            if (currentMap == null)
            {
                Debug.LogWarning("[Map] No map generated. Use 'Debug: Generate Map' first.");
                return;
            }

            string json = JsonUtility.ToJson(currentMap, true);
            Debug.Log($"[Map] Current Map JSON:\n{json}");
        }
#endif
    }
}
