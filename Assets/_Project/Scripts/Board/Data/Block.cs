using UnityEngine;

namespace MatchBattle
{
    public class Block
    {
        public BlockColor color;
        public Vector2Int gridPos;
        public GameObject gameObject;
        public SpriteRenderer backgroundSprite;  // 배경/테두리 (색상 변경용)
        public SpriteRenderer iconSprite;        // 아이콘 (스프라이트 교체용)

        // 효과 데이터
        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int goldValue;
        // public StatusEffect[] statusEffects; // Phase 3
    }
}
