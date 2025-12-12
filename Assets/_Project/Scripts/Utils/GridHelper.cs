using UnityEngine;

namespace MatchBattle
{
    public static class GridHelper
    {
        public const float CELL_SIZE = 1.0f;
        public const int BOARD_SIZE = 8;

        // 그리드 좌표 → 월드 좌표 변환
        // 보드를 중앙에 배치하기 위해 -3.5f 오프셋 적용
        public static Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                x - 3.5f,
                y - 3.5f,
                0
            );
        }

        // 월드 좌표 → 그리드 좌표 변환
        public static Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x + 3.5f);
            int y = Mathf.RoundToInt(worldPos.y + 3.5f);
            return new Vector2Int(x, y);
        }

        // 그리드 범위 체크
        public static bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }

        public static bool IsInBounds(Vector2Int pos)
        {
            return IsInBounds(pos.x, pos.y);
        }

        // 인접성 체크 (상하좌우 + 대각선 모두 허용)
        public static bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            int dx = Mathf.Abs(pos1.x - pos2.x);
            int dy = Mathf.Abs(pos1.y - pos2.y);

            // 8방향 모두 허용 (상하좌우 + 대각선)
            // 단, 같은 위치는 제외
            return dx <= 1 && dy <= 1 && (dx + dy) > 0;
        }
    }
}
