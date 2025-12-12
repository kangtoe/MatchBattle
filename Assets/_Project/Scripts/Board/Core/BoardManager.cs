using UnityEngine;

namespace MatchBattle
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Block Prefabs")]
        [SerializeField] private GameObject redBlockPrefab;
        [SerializeField] private GameObject blueBlockPrefab;
        [SerializeField] private GameObject yellowBlockPrefab;
        [SerializeField] private GameObject brownBlockPrefab;
        [SerializeField] private GameObject purpleBlockPrefab;

        [Header("References")]
        [SerializeField] private Transform boardParent;

        // 보드 데이터
        private Block[,] board = new Block[GridHelper.BOARD_SIZE, GridHelper.BOARD_SIZE];

        void Start()
        {
            InitializeBoard();
        }

        void InitializeBoard()
        {
            // 보드 부모가 없으면 생성
            if (boardParent == null)
            {
                GameObject parent = new GameObject("Board");
                boardParent = parent.transform;
            }

            // 모든 칸에 블록 생성
            for (int x = 0; x < GridHelper.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GridHelper.BOARD_SIZE; y++)
                {
                    // Phase 1: 간단하게 Red와 Blue만 랜덤 생성
                    BlockType type = Random.value > 0.5f ? BlockType.Sword : BlockType.Shield;
                    Block block = CreateBlock(type, x, y);
                    board[x, y] = block;
                }
            }

            Debug.Log("Board initialized: 8x8 grid created");
        }

        Block CreateBlock(BlockType type, int x, int y)
        {
            // 블록 색상 결정
            BlockColor color = GetBlockColor(type);

            // 색상에 맞는 프리팹 선택
            GameObject prefab = GetPrefabForColor(color);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not assigned for color: {color}");
                return null;
            }

            // 프리팹 인스턴스 생성
            Vector3 worldPos = GridHelper.GridToWorld(x, y);
            GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, boardParent);
            obj.name = $"Block_{color}_{x}_{y}";

            // Block 데이터 설정
            Block block = new Block();
            block.type = type;
            block.color = color;
            block.gridPos = new Vector2Int(x, y);
            block.gameObject = obj;
            block.sprite = obj.GetComponent<SpriteRenderer>();

            // 효과 데이터 설정 (Phase 1: 기본값)
            SetBlockEffect(block, type);

            return block;
        }

        GameObject GetPrefabForColor(BlockColor color)
        {
            switch (color)
            {
                case BlockColor.Red: return redBlockPrefab;
                case BlockColor.Blue: return blueBlockPrefab;
                case BlockColor.Yellow: return yellowBlockPrefab;
                case BlockColor.Brown: return brownBlockPrefab;
                case BlockColor.Purple: return purpleBlockPrefab;
                default: return redBlockPrefab;
            }
        }

        BlockColor GetBlockColor(BlockType type)
        {
            switch (type)
            {
                // 붉은 블록
                case BlockType.Sword:
                case BlockType.Axe:
                case BlockType.Fire:
                    return BlockColor.Red;

                // 푸른 블록
                case BlockType.Shield:
                case BlockType.Dodge:
                case BlockType.Counter:
                    return BlockColor.Blue;

                // 노란 블록
                case BlockType.Gold:
                case BlockType.Gem:
                case BlockType.Bonus:
                    return BlockColor.Yellow;

                // 갈색 블록
                case BlockType.Trash:
                case BlockType.Potion:
                case BlockType.Buff:
                    return BlockColor.Brown;

                // 보라 블록
                case BlockType.Wildcard:
                    return BlockColor.Purple;

                default:
                    return BlockColor.Red;
            }
        }

        void SetBlockEffect(Block block, BlockType type)
        {
            // Phase 1: 기본값만 설정
            // Phase 2에서 상세한 효과 구현
            switch (type)
            {
                case BlockType.Sword:
                    block.attackValue = 3;
                    break;
                case BlockType.Shield:
                    block.defenseValue = 3;
                    break;
                default:
                    break;
            }
        }

        // Public API
        public Block GetBlockAt(int x, int y)
        {
            if (!GridHelper.IsInBounds(x, y))
                return null;

            return board[x, y];
        }

        public Block GetBlockAt(Vector2Int gridPos)
        {
            return GetBlockAt(gridPos.x, gridPos.y);
        }

        // 디버그용 그리드 시각화
        void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            for (int x = 0; x <= GridHelper.BOARD_SIZE; x++)
            {
                Vector3 start = GridHelper.GridToWorld(x, 0);
                Vector3 end = GridHelper.GridToWorld(x, GridHelper.BOARD_SIZE);
                start.x -= 0.5f;
                end.x -= 0.5f;
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= GridHelper.BOARD_SIZE; y++)
            {
                Vector3 start = GridHelper.GridToWorld(0, y);
                Vector3 end = GridHelper.GridToWorld(GridHelper.BOARD_SIZE, y);
                start.y -= 0.5f;
                end.y -= 0.5f;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
