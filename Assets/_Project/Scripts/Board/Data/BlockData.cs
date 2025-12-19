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
        public Sprite icon;

        /// <summary>
        /// Block 인스턴스에 이 데이터를 적용합니다.
        /// </summary>
        public void ApplyToBlock(Block block, BlockPool blockPool)
        {
            block.color = color;
            block.attackValue = attackValue;
            block.defenseValue = defenseValue;
            block.healValue = healValue;
            block.goldValue = goldValue;

            // 비주얼 적용
            if (block.visual != null)
            {
                // 배경 색상
                if (block.visual.BackgroundSprite != null)
                {
                    Color displayColor = blockPool.GetColorForBlockColor(color);
                    block.visual.BackgroundSprite.color = displayColor;
                }

                // 아이콘
                if (block.visual.IconSprite != null && icon != null)
                {
                    block.visual.IconSprite.sprite = icon;
                }

                // 이름 표시
                if (block.visual.NameText != null)
                {
                    block.visual.NameText.text = displayName;
                }
            }
        }
    }
}
