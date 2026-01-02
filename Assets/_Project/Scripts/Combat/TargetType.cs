namespace MatchBattle
{
    /// <summary>
    /// 효과/공격 대상 타입 (통합)
    /// 플레이어와 적 모두 사용 가능
    /// Self는 발동자 자신을 의미 (플레이어든 적이든)
    /// </summary>
    public enum TargetType
    {
        Self,           // 발동자 자신 (플레이어 or 적)
        EnemyFront,     // 전방 적 우선 (단일)
        EnemyBack,      // 후방 적 우선 (단일, Post-MVP)
        EnemyRandom,    // 무작위 적 (단일)
        EnemyAll,       // 모든 적
        Player          // 플레이어 (적이 사용 - 디버프/공격용)
    }
}
