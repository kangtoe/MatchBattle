using UnityEngine;

namespace MatchBattle
{
    public class BlockVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Vector3 originalScale;
        private bool isHighlighted = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
        }

        public void SetHighlight(bool highlighted)
        {
            if (isHighlighted == highlighted) return;

            isHighlighted = highlighted;

            if (highlighted)
            {
                // 밝게 + 확대
                spriteRenderer.color = Color.white * 1.5f;
                transform.localScale = originalScale * 1.1f;
            }
            else
            {
                // 원래대로
                spriteRenderer.color = Color.white;
                transform.localScale = originalScale;
            }
        }
    }
}
