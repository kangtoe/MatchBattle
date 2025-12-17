using UnityEngine;

namespace MatchBattle
{
    [CreateAssetMenu(fileName = "BlockData", menuName = "MatchBattle/BlockData")]
    public class BlockData : ScriptableObject
    {
        [Header("Block Info")]
        public BlockColor color;
        public string displayName;

        [Header("Effects")]
        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int goldValue;

        [Header("Visual")]
        public GameObject prefab;
        public Sprite icon;

        /// <summary>
        /// Block 인스턴스에 이 데이터를 적용합니다.
        /// </summary>
        public void ApplyToBlock(Block block)
        {
            block.color = color;
            block.attackValue = attackValue;
            block.defenseValue = defenseValue;
            block.healValue = healValue;
            block.goldValue = goldValue;
        }
    }
}
