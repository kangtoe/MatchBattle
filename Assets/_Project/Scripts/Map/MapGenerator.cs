using UnityEngine;
using System.Collections.Generic;

namespace MatchBattle
{
    /// <summary>
    /// 맵 생성 유틸리티 (Static)
    ///
    /// 격자 구조:
    /// - Level = 진행 단계 (1, 2, 3...)
    /// - 각 Level은 여러 Stage로 구성
    /// - 각 Stage는 N개의 선택지 노드를 가짐
    /// - 각 Level의 마지막 Stage는 보스 (1개 선택지만)
    ///
    /// 구조: levels[levelIndex].stages[stageIndex].choices[choiceIndex]
    /// </summary>
    public static class MapGenerator
    {
        // 선택지 개수 상수
        private const int MIN_CHOICES = 2;
        private const int MAX_CHOICES = 3;

        /// <summary>
        /// 랜덤 맵 생성 (격자 구조)
        /// </summary>
        public static MapData GenerateMap(MapGenerationConfig config, int seed)
        {
            if (config == null)
            {
                Debug.LogError("[MapGen] MapGenerationConfig is null! Cannot generate map.");
                return null;
            }

            Random.InitState(seed);

            MapData map = new MapData();
            map.seed = seed;

            Debug.Log($"[MapGen] Creating grid map with seed: {seed}, totalLevels: {config.totalLevels}");

            // 레벨별로 격자 생성
            for (int levelIndex = 1; levelIndex <= config.totalLevels; levelIndex++)
            {
                LevelData levelData = GenerateLevel(levelIndex, config);
                map.levels.Add(levelData);
            }

            // 생성된 전체 노드 수 확인
            int totalNodes = map.GetAllNodes().Count;
            Debug.Log($"[MapGen] Grid map generated with {totalNodes} total nodes across {config.totalLevels} levels");

            return map;
        }

        /// <summary>
        /// 단일 레벨 생성 (여러 스테이지 그룹 포함)
        /// </summary>
        private static LevelData GenerateLevel(int levelIndex, MapGenerationConfig config)
        {
            LevelData levelData = new LevelData();
            int stagesInLevel = config.GetStageCountForLevel(levelIndex);

            Debug.Log($"[MapGen] Generating Level {levelIndex} with {stagesInLevel} stages + boss");

            // 일반 스테이지들 생성
            for (int stageIndex = 1; stageIndex <= stagesInLevel; stageIndex++)
            {
                StageGroup stageGroup = GenerateStageGroup(levelIndex, stageIndex, config);
                levelData.stages.Add(stageGroup);
            }

            // 보스 스테이지 생성 (선택지 1개만)
            StageGroup bossStage = GenerateBossStage(levelIndex, stagesInLevel + 1);
            levelData.stages.Add(bossStage);

            Debug.Log($"[MapGen] Level {levelIndex} created with {levelData.stages.Count} stages");

            return levelData;
        }

        /// <summary>
        /// 단일 스테이지 그룹 생성 (N개 선택지)
        /// </summary>
        private static StageGroup GenerateStageGroup(int levelIndex, int stageIndex, MapGenerationConfig config)
        {
            StageGroup stageGroup = new StageGroup();

            // 선택지 개수 결정 (2-3개)
            int choiceCount = Random.Range(MIN_CHOICES, MAX_CHOICES + 1);

            // 선택지 노드들 생성
            for (int i = 0; i < choiceCount; i++)
            {
                StageType stageType = config.stageTypeConfig.GetRandomStageType(levelIndex);
                StageNode node = new StageNode(levelIndex, stageIndex, stageType);
                stageGroup.choices.Add(node);

                Debug.Log($"[MapGen] Created choice {i + 1}/{choiceCount}: {node.GetNodeID()}");
            }

            return stageGroup;
        }

        /// <summary>
        /// 보스 스테이지 생성 (선택지 1개만)
        /// </summary>
        private static StageGroup GenerateBossStage(int levelIndex, int stageIndex)
        {
            StageGroup bossStage = new StageGroup();
            StageNode bossNode = new StageNode(levelIndex, stageIndex, StageType.Boss);
            bossStage.choices.Add(bossNode);

            Debug.Log($"[MapGen] Created boss stage: {bossNode.GetNodeID()}");

            return bossStage;
        }
    }
}
