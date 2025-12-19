using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Block Pool")]
        [SerializeField] private BlockPool blockPool;

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

            // BlockPool이 없으면 자동 생성
            if (blockPool == null)
            {
                Debug.LogWarning("BlockPool not assigned. Creating default pool.");
                GameObject poolObj = new GameObject("BlockPool");
                blockPool = poolObj.AddComponent<BlockPool>();
            }

            // 모든 칸에 블록 생성
            for (int x = 0; x < GridHelper.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GridHelper.BOARD_SIZE; y++)
                {
                    // BlockPool에서 랜덤 블록 데이터 가져오기
                    BlockData blockData = blockPool.GetRandomBlockData();
                    Block block = CreateBlock(blockData, x, y);
                    board[x, y] = block;
                }
            }

            Debug.Log("Board initialized: 8x8 grid created");
        }

        Block CreateBlock(BlockData blockData, int x, int y)
        {
            if (blockData == null)
            {
                Debug.LogError($"BlockData is null. Cannot create block.");
                return null;
            }

            if (blockData.prefab == null)
            {
                Debug.LogError($"Prefab not assigned in BlockData: {blockData.name}");
                return null;
            }

            // 프리팹 인스턴스 생성
            Vector3 worldPos = GridHelper.GridToWorld(x, y);
            GameObject obj = Instantiate(blockData.prefab, worldPos, Quaternion.identity, boardParent);
            obj.name = $"Block_{blockData.name}_{x}_{y}";

            // Block 데이터 설정
            Block block = new Block();
            block.gridPos = new Vector2Int(x, y);
            block.gameObject = obj;
            block.sprite = obj.GetComponent<SpriteRenderer>();

            // BlockData에서 색상 및 효과 적용
            blockData.ApplyToBlock(block);

            return block;
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

        // Phase 2: 블록 제거 (Public API)
        public void RemoveBlocks(List<Block> path)
        {
            StartCoroutine(RemoveBlocksSequence(path));
        }

        // Phase 2: 블록 효과 적용
        void ApplyBlockEffects(List<Block> path)
        {
            // 효과 누적
            int totalAttack = 0;
            int totalDefense = 0;
            int totalGold = 0;
            int totalHeal = 0;

            // 각 블록의 효과 합산
            foreach (Block block in path)
            {
                totalAttack += block.attackValue;
                totalDefense += block.defenseValue;
                totalGold += block.goldValue;
                totalHeal += block.healValue;
            }

            // TODO: 나중에 연쇄 보너스 구현
            // float bonus = GetChainBonus(path.Count);
            // totalAttack = Mathf.RoundToInt(totalAttack * bonus);
            // totalDefense = Mathf.RoundToInt(totalDefense * bonus);

            // 효과 발동
            if (totalAttack > 0)
            {
                CombatManager.Instance?.DealDamage(totalAttack);
            }

            if (totalDefense > 0)
            {
                CombatManager.Instance?.AddDefense(totalDefense);
            }

            if (totalGold > 0)
            {
                GameManager.Instance?.AddGold(totalGold);
            }

            if (totalHeal > 0)
            {
                CombatManager.Instance?.HealPlayer(totalHeal);
            }
        }

        // TODO: Phase 3 - 연쇄 보너스 시스템 구현 예정
        // float GetChainBonus(int chainLength)
        // {
        //     // 예시: 1개=30%, 2개=60%, 3개=100%, 4개=110%, 5개=125%, 6개+=150%
        //     if (chainLength == 1) return 0.3f;
        //     if (chainLength == 2) return 0.6f;
        //     if (chainLength == 3) return 1.0f;
        //     if (chainLength == 4) return 1.1f;
        //     if (chainLength == 5) return 1.25f;
        //     return 1.5f;
        // }

        // Phase 2: 블록 제거 시퀀스
        IEnumerator RemoveBlocksSequence(List<Block> path)
        {
            // 효과 적용과 턴 종료는 CombatManager.HandlePathCompleted에서 처리함

            // 1. 블록 제거 애니메이션
            foreach (Block block in path)
            {
                if (block.gameObject != null)
                {
                    Destroy(block.gameObject);
                }
                board[block.gridPos.x, block.gridPos.y] = null;
            }

            yield return new WaitForSeconds(0.3f);

            // 2. 낙하 처리
            yield return StartCoroutine(DropBlocks());

            // 3. 빈 칸 채우기
            FillEmptySpaces();

            yield return new WaitForSeconds(0.2f);
        }

        // Phase 2: 낙하 알고리즘
        IEnumerator DropBlocks()
        {
            bool moved = true;

            while (moved)
            {
                moved = false;

                // 아래에서 위로 스캔
                for (int x = 0; x < GridHelper.BOARD_SIZE; x++)
                {
                    for (int y = 0; y < GridHelper.BOARD_SIZE - 1; y++)
                    {
                        // 현재 칸이 비어있고 위에 블록이 있는 경우
                        if (board[x, y] == null && board[x, y + 1] != null)
                        {
                            // 블록 이동
                            board[x, y] = board[x, y + 1];
                            board[x, y].gridPos = new Vector2Int(x, y);
                            board[x, y + 1] = null;

                            // 애니메이션
                            Vector3 targetPos = GridHelper.GridToWorld(x, y);
                            StartCoroutine(MoveBlock(board[x, y], targetPos, 0.1f));

                            moved = true;
                        }
                    }
                }

                if (moved)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        // Phase 2: 블록 이동 애니메이션
        IEnumerator MoveBlock(Block block, Vector3 target, float duration)
        {
            if (block.gameObject == null) yield break;

            Vector3 start = block.gameObject.transform.position;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                block.gameObject.transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            block.gameObject.transform.position = target;
        }

        // Phase 2: 빈 칸 채우기
        void FillEmptySpaces()
        {
            for (int x = 0; x < GridHelper.BOARD_SIZE; x++)
            {
                for (int y = 0; y < GridHelper.BOARD_SIZE; y++)
                {
                    if (board[x, y] == null)
                    {
                        // BlockPool에서 랜덤 블록 데이터 가져오기
                        BlockData blockData = blockPool.GetRandomBlockData();
                        Block newBlock = CreateBlock(blockData, x, y);
                        board[x, y] = newBlock;

                        // 위에서 떨어지는 애니메이션
                        Vector3 startPos = GridHelper.GridToWorld(x, GridHelper.BOARD_SIZE + 2);
                        Vector3 targetPos = GridHelper.GridToWorld(x, y);
                        newBlock.gameObject.transform.position = startPos;
                        StartCoroutine(MoveBlock(newBlock, targetPos, 0.2f));
                    }
                }
            }
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
