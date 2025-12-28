using UnityEngine;
using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 맵 생성 유틸리티 (Static)
    ///
    /// 새로운 구조:
    /// - Level = 진행 단계 (1, 2, 3...)
    /// - 각 Level은 여러 Stage로 구성
    /// - 각 Level의 마지막 Stage는 보스
    ///
    /// 트리 구조:
    /// - 각 스테이지에서 1-3개 선택지 제공
    /// - 선택지들은 다음 스테이지로 수렴 (중복 트리 방지)
    /// </summary>
    public static class MapGenerator
    {
        // 선택지 개수 상수
        private const int MIN_CHOICES = 1;
        private const int MAX_CHOICES = 3;

        // 안전 장치: 최대 노드 수 제한
        private const int MAX_NODES = 500;
        private static int nodeCount;

        /// <summary>
        /// 랜덤 맵 생성
        /// </summary>
        public static MapData GenerateMap(MapGenerationConfig config, int seed)
        {
            if (config == null)
            {
                Debug.LogError("[MapGen] MapGenerationConfig is null! Cannot generate map.");
                return null;
            }

            Random.InitState(seed);
            nodeCount = 0;

            MapData map = new MapData();
            map.seed = seed;

            Debug.Log($"[MapGen] Creating map with seed: {seed}, totalLevels: {config.totalLevels}");

            // Level 1, Stage 1: Combat 노드 생성 (시작 노드)
            StageNode rootNode = new StageNode(1, 1, StageType.Combat);
            map.rootNode = rootNode;
            nodeCount++;

            Debug.Log($"[MapGen] Created root: {rootNode.GetNodeID()}");

            // 레벨별로 순차적으로 생성 (수렴 구조)
            GenerateAllLevels(rootNode, config);

            // 생성된 전체 노드 수 확인
            int totalNodes = map.GetAllNodes().Count;
            Debug.Log($"[MapGen] Map generated with {totalNodes} total nodes across {config.totalLevels} levels");

            return map;
        }

        /// <summary>
        /// 모든 레벨을 순차적으로 생성 (수렴 구조)
        /// </summary>
        private static void GenerateAllLevels(StageNode rootNode, MapGenerationConfig config)
        {
            // 현재 스테이지의 모든 리프 노드들을 추적
            List<StageNode> currentLeaves = new List<StageNode> { rootNode };

            for (int levelIndex = 1; levelIndex <= config.totalLevels; levelIndex++)
            {
                int stagesInLevel = config.GetStageCountForLevel(levelIndex);

                Debug.Log($"[MapGen] Generating Level {levelIndex} with {stagesInLevel} stages + boss");

                // 이 레벨의 스테이지들 생성 (루트 노드는 Level 1 Stage 1으로 이미 생성됨)
                int startStage = (levelIndex == 1) ? 2 : 1;

                for (int stageIndex = startStage; stageIndex <= stagesInLevel; stageIndex++)
                {
                    currentLeaves = GenerateNextStageLayer(currentLeaves, levelIndex, stageIndex, config);

                    if (nodeCount >= MAX_NODES)
                    {
                        Debug.LogWarning($"[MapGen] Max node limit reached ({MAX_NODES}). Stopping generation.");
                        return;
                    }
                }

                // 보스 스테이지 생성 (수렴: 모든 리프가 하나의 보스로)
                StageNode bossNode = new StageNode(levelIndex, stagesInLevel + 1, StageType.Boss);
                nodeCount++;

                foreach (var leaf in currentLeaves)
                {
                    leaf.nextNodes.Add(bossNode);
                }

                Debug.Log($"[MapGen] Created boss: {bossNode.GetNodeID()}");

                // 마지막 레벨이면 종료
                if (levelIndex >= config.totalLevels)
                {
                    Debug.Log($"[MapGen] Final boss created at Level {levelIndex}");
                    break;
                }

                // 다음 레벨의 첫 스테이지로 연결
                currentLeaves = new List<StageNode> { bossNode };
            }
        }

        /// <summary>
        /// 다음 스테이지 레이어 생성 (선택지 노드들)
        /// </summary>
        private static List<StageNode> GenerateNextStageLayer(List<StageNode> currentLeaves,
            int levelIndex, int stageIndex, MapGenerationConfig config)
        {
            // 이 스테이지의 선택지 개수 결정 (1-3개)
            int choiceCount = Random.Range(MIN_CHOICES, MAX_CHOICES + 1);

            // 선택지 노드들 생성
            List<StageNode> newNodes = new List<StageNode>();
            for (int i = 0; i < choiceCount; i++)
            {
                StageNode newNode = GenerateStageNode(levelIndex, stageIndex, config);
                newNodes.Add(newNode);
                nodeCount++;
            }

            // 모든 현재 리프 노드들이 새 노드들을 선택지로 가짐
            foreach (var leaf in currentLeaves)
            {
                foreach (var newNode in newNodes)
                {
                    leaf.nextNodes.Add(newNode);
                }
            }

            return newNodes;
        }

        /// <summary>
        /// 단일 스테이지 노드 생성 (보스 아닌 일반 스테이지)
        /// </summary>
        private static StageNode GenerateStageNode(int levelIndex, int stageIndex, MapGenerationConfig config)
        {
            // 확률 기반 스테이지 타입 선택
            StageType stageType = config.stageTypeConfig.GetRandomStageType(levelIndex);

            StageNode node = new StageNode(levelIndex, stageIndex, stageType);

            Debug.Log($"[MapGen] Created node: {node.GetNodeID()}");
            return node;
        }
    }
}
