namespace MatchBattle
{
    /// <summary>
    /// 보상 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class RewardData
    {
        public RewardType type;
        public int value;           // 골드량, 회복량, 증가량
        public string displayName;  // UI 표시명
        public string description;  // 효과 설명

        public RewardData(RewardType type, int value, string displayName, string description)
        {
            this.type = type;
            this.value = value;
            this.displayName = displayName;
            this.description = description;
        }

        /// <summary>
        /// 보상 타입에 해당하는 아이콘 문자 반환
        /// </summary>
        public string GetIcon()
        {
            switch (type)
            {
                case RewardType.Gold: return "G";
                case RewardType.Heal: return "+";
                case RewardType.MaxHPUp: return "^";
                default: return "?";
            }
        }
    }
}
