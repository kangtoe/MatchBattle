using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    public class BoardInputHandler : MonoBehaviour
    {
        private BoardManager boardManager;
        private PathVisualizer pathVisualizer;
        private Camera mainCamera;

        // 경로 데이터
        private List<Block> currentPath = new List<Block>();
        private BlockColor currentColor;
        private bool isDragging = false;

        void Start()
        {
            boardManager = GetComponent<BoardManager>();
            pathVisualizer = FindObjectOfType<PathVisualizer>();
            mainCamera = Camera.main;

            if (boardManager == null)
            {
                Debug.LogError("BoardInputHandler: BoardManager not found!");
            }

            if (pathVisualizer == null)
            {
                Debug.LogError("BoardInputHandler: PathVisualizer not found!");
            }
        }

        void Update()
        {
            // 마우스 버튼 Down (드래그 시작)
            if (Input.GetMouseButtonDown(0))
            {
                HandleInputDown(Input.mousePosition);
            }
            // 마우스 버튼 Hold (드래그 중)
            else if (Input.GetMouseButton(0) && isDragging)
            {
                HandleInputDrag(Input.mousePosition);
            }
            // 마우스 버튼 Up (드래그 종료)
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                HandleInputUp();
            }
        }

        void HandleInputDown(Vector3 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2Int gridPos = GridHelper.WorldToGrid(worldPos);

            Block block = boardManager.GetBlockAt(gridPos);
            if (block != null)
            {
                isDragging = true;
                StartPath(block);
            }
        }

        void HandleInputDrag(Vector3 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2Int gridPos = GridHelper.WorldToGrid(worldPos);

            Block block = boardManager.GetBlockAt(gridPos);
            if (block != null)
            {
                TryAddBlockToPath(block);
            }
        }

        void HandleInputUp()
        {
            isDragging = false;

            // 최소 3개 이상인가?
            if (currentPath.Count >= 3)
            {
                Debug.Log($"✓ Valid path! {currentPath.Count} blocks connected ({currentColor})");
                // Phase 2: ApplyBlockEffects(currentPath);
                // Phase 2: RemoveBlocks(currentPath);
            }
            else
            {
                Debug.Log($"✗ Path too short: {currentPath.Count} blocks (need 3+)");
            }

            ClearPath();
        }

        void StartPath(Block block)
        {
            currentPath.Clear();
            currentPath.Add(block);
            currentColor = block.color;

            // 시각적 피드백
            HighlightBlock(block, true);
            pathVisualizer.UpdatePath(currentPath, currentColor);

            Debug.Log($"Path started: {block.type} at {block.gridPos}");
        }

        void TryAddBlockToPath(Block block)
        {
            // 이미 경로에 있는 블록인가?
            int existingIndex = currentPath.IndexOf(block);
            if (existingIndex >= 0)
            {
                // Undo: 바로 이전 블록으로 돌아가는 경우
                if (existingIndex == currentPath.Count - 2)
                {
                    Block removed = currentPath[currentPath.Count - 1];
                    currentPath.RemoveAt(currentPath.Count - 1);
                    HighlightBlock(removed, false);
                    pathVisualizer.UpdatePath(currentPath, currentColor);
                    Debug.Log($"Undo: Removed {removed.type} at {removed.gridPos}");
                }
                return;
            }

            // 마지막 블록과 인접한가?
            Block lastBlock = currentPath[currentPath.Count - 1];
            if (!GridHelper.IsAdjacent(lastBlock.gridPos, block.gridPos))
            {
                return;
            }

            // 연결 가능한가?
            if (!CanConnect(lastBlock, block))
            {
                Debug.Log($"Cannot connect: {lastBlock.color} -> {block.color}");
                return;
            }

            // 경로에 추가
            currentPath.Add(block);
            HighlightBlock(block, true);
            pathVisualizer.UpdatePath(currentPath, currentColor);

            Debug.Log($"Added to path: {block.type} at {block.gridPos} (total: {currentPath.Count})");
        }

        bool CanConnect(Block lastBlock, Block newBlock)
        {
            // 와일드카드는 모든 색과 연결 가능
            if (lastBlock.color == BlockColor.Purple || newBlock.color == BlockColor.Purple)
            {
                return true;
            }

            // 같은 색상만 연결 가능
            return newBlock.color == currentColor;
        }

        void HighlightBlock(Block block, bool highlight)
        {
            BlockVisual visual = block.gameObject.GetComponent<BlockVisual>();
            if (visual != null)
            {
                visual.SetHighlight(highlight);
            }
        }

        void ClearPath()
        {
            // 모든 하이라이트 해제
            foreach (Block block in currentPath)
            {
                HighlightBlock(block, false);
            }

            currentPath.Clear();
            pathVisualizer.ClearPath();
        }
    }
}
