namespace MatchBattle
{
    /// <summary>
    /// 유물 등급
    /// </summary>
    public enum RelicRarity
    {
        Common,     // 일반 - 일반/엘리트 전투, 상점
        Boss        // 보스 - 보스 처치 시 확정
    }

    /// <summary>
    /// 유물 효과 발동 시점 (전투 중 상태 효과 전용)
    /// </summary>
    public enum RelicTriggerType
    {
        OnBattleStart,  // 전투 시작 시
        OnTurnStart     // 매 턴 시작 시
    }
}
