using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 블록 풀 시스템: 가중치 기반으로 블록을 생성합니다.
    /// 게임이 진행되면서 보상으로 블록을 추가/제거/업그레이드 할 수 있습니다.
    /// </summary>
    public class BlockPool : MonoBehaviour
    {
        [System.Serializable]
        public class BlockEntry
        {
            public BlockData data;
            public int weight = 10;

            public BlockEntry(BlockData data, int weight)
            {
                this.data = data;
                this.weight = weight;
            }
        }

        [System.Serializable]
        public class BlockWeightConfig
        {
            public BlockData blockData;
            [Tooltip("가중치. 높을수록 자주 생성됨")]
            public int weight = 10;
        }

        [System.Serializable]
        public class BlockColorMapping
        {
            public BlockColor blockColor;
            public Color displayColor;
        }

        [Header("Block Prefab")]
        [Tooltip("모든 블록이 사용할 공통 프리팹 (배경 + 아이콘 구조)")]
        [SerializeField] private GameObject blockPrefab;

        [Header("Block Colors")]
        [Tooltip("각 블록 색상에 대응되는 실제 표시 색상")]
        [SerializeField] private List<BlockColorMapping> colorMappings = new List<BlockColorMapping>();

        [Header("Initial Pool Configuration")]
        [Tooltip("게임 시작 시 사용할 블록 풀과 각 블록의 가중치 설정")]
        [SerializeField] private List<BlockWeightConfig> initialPoolConfig = new List<BlockWeightConfig>();

        [Header("Current Pool (Runtime)")]
        [SerializeField] private List<BlockEntry> currentPool = new List<BlockEntry>();

        private int totalWeight = 0;

        void Awake()
        {
            InitializePool();
        }

        void InitializePool()
        {
            currentPool = new List<BlockEntry>();
            foreach (var config in initialPoolConfig)
            {
                if (config != null && config.blockData != null)
                {
                    currentPool.Add(new BlockEntry(config.blockData, config.weight));
                }
            }
            RecalculateTotalWeight();
        }

        void RecalculateTotalWeight()
        {
            totalWeight = 0;
            foreach (var entry in currentPool)
            {
                totalWeight += entry.weight;
            }
        }

        /// <summary>
        /// 공통 블록 프리팹을 반환합니다.
        /// </summary>
        public GameObject GetBlockPrefab()
        {
            return blockPrefab;
        }

        /// <summary>
        /// 특정 BlockColor에 대응되는 표시 색상을 반환합니다.
        /// </summary>
        public Color GetColorForBlockColor(BlockColor blockColor)
        {
            var mapping = colorMappings.Find(m => m.blockColor == blockColor);
            if (mapping != null)
            {
                return mapping.displayColor;
            }

            Debug.LogWarning($"Color mapping not found for {blockColor}. Using white.");
            return Color.white;
        }

        /// <summary>
        /// 가중치 기반으로 랜덤 블록 데이터를 선택합니다.
        /// </summary>
        public BlockData GetRandomBlockData()
        {
            if (currentPool.Count == 0)
            {
                Debug.LogWarning("BlockPool is empty!");
                return null;
            }

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var entry in currentPool)
            {
                cumulativeWeight += entry.weight;
                if (randomValue < cumulativeWeight)
                {
                    return entry.data;
                }
            }

            // Fallback
            return currentPool[0].data;
        }


        /// <summary>
        /// 새 블록을 풀에 추가합니다. (보상 시스템에서 사용)
        /// </summary>
        public void AddBlock(BlockData data, int weight = 10)
        {
            // 이미 풀에 있는지 확인 (ScriptableObject 참조 비교)
            BlockEntry existing = currentPool.Find(e => e.data == data);
            if (existing != null)
            {
                Debug.LogWarning($"Block {data.name} already in pool. Use IncreaseWeight instead.");
                return;
            }

            currentPool.Add(new BlockEntry(data, weight));
            RecalculateTotalWeight();
            Debug.Log($"Added {data.name} to pool with weight {weight}");
        }

        /// <summary>
        /// 특정 블록의 가중치를 증가시킵니다. (보상 시스템에서 사용)
        /// </summary>
        public void IncreaseWeight(BlockData data, int amount)
        {
            BlockEntry entry = currentPool.Find(e => e.data == data);
            if (entry != null)
            {
                entry.weight += amount;
                RecalculateTotalWeight();
                Debug.Log($"Increased {data.name} weight by {amount}. New weight: {entry.weight}");
            }
            else
            {
                Debug.LogWarning($"Block {data.name} not found in pool.");
            }
        }

        /// <summary>
        /// 특정 블록을 풀에서 제거합니다. (고급 보상에서 사용)
        /// </summary>
        public void RemoveBlock(BlockData data)
        {
            BlockEntry entry = currentPool.Find(e => e.data == data);
            if (entry != null)
            {
                currentPool.Remove(entry);
                RecalculateTotalWeight();
                Debug.Log($"Removed {data.name} from pool");
            }
            else
            {
                Debug.LogWarning($"Block {data.name} not found in pool.");
            }
        }

        /// <summary>
        /// 현재 풀 상태를 디버그 로그로 출력합니다.
        /// </summary>
        public void PrintPoolStatus()
        {
            Debug.Log("=== Block Pool Status ===");
            Debug.Log($"Total Weight: {totalWeight}");
            foreach (var entry in currentPool)
            {
                float percentage = (float)entry.weight / totalWeight * 100f;
                Debug.Log($"{entry.data.name}: {entry.weight} ({percentage:F1}%)");
            }
        }

        /// <summary>
        /// Reset 메서드: 컴포넌트가 처음 추가될 때 기본 색상 매핑을 설정합니다.
        /// </summary>
        void Reset()
        {
            colorMappings = new List<BlockColorMapping>
            {
                new BlockColorMapping { blockColor = BlockColor.Red, displayColor = new Color(0.9f, 0.2f, 0.2f) },
                new BlockColorMapping { blockColor = BlockColor.Blue, displayColor = new Color(0.2f, 0.5f, 0.9f) },
                new BlockColorMapping { blockColor = BlockColor.Yellow, displayColor = new Color(0.95f, 0.85f, 0.2f) },
                new BlockColorMapping { blockColor = BlockColor.Purple, displayColor = new Color(0.7f, 0.3f, 0.8f) },
                new BlockColorMapping { blockColor = BlockColor.Brown, displayColor = new Color(0.6f, 0.4f, 0.2f) }
            };
        }
    }
}
