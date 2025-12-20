using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 공격 대상 (타겟팅)
    /// </summary>
    public enum AttackTarget
    {
        Front,      // 전방 적 우선
        Back,       // 후방 적 우선 (Post-MVP)
        Random,     // 무작위 적
        All         // 모든 적 (AOE)
    }

    /// <summary>
    /// 상태 효과 적용 대상
    /// </summary>
    public enum StatusEffectTarget
    {
        Self,   // 플레이어 자신
        Enemy   // 적
    }

    /// <summary>
    /// 블록이 부여하는 상태 효과
    /// </summary>
    [System.Serializable]
    public class BlockStatusEffect
    {
        public StatusEffectType effectType;
        public int stacks;
        public StatusEffectTarget target;

        public BlockStatusEffect(StatusEffectType effectType, int stacks, StatusEffectTarget target)
        {
            this.effectType = effectType;
            this.stacks = stacks;
            this.target = target;
        }
    }

    [CreateAssetMenu(fileName = "BlockData", menuName = "MatchBattle/BlockData")]
    public class BlockData : ScriptableObject
    {
        [Header("Block Info")]
        public BlockColor color;
        public string displayName;
        public Sprite icon;

        [Header("Effects")]
        public int attackValue;
        public int defenseValue;
        public int healValue;
        public int goldValue;

        [Header("Attack Properties")]
        [Tooltip("공격 대상 (다중 적 전투 시 적용)")]
        public AttackTarget attackTarget = AttackTarget.Front;

        [Header("Status Effects")]
        [Tooltip("이 블록을 연결했을 때 부여되는 상태 효과")]
        public List<BlockStatusEffect> statusEffects = new List<BlockStatusEffect>();

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
