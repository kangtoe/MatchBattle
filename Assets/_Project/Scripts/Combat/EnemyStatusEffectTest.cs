using System.Collections;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// Enemy 상태 효과 시스템 테스트
    /// 빈 GameObject에 추가하고 Play 모드로 실행하세요.
    /// </summary>
    public class EnemyStatusEffectTest : MonoBehaviour
    {
        private Enemy testEnemy;

        void Start()
        {
            Debug.Log("=== Enemy Status Effect System Test Start ===");

            // 테스트용 Enemy 생성
            testEnemy = new Enemy("Test Slime", 100, 10);

            // 테스트 시작
            StartCoroutine(RunTests());
        }

        IEnumerator RunTests()
        {
            yield return new WaitForSeconds(0.5f);
            Test1_BasicStatusEffects();

            yield return new WaitForSeconds(1f);
            Test2_PlatedSystem();

            yield return new WaitForSeconds(1f);
            Test3_TurnProcessing();

            yield return new WaitForSeconds(1f);
            Test4_StatusEffectStacking();

            yield return new WaitForSeconds(1f);
            Test5_ExhaustedMechanic();

            yield return new WaitForSeconds(1f);
            Debug.Log("=== All Tests Complete ===");
        }

        /// <summary>
        /// 테스트 1: 기본 상태 효과 추가
        /// </summary>
        void Test1_BasicStatusEffects()
        {
            Debug.Log("\n[Test 1] Basic Status Effects");
            Debug.Log("----------------------------------------");

            // 힘 추가
            testEnemy.AddSTR(5);
            Debug.Log($"Expected Attack Power: 15 (10 base + 5 STR)");
            Debug.Log($"Actual Attack Power: {testEnemy.CurrentAttackPower}");

            // 금속화 추가
            testEnemy.AddPLATED(10);
            Debug.Log($"Expected PLATED: 10");
            Debug.Log($"Actual PLATED: {testEnemy.GetPLATED()}");

            // 상태 로그
            testEnemy.LogStatus();
        }

        /// <summary>
        /// 테스트 2: PLATED 시스템 (영구 데미지 감소)
        /// </summary>
        void Test2_PlatedSystem()
        {
            Debug.Log("\n[Test 2] PLATED System (Permanent Damage Reduction)");
            Debug.Log("----------------------------------------");

            // 새 Enemy 생성
            Enemy platedEnemy = new Enemy("Armored Goblin", 50, 5);
            platedEnemy.AddPLATED(10);

            Debug.Log("Initial State:");
            platedEnemy.LogStatus();

            // 공격 1: 25 데미지
            Debug.Log("\nAttack 1: 25 damage");
            platedEnemy.TakeDamage(25);
            Debug.Log($"Expected HP: 35 (50 - (25-10))");
            Debug.Log($"Actual HP: {platedEnemy.CurrentHP}/{platedEnemy.MaxHP}");
            Debug.Log($"PLATED still: {platedEnemy.GetPLATED()} (should be 10)");

            // 공격 2: 8 데미지 (PLATED보다 작음)
            Debug.Log("\nAttack 2: 8 damage");
            platedEnemy.TakeDamage(8);
            Debug.Log($"Expected HP: 35 (no damage, blocked by PLATED)");
            Debug.Log($"Actual HP: {platedEnemy.CurrentHP}/{platedEnemy.MaxHP}");
            Debug.Log($"PLATED still: {platedEnemy.GetPLATED()} (should be 10)");
        }

        /// <summary>
        /// 테스트 3: 턴 처리 (REGEN, POISON, Duration)
        /// </summary>
        void Test3_TurnProcessing()
        {
            Debug.Log("\n[Test 3] Turn Processing");
            Debug.Log("----------------------------------------");

            // 새 Enemy 생성
            Enemy turnEnemy = new Enemy("Turn Test Enemy", 50, 5);

            // REGEN 추가
            turnEnemy.AddStatusEffect(new StatusEffect(StatusEffectType.REGEN, 5));

            // POISON 추가
            turnEnemy.AddStatusEffect(new StatusEffect(StatusEffectType.POISON, 3));

            // WEAK 추가 (3턴)
            turnEnemy.AddStatusEffect(new StatusEffect(StatusEffectType.WEAK, 0, 3));

            Debug.Log("Initial State:");
            turnEnemy.LogStatus();

            // 턴 1
            Debug.Log("\n--- Turn 1 Start ---");
            turnEnemy.ProcessTurnStart();
            Debug.Log($"Expected HP: 52 (50 +5 REGEN -3 POISON)");
            Debug.Log($"Actual HP: {turnEnemy.CurrentHP}/{turnEnemy.MaxHP}");
            turnEnemy.LogStatus();

            // 턴 2
            Debug.Log("\n--- Turn 2 Start ---");
            turnEnemy.ProcessTurnStart();
            Debug.Log($"Expected HP: 54 (52 +4 REGEN -2 POISON)");
            Debug.Log($"Actual HP: {turnEnemy.CurrentHP}/{turnEnemy.MaxHP}");
            turnEnemy.LogStatus();

            // 턴 3
            Debug.Log("\n--- Turn 3 Start ---");
            turnEnemy.ProcessTurnStart();
            Debug.Log($"Expected HP: 55 (54 +3 REGEN -1 POISON -1 자연 회복 상한)");
            Debug.Log($"Actual HP: {turnEnemy.CurrentHP}/{turnEnemy.MaxHP}");
            turnEnemy.LogStatus();
        }

        /// <summary>
        /// 테스트 4: 상태 효과 스택
        /// </summary>
        void Test4_StatusEffectStacking()
        {
            Debug.Log("\n[Test 4] Status Effect Stacking");
            Debug.Log("----------------------------------------");

            Enemy stackEnemy = new Enemy("Stack Test", 100, 10);

            // 힘 스택
            Debug.Log("Adding STR +3");
            stackEnemy.AddSTR(3);
            Debug.Log($"Attack Power: {stackEnemy.CurrentAttackPower}");

            Debug.Log("\nAdding STR +5");
            stackEnemy.AddSTR(5);
            Debug.Log($"Expected Attack Power: 18 (10 base + 3 + 5)");
            Debug.Log($"Actual Attack Power: {stackEnemy.CurrentAttackPower}");

            // 금속화 스택
            Debug.Log("\nAdding PLATED +10");
            stackEnemy.AddPLATED(10);
            Debug.Log($"PLATED: {stackEnemy.GetPLATED()}");

            Debug.Log("\nAdding PLATED +15");

            stackEnemy.AddPLATED(15);
            Debug.Log($"Expected PLATED: 25 (10 + 15)");
            Debug.Log($"Actual PLATED: {stackEnemy.GetPLATED()}");

            stackEnemy.LogStatus();
        }

        /// <summary>
        /// 테스트 5: EXHAUSTED 메커니즘 (일시적 강화)
        /// </summary>
        void Test5_ExhaustedMechanic()
        {
            Debug.Log("\n[Test 5] EXHAUSTED Mechanic (Temporary Buff)");
            Debug.Log("----------------------------------------");

            Enemy exhaustedEnemy = new Enemy("Berserker", 100, 10);

            Debug.Log("Initial State:");
            exhaustedEnemy.LogStatus();

            // 일시적 강화: 힘 +5, 턴 종료 시 -5
            Debug.Log("\nApplying temporary buff: STR +5 + EXHAUSTED 5");
            exhaustedEnemy.AddSTR(5);
            exhaustedEnemy.AddStatusEffect(new StatusEffect(StatusEffectType.EXHAUSTED, 5));

            Debug.Log($"Attack Power during turn: {exhaustedEnemy.CurrentAttackPower}");
            exhaustedEnemy.LogStatus();

            // 턴 종료
            Debug.Log("\n--- Turn End ---");
            exhaustedEnemy.ProcessTurnEnd();

            Debug.Log($"Expected Attack Power: 10 (buff removed)");
            Debug.Log($"Actual Attack Power: {exhaustedEnemy.CurrentAttackPower}");
            exhaustedEnemy.LogStatus();
        }
    }
}
