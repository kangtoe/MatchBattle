using UnityEngine;
using TMPro;

namespace MatchBattle
{
    public class BlockVisual : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer backgroundSprite;  // 배경/테두리
        [SerializeField] private SpriteRenderer iconSprite;        // 아이콘
        [SerializeField] private TextMeshPro nameText;             // 이름 텍스트

        private Vector3 originalScale;
        private Color originalColor;
        private bool isHighlighted = false;

        public SpriteRenderer BackgroundSprite => backgroundSprite;
        public SpriteRenderer IconSprite => iconSprite;
        public TextMeshPro NameText => nameText;

        void Awake()
        {
            originalScale = transform.localScale;
            if (backgroundSprite != null)
            {
                originalColor = backgroundSprite.color;
            }
        }

        public void SetHighlight(bool highlighted)
        {
            if (isHighlighted == highlighted || backgroundSprite == null)
            {
                return;
            }

            isHighlighted = highlighted;

            if (highlighted)
            {
                // 밝게 + 확대
                backgroundSprite.color = originalColor * 1.5f;
                transform.localScale = originalScale * 1.1f;
            }
            else
            {
                // 원래대로
                backgroundSprite.color = originalColor;
                transform.localScale = originalScale;
            }
        }
    }
}
