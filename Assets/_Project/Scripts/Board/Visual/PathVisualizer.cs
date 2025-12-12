using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    public class PathVisualizer : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        void Awake()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            ConfigureLineRenderer();
        }

        void ConfigureLineRenderer()
        {
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 0;
            lineRenderer.sortingOrder = 10; // UI 레이어 위에 표시
            lineRenderer.useWorldSpace = true;

            // z 위치를 -1로 설정해서 블록 위에 표시
            lineRenderer.transform.position = new Vector3(0, 0, -1);
        }

        public void UpdatePath(List<Block> path, BlockColor color)
        {
            if (path == null || path.Count == 0)
            {
                ClearPath();
                return;
            }

            lineRenderer.positionCount = path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 pos = path[i].gameObject.transform.position;
                pos.z = -1; // 블록 위에 표시
                lineRenderer.SetPosition(i, pos);
            }

            // 색상 설정
            Color lineColor = GetColorForBlockColor(color);
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }

        public void ClearPath()
        {
            lineRenderer.positionCount = 0;
        }

        Color GetColorForBlockColor(BlockColor blockColor)
        {
            switch (blockColor)
            {
                case BlockColor.Red:
                    return new Color(1f, 0.3f, 0.3f);
                case BlockColor.Blue:
                    return new Color(0.3f, 0.5f, 1f);
                case BlockColor.Yellow:
                    return new Color(1f, 0.9f, 0.3f);
                case BlockColor.Brown:
                    return new Color(0.6f, 0.4f, 0.2f);
                case BlockColor.Purple:
                    return new Color(0.8f, 0.3f, 1f);
                default:
                    return Color.white;
            }
        }
    }
}
