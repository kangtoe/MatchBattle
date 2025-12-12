using UnityEngine;

namespace MatchBattle
{
    public class Block
    {
        public BlockColor color;
        public BlockType type;
        public Vector2Int gridPos;
        public GameObject gameObject;
        public SpriteRenderer sprite;

        // 효과 데이터 (Phase 2에서 사용)
        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int goldValue;
        // public StatusEffect[] statusEffects; // Phase 2
    }
}
