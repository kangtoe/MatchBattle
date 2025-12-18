using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    // 경로 완료 이벤트 데이터
    public class PathCompletedEventArgs : EventArgs
    {
        public List<Block> Path { get; }
        public BlockColor Color { get; }
        public int BlockCount { get; }
        public bool IsValid { get; }

        public PathCompletedEventArgs(List<Block> path, BlockColor color, bool isValid)
        {
            Path = new List<Block>(path); // 복사본 생성
            Color = color;
            BlockCount = path.Count;
            IsValid = isValid;
        }
    }

    public class BoardInputHandler : MonoBehaviour
    {
        private BoardManager boardManager;
        private PathVisualizer pathVisualizer;
        private Camera mainCamera;

        // 경로 데이터
        private List<Block> currentPath = new List<Block>();
        private BlockColor currentColor;
        private bool isDragging = false;

        // 입력 활성화 상태
        private bool inputEnabled = true;

        // 이벤트 선언
        public event EventHandler<PathCompletedEventArgs> OnPathCompleted;
        public event EventHandler<PathCompletedEventArgs> OnPathFailed;

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
            // 입력 비활성화 상태면 무시
            if (!inputEnabled)
            {
                return;
            }

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

        // ===========================================
        // 입력 제어
        // ===========================================

        /// <summary>
        /// 보드 입력 활성화 (플레이어 턴 시작 시)
        /// </summary>
        public void EnableInput()
        {
            inputEnabled = true;
            Debug.Log("[BoardInput] Input enabled");
        }

        /// <summary>
        /// 보드 입력 비활성화 (플레이어 턴 종료 시)
        /// </summary>
        public void DisableInput()
        {
            inputEnabled = false;

            // 진행 중인 드래그가 있다면 강제로 취소
            if (isDragging)
            {
                isDragging = false;
                ClearPath();
                Debug.Log("[BoardInput] Input disabled - path cleared");
            }
            else
            {
                Debug.Log("[BoardInput] Input disabled");
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

            // 최소 1개 이상 선택했는가?
            if (currentPath.Count >= 1)
            {
                bool isOptimal = currentPath.Count >= 3;
                string message = isOptimal
                    ? $"✓ Optimal path! {currentPath.Count} blocks connected ({currentColor})"
                    : $"⚠ Short path: {currentPath.Count} blocks ({currentColor}) - reduced effect";

                Debug.Log(message);

                // 이벤트 발생: 경로 완성 (1개라도 효과 발동)
                var eventArgs = new PathCompletedEventArgs(currentPath, currentColor, true);
                OnPathCompleted?.Invoke(this, eventArgs);

                // 블록 제거 및 효과 적용
                boardManager.RemoveBlocks(new List<Block>(currentPath));
            }
            else
            {
                Debug.Log($"✗ No blocks selected");

                // 이벤트 발생: 아무것도 선택 안함
                var eventArgs = new PathCompletedEventArgs(currentPath, currentColor, false);
                OnPathFailed?.Invoke(this, eventArgs);
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

            Debug.Log($"Path started: {block.color} block at {block.gridPos}");
        }

        void TryAddBlockToPath(Block block)
        {
            // 이미 경로에 있는 블록인가?
            int existingIndex = currentPath.IndexOf(block);
            if (existingIndex >= 0)
            {
                // Undo: 해당 블록 이후의 모든 블록 제거
                if (existingIndex < currentPath.Count - 1)
                {
                    // 뒤에서부터 제거
                    for (int i = currentPath.Count - 1; i > existingIndex; i--)
                    {
                        Block removed = currentPath[i];
                        HighlightBlock(removed, false);
                        currentPath.RemoveAt(i);
                        Debug.Log($"Undo: Removed {removed.color} block at {removed.gridPos}");
                    }

                    // 블록 제거 후 currentColor 재계산
                    RecalculateCurrentColor();
                    pathVisualizer.UpdatePath(currentPath, currentColor);
                }
                return;
            }

            // 마지막 블록과 인접하고 연결 가능한가?
            Block lastBlock = currentPath[currentPath.Count - 1];

            if (!GridHelper.IsAdjacent(lastBlock.gridPos, block.gridPos) || !CanConnect(lastBlock, block))
            {
                // 역추적: 경로를 거슬러 올라가면서 연결 가능한 블록 찾기
                bool found = false;
                for (int i = currentPath.Count - 2; i >= 0; i--)
                {
                    if (GridHelper.IsAdjacent(currentPath[i].gridPos, block.gridPos) &&
                        CanConnect(currentPath[i], block))
                    {
                        // i 이후의 모든 블록 제거 (자동 되돌리기)
                        for (int j = currentPath.Count - 1; j > i; j--)
                        {
                            Block removed = currentPath[j];
                            HighlightBlock(removed, false);
                            currentPath.RemoveAt(j);
                            Debug.Log($"Auto-undo: Removed {removed.color} block at {removed.gridPos}");
                        }

                        // 블록 제거 후 currentColor 재계산
                        RecalculateCurrentColor();
                        pathVisualizer.UpdatePath(currentPath, currentColor);
                        found = true;
                        break;
                    }
                }

                // 여전히 연결할 수 없으면 무시
                if (!found)
                {
                    return;
                }
            }

            // 경로에 추가
            currentPath.Add(block);

            // 와일드카드가 아닌 블록이면 현재 색상 업데이트 (색상 전환)
            if (block.color != BlockColor.Purple)
            {
                currentColor = block.color;
            }

            HighlightBlock(block, true);
            pathVisualizer.UpdatePath(currentPath, currentColor);

            Debug.Log($"Added to path: {block.color} block at {block.gridPos} (total: {currentPath.Count})");
        }

        /// <summary>
        /// 현재 경로를 기반으로 currentColor 재계산
        /// </summary>
        void RecalculateCurrentColor()
        {
            // 경로를 역순으로 순회하며 첫 번째 non-purple 블록 찾기
            for (int i = currentPath.Count - 1; i >= 0; i--)
            {
                if (currentPath[i].color != BlockColor.Purple)
                {
                    currentColor = currentPath[i].color;
                    Debug.Log($"[Color Recalc] currentColor updated to {currentColor}");
                    return;
                }
            }

            // 모든 블록이 Purple이면 currentColor도 Purple
            currentColor = BlockColor.Purple;
            Debug.Log($"[Color Recalc] All blocks are Purple, currentColor = Purple");
        }

        bool CanConnect(Block lastBlock, Block newBlock)
        {
            // Purple to Purple: 항상 연결 가능
            if (lastBlock.color == BlockColor.Purple && newBlock.color == BlockColor.Purple)
            {
                return true;
            }

            // 새 블록이 Purple인 경우
            if (newBlock.color == BlockColor.Purple)
            {
                // 마지막 블록이 현재 경로 색상과 일치해야 함
                return lastBlock.color == currentColor;
            }

            // 마지막 블록이 Purple인 경우
            if (lastBlock.color == BlockColor.Purple)
            {
                // 경로가 Purple로 시작했다면 (아직 색상이 정해지지 않음)
                // 모든 색상 연결 가능 (이 블록이 경로 색상을 정함)
                if (currentColor == BlockColor.Purple)
                {
                    return true;
                }

                // 그 외의 경우, 새 블록이 현재 경로 색상과 일치해야 함
                return newBlock.color == currentColor;
            }

            // 일반 케이스: 같은 색상만 연결 가능
            return newBlock.color == currentColor;
        }

        void HighlightBlock(Block block, bool highlight)
        {
            if (block == null || block.gameObject == null)
            {
                Debug.LogWarning("HighlightBlock: Block or GameObject is null");
                return;
            }

            BlockVisual visual = block.gameObject.GetComponent<BlockVisual>();
            if (visual != null)
            {
                visual.SetHighlight(highlight);
            }
            else
            {
                Debug.LogError($"BlockVisual component not found on {block.gameObject.name}");
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
