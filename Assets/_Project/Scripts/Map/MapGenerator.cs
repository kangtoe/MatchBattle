using UnityEngine;
using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 맵 생성 유틸리티 (Static)
    /// </summary>
    public static class MapGenerator
    {
        // 노드 개수 상수 (UI 제약으로 고정)
        private const int MIN_NODES_PER_STAGE = 1;
        private const int MAX_NODES_PER_STAGE = 3;

        /// <summary>
        /// 랜덤 맵 생성 (트리 구조)
        /// </summary>
        public static MapData GenerateMap(MapGenerationConfig config, int seed)
        {
            Random.InitState(seed);

            MapData map = new MapData();
            map.seed = seed;

            // Stage 1: Combat 노드 생성 (시작 노드)
            StageNode rootNode = new StageNode(1, StageType.Combat);
            map.rootNode = rootNode;

            Debug.Log($"[MapGen] Creating map with seed: {seed}");

            // 재귀적으로 트리 생성
            GenerateNextStages(rootNode, config, 1);

            // 생성된 전체 노드 수 확인
            int totalNodes = map.GetAllNodes().Count;
            Debug.Log($"[MapGen] Map generated with {totalNodes} total nodes");

            return map;
        }

        /// <summary>
        /// 재귀적으로 다음 단계 노드들 생성
        /// </summary>
        private static void GenerateNextStages(StageNode currentNode, MapGenerationConfig config, int currentStageIndex)
        {
            // Stage 7에 도달하면 종료
            if (currentStageIndex >= config.totalStages)
                return;

            int nextStageIndex = currentStageIndex + 1;

            // 다음 노드 개수 결정
            int nextNodeCount;
            if (nextStageIndex == config.totalStages)
            {
                // Stage 7 (보스)는 1개만
                nextNodeCount = 1;
            }
            else if (currentStageIndex == config.totalStages - 1)
            {
                // Stage 6 → Stage 7도 1개만
                nextNodeCount = 1;
            }
            else
            {
                // Stage 1-5: 1-3개 랜덤
                nextNodeCount = Random.Range(MIN_NODES_PER_STAGE, MAX_NODES_PER_STAGE + 1);
            }

            // 다음 노드들 생성
            for (int i = 0; i < nextNodeCount; i++)
            {
                StageNode nextNode = GenerateStageNode(nextStageIndex, config);
                currentNode.nextNodes.Add(nextNode);

                // 재귀 호출
                GenerateNextStages(nextNode, config, nextStageIndex);
            }
        }

        /// <summary>
        /// 단일 스테이지 노드 생성
        /// </summary>
        private static StageNode GenerateStageNode(int stageIndex, MapGenerationConfig config)
        {
            StageType stageType;

            // Stage 7은 보스 고정
            if (stageIndex == config.totalStages)
            {
                stageType = StageType.Boss;
            }
            else
            {
                // 확률 기반 스테이지 타입 선택
                stageType = config.stageTypeConfig.GetRandomStageType(stageIndex);
            }

            StageNode node = new StageNode(stageIndex, stageType);

            Debug.Log($"[MapGen] Created node: {node.GetNodeID()}");
            return node;
        }
    }
}
