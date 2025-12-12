using UnityEngine;

namespace MatchBattle
{
    public class BlockVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Vector3 originalScale;
        private Color originalColor;
        private bool isHighlighted = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;
            originalColor = spriteRenderer.color;
        }

        public void SetHighlight(bool highlighted)
        {
            if (isHighlighted == highlighted)
            {
                return;
            }

            isHighlighted = highlighted;

            if (highlighted)
            {
                // 밝게 + 확대
                spriteRenderer.color = originalColor * 1.5f;
                transform.localScale = originalScale * 1.1f;
            }
            else
            {
                // 원래대로
                spriteRenderer.color = originalColor;
                transform.localScale = originalScale;
            }
        }
    }
}
