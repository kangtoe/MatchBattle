using UnityEngine;
using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 맵 시스템 관리자 (싱글톤)
    /// </summary>
    public class MapManager : Singleton<MapManager>
    {
        [Header("Configuration")]
        [SerializeField] private MapGenerationConfig config;

        [Header("Current Map (Inspector에서 확인 가능)")]
        [SerializeField] private MapData currentMap;

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [SerializeField] private int editorTestSeed = 12345;
#endif

        // 이벤트
        public System.Action<StageNode> OnStageEntered;                     // 스테이지 진입 시 발생
        public System.Action<StageNode> OnStageCompleted;
        public System.Action<List<StageNode>> OnNextStageSelectionRequired; // 다음 스테이지 선택 필요 시 발생
        public System.Action<MapData> OnMapGenerated;
        public System.Action OnRunCompleted;
        public System.Action OnRunFailed;

        void Start()
        {
            // TODO: 추후 메인 메뉴의 "새 게임" 버튼 이벤트로 연결 예정
            // 현재는 테스트를 위해 시작 시 자동 호출
            StartNewRun();
        }

        /// <summary>
        /// 새로운 런 시작 (새 맵 생성)
        /// </summary>
        public void StartNewRun()
        {
            if (config == null)
            {
                Debug.LogError("[Map] MapGenerationConfig is not assigned! Please assign a MapGenerationConfig in the Inspector.");
                return;
            }

            // RunManager 초기화 (플레이어 생성 및 런 상태 초기화)
            if (RunManager.Instance != null)
            {
                RunManager.Instance.StartNewRun();
            }
            else
            {
                Debug.LogWarning("[Map] RunManager not found! Player state will not persist.");
            }

            int seed = Random.Range(0, int.MaxValue);
            currentMap = MapGenerator.GenerateMap(config, seed);

            // 첫 번째 스테이지로 위치 설정 (Level 0, Stage 0)
            currentMap.currentLevelIndex = 0;
            currentMap.currentStageIndex = 0;

            // 첫 스테이지의 첫 번째 선택지를 자동 선택 (게임 시작)
            currentMap.currentChoiceIndex = 0;

            OnMapGenerated?.Invoke(currentMap);
            Debug.Log($"[Map] New run started with seed: {seed}");
            Debug.Log($"[Map] Starting at Level {currentMap.currentLevelIndex + 1}, Stage {currentMap.currentStageIndex + 1}");

            // 첫 스테이지 진입
            StageNode firstStage = currentMap.GetCurrentSelectedNode();
            if (firstStage != null)
            {
                Debug.Log($"[Map] Auto-selected first stage: {firstStage.GetNodeID()}");
                HandleStageEntry(firstStage);
            }
            else
            {
                Debug.LogError("[Map] Failed to get first stage!");
            }
        }

        /// <summary>
        /// 다음 스테이지 선택 (현재 스테이지의 선택지 중 하나 선택)
        /// </summary>
        public void SelectNextStage(int choiceIndex)
        {
            StageGroup currentStageGroup = currentMap.GetCurrentStageGroup();
            if (currentStageGroup == null || choiceIndex < 0 || choiceIndex >= currentStageGroup.GetChoiceCount())
            {
                Debug.LogError($"[Map] Invalid choice index: {choiceIndex}");
                return;
            }

            // 선택한 인덱스 저장
            currentMap.currentChoiceIndex = choiceIndex;

            StageNode selectedNode = currentStageGroup.GetChoice(choiceIndex);
            Debug.Log($"[Map] Selected stage: {selectedNode.GetNodeID()} (Choice {choiceIndex + 1}/{currentStageGroup.GetChoiceCount()})");

            // 스테이지 타입에 따라 적절한 시스템 호출
            HandleStageEntry(selectedNode);
        }

        /// <summary>
        /// 스테이지 진입 처리 (타입별 분기)
        /// </summary>
        private void HandleStageEntry(StageNode node)
        {
            Debug.Log($"[Map] Stage entry: {node.stageType} - {node.GetNodeID()}");

            // 스테이지 진입 이벤트 발생 (UI가 이 이벤트를 구독)
            OnStageEntered?.Invoke(node);

            // 스테이지 타입별 로직 처리
            switch (node.stageType)
            {
                case StageType.Combat:
                case StageType.Elite:
                case StageType.Boss:
                    // 전투 시작
                    if (CombatManager.Instance != null)
                    {
                        CombatManager.Instance.StartCombatFromCurrentStage();
                    }
                    else
                    {
                        Debug.LogError("[Map] CombatManager instance not found!");
                    }
                    break;

                case StageType.Shop:
                    // TODO: 상점 시스템 호출
                    break;

                case StageType.Rest:
                    // RestManager의 완료 이벤트에 다음 단계 진행 연결
                    if (RestManager.Instance != null)
                    {
                        RestManager.Instance.OnRestCompleted.RemoveAllListeners();
                        RestManager.Instance.OnRestCompleted.AddListener(OnRestCompleted);
                    }
                    else
                    {
                        Debug.LogError("[Map] RestManager instance not found!");
                    }
                    break;

                case StageType.Event:
                    // TODO: 이벤트 시스템 호출
                    break;

                default:
                    Debug.LogWarning($"[Map] Unknown stage type: {node.stageType}");
                    break;
            }
        }

        /// <summary>
        /// 휴식 완료 후 처리
        /// </summary>
        private void OnRestCompleted()
        {
            Debug.Log("[Map] Rest completed, proceeding to next stage selection");

            // 현재 스테이지 완료 처리
            List<StageNode> nextStages = CompleteCurrentStage();

            // 다음 스테이지 선택 이벤트 발생 (UIManager가 구독)
            if (nextStages.Count > 0)
            {
                Debug.Log($"[Map] Next stage choices available: {nextStages.Count}");
                OnNextStageSelectionRequired?.Invoke(nextStages);
            }
            else
            {
                // 런 완료
                Debug.Log("[Map] Run completed!");
                OnRunCompleted?.Invoke();
            }
        }

        /// <summary>
        /// 현재 스테이지 완료 처리 (이전에 선택한 노드 완료)
        /// SelectNextStage()로 선택한 노드를 자동으로 완료 처리
        /// </summary>
        /// <returns>다음 선택지 노드 리스트</returns>
        public List<StageNode> CompleteCurrentStage()
        {
            if (currentMap.currentChoiceIndex < 0)
            {
                Debug.LogError("[Map] No stage choice selected! Call SelectNextStage() first.");
                return new List<StageNode>();
            }

            return CompleteCurrentStage(currentMap.currentChoiceIndex);
        }

        /// <summary>
        /// 현재 스테이지 완료 처리 (명시적 선택지 인덱스)
        /// </summary>
        /// <returns>다음 선택지 노드 리스트</returns>
        public List<StageNode> CompleteCurrentStage(int choiceIndex)
        {
            StageGroup currentStageGroup = currentMap.GetCurrentStageGroup();
            if (currentStageGroup == null || choiceIndex < 0 || choiceIndex >= currentStageGroup.GetChoiceCount())
            {
                Debug.LogError($"[Map] Invalid choice index: {choiceIndex}");
                return new List<StageNode>();
            }

            // 선택한 노드 완료 처리
            StageNode completedNode = currentStageGroup.GetChoice(choiceIndex);
            completedNode.isCompleted = true;
            currentMap.completedNodes.Add(completedNode);

            Debug.Log($"[Map] Stage completed: {completedNode.GetNodeID()}");
            Debug.Log($"[Map] Total completed stages: {currentMap.GetCompletedStageCount()}");

            OnStageCompleted?.Invoke(completedNode);

            // 레벨 보스 완료 시
            if (completedNode.stageType == StageType.Boss)
            {
                // 마지막 레벨 보스면 런 클리어
                if (currentMap.currentLevelIndex >= currentMap.levels.Count - 1)
                {
                    CompleteRun();
                    return new List<StageNode>();
                }

                // 다음 레벨로 이동
                currentMap.currentLevelIndex++;
                currentMap.currentStageIndex = 0;
                currentMap.currentChoiceIndex = -1;  // 다음 선택 대기
                Debug.Log($"[Map] Level {completedNode.levelIndex} Boss cleared! Moving to Level {currentMap.currentLevelIndex + 1}");
            }
            else
            {
                // 다음 스테이지로 이동
                currentMap.currentStageIndex++;
                currentMap.currentChoiceIndex = -1;  // 다음 선택 대기
            }

            // 다음 선택지 그룹 가져오기
            StageGroup nextStageGroup = currentMap.GetCurrentStageGroup();
            if (nextStageGroup != null)
            {
                return nextStageGroup.choices;
            }

            return new List<StageNode>();
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
        public StageGroup GetCurrentStageGroup() => currentMap?.GetCurrentStageGroup();
        public StageNode GetCurrentNode() => currentMap?.GetCurrentSelectedNode();
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
            currentMap.currentLevelIndex = 0;
            currentMap.currentStageIndex = 0;
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
