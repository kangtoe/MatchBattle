using UnityEngine;

namespace MatchBattle
{
    public class Block
    {
        public BlockColor color;
        public Vector2Int gridPos;
        public GameObject gameObject;
        public BlockVisual visual;  // 비주얼 컴포넌트 참조

        // 효과 데이터
        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int goldValue;
        // public StatusEffect[] statusEffects; // Phase 3
    }
}
