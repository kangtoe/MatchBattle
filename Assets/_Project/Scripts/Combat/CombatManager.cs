using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 전투 상태
    /// </summary>
    public enum CombatState
    {
        None,           // 전투 전
        Start,          // 전투 시작
        PlayerTurn,     // 플레이어 턴
        EnemyTurn,      // 적 턴
        Victory,        // 승리
        Defeat          // 패배
    }

    /// <summary>
    /// 전투 시스템 관리 (렌더링 없는 순수 로직)
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        // 전투 참가자
        public Player player;
        public Enemy currentEnemy;

        // 전투 상태
        public CombatState currentState = CombatState.None;
        public int turnCount = 0;

        // 참조
        private BoardManager boardManager;
        private BoardInputHandler boardInputHandler;
        private CombatUI combatUI;

        // 테스트용 적 데이터
        [Header("Test Settings")]
        [SerializeField] private EnemyData testEnemyData;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            boardManager = FindAnyObjectByType<BoardManager>();
            boardInputHandler = FindAnyObjectByType<BoardInputHandler>();
            combatUI = FindAnyObjectByType<CombatUI>();

            if (boardManager == null)
            {
                Debug.LogError("[CombatManager] BoardManager not found!");
            }

            if (boardInputHandler == null)
            {
                Debug.LogError("[CombatManager] BoardInputHandler not found!");
            }
            else
            {
                // 보드 이벤트 구독
                boardInputHandler.OnPathCompleted += HandlePathCompleted;
            }

            if (combatUI == null)
            {
                Debug.LogWarning("[CombatManager] CombatUI not found! UI will not be displayed.");
            }

            // TODO: 나중에 제거 - 테스트용 자동 전투 시작
            StartTestCombat();
        }

        void OnDestroy()
        {
            // 이벤트 구독 해제
            if (boardInputHandler != null)
            {
                boardInputHandler.OnPathCompleted -= HandlePathCompleted;
            }
        }

        // ===========================================
        // 보드 이벤트 핸들러
        // ===========================================

        /// <summary>
        /// 블록 경로 완료 시 호출 (보드 → 전투 연동)
        /// </summary>
        void HandlePathCompleted(object sender, PathCompletedEventArgs args)
        {
            if (currentState != CombatState.PlayerTurn)
            {
                Debug.LogWarning("[Combat] Path completed but it's not player turn!");
                return;
            }

            if (!args.IsValid || args.BlockCount == 0)
            {
                Debug.LogWarning("[Combat] Invalid path, no effect applied");
                return;
            }

            // 블록 개수에 따른 효과 배수 계산
            float effectMultiplier = CalculateEffectMultiplier(args.BlockCount);
            Debug.Log($"[Combat] Path completed: {args.BlockCount} {args.Color} blocks (x{effectMultiplier:F1} multiplier)");

            // 색상별 효과 적용
            ApplyBlockEffect(args.Color, args.BlockCount, effectMultiplier);

            // 플레이어 턴 종료
            EndPlayerTurn();
        }

        /// <summary>
        /// 블록 개수에 따른 효과 배수 계산
        /// </summary>
        float CalculateEffectMultiplier(int blockCount)
        {
            if (blockCount >= 3)
            {
                return 1.0f; // 100% 효과
            }
            else
            {
                return 0.5f; // 50% 효과 (1-2개 블록)
            }
        }

        /// <summary>
        /// 블록 색상별 효과 적용
        /// </summary>
        void ApplyBlockEffect(BlockColor color, int blockCount, float multiplier)
        {
            // 기본 효과값 (블록 1개당)
            int baseValue = 0;

            switch (color)
            {
                case BlockColor.Red:
                    // 공격: 블록 1개당 5 데미지
                    baseValue = 5;
                    int damage = Mathf.RoundToInt(baseValue * blockCount * multiplier);
                    Debug.Log($"[Combat] Red blocks → Attack {damage} damage");
                    DealDamage(damage);
                    break;

                case BlockColor.Blue:
                    // 방어: 블록 1개당 5 방어력
                    baseValue = 5;
                    int defense = Mathf.RoundToInt(baseValue * blockCount * multiplier);
                    Debug.Log($"[Combat] Blue blocks → Defense +{defense}");
                    AddDefense(defense);
                    break;

                case BlockColor.Yellow:
                    // 골드: 블록 1개당 1 골드
                    baseValue = 1;
                    int gold = Mathf.RoundToInt(baseValue * blockCount * multiplier);
                    Debug.Log($"[Combat] Yellow blocks → Gold +{gold}");
                    AddGold(gold);
                    break;

                case BlockColor.Brown:
                    // 회복: 블록 1개당 10 HP (임시, 나중에 BlockType별로 구분)
                    baseValue = 10;
                    int heal = Mathf.RoundToInt(baseValue * blockCount * multiplier);
                    Debug.Log($"[Combat] Brown blocks → Heal +{heal} HP");
                    HealPlayer(heal);
                    break;

                case BlockColor.Purple:
                    // 와일드카드: 임시로 공격으로 처리
                    baseValue = 5;
                    int wildcardDamage = Mathf.RoundToInt(baseValue * blockCount * multiplier);
                    Debug.Log($"[Combat] Purple blocks (wildcard) → Attack {wildcardDamage} damage");
                    DealDamage(wildcardDamage);
                    break;

                default:
                    Debug.LogWarning($"[Combat] Unknown block color: {color}");
                    break;
            }
        }

        // ===========================================
        // 전투 시작/종료
        // ===========================================

        /// <summary>
        /// 전투 시작 (테스트용 - EnemyData 기반)
        /// </summary>
        void StartTestCombat()
        {
            Debug.Log("=== TEST COMBAT START ===");

            // 플레이어 생성
            player = new Player(maxHP: 100, maxDefense: 30, startingGold: 0);

            // EnemyData로부터 적 생성
            if (testEnemyData != null)
            {
                Enemy enemy = testEnemyData.CreateEnemy();
                StartCombat(enemy);
            }
            else
            {
                Debug.LogError("CombatManager: testEnemyData is not assigned! Please assign an EnemyData SO in Inspector.");
            }
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat(Enemy enemy)
        {
            turnCount = 0;
            currentState = CombatState.Start;
            currentEnemy = enemy;

            Debug.Log($"\n========== COMBAT START ==========");
            Debug.Log($"Enemy: {currentEnemy.EnemyName}");
            player.LogStatus();
            currentEnemy.LogStatus();

            // UI 초기화
            if (combatUI != null)
            {
                combatUI.SetupBattle(player, currentEnemy);
            }

            // 적 첫 행동 결정
            currentEnemy.SelectNextAction();
            Debug.Log($"[Enemy] Next action: {currentEnemy.nextAction}");

            // 적 행동 예고 UI 표시
            if (combatUI != null)
            {
                combatUI.ShowEnemyIntent(currentEnemy.nextAction);
            }

            // 플레이어 턴 시작
            StartPlayerTurn();
        }

        /// <summary>
        /// 플레이어 턴 시작
        /// </summary>
        public void StartPlayerTurn()
        {
            currentState = CombatState.PlayerTurn;
            turnCount++;

            Debug.Log($"\n========== TURN {turnCount} - PLAYER TURN ==========");
            Debug.Log($"[Enemy] Next action: {currentEnemy.nextAction}");

            // TODO: 보드 입력 활성화 (나중에 구현)
            // boardManager.EnablePlayerInput();
        }

        /// <summary>
        /// 플레이어 턴 종료
        /// </summary>
        public void EndPlayerTurn()
        {
            Debug.Log("[Player] Turn ended");

            // TODO: 보드 입력 비활성화
            // boardManager.DisablePlayerInput();

            // 적 턴 시작 (딜레이 없이 바로 실행, UI 없으므로)
            StartEnemyTurn();
        }

        /// <summary>
        /// 적 턴 시작
        /// </summary>
        void StartEnemyTurn()
        {
            currentState = CombatState.EnemyTurn;

            Debug.Log($"\n========== TURN {turnCount} - ENEMY TURN ==========");

            // 적 행동 실행
            ExecuteEnemyAction(currentEnemy.nextAction);

            // 다음 행동 선택
            currentEnemy.SelectNextAction();

            // 적 행동 예고 UI 업데이트
            if (combatUI != null)
            {
                combatUI.ShowEnemyIntent(currentEnemy.nextAction);
            }

            // 승패 판정
            if (!player.IsAlive())
            {
                HandleDefeat();
                return;
            }

            if (!currentEnemy.IsAlive())
            {
                HandleVictory();
                return;
            }

            // 플레이어 턴으로 복귀 (UI 없으므로 바로)
            StartPlayerTurn();
        }

        // ===========================================
        // 적 AI
        // ===========================================

        /// <summary>
        /// 적 행동 실행
        /// </summary>
        void ExecuteEnemyAction(EnemyAction action)
        {
            Debug.Log($"[Enemy] Executing action: {action}");

            switch (action.type)
            {
                case EnemyActionType.Attack:
                    DealDamageToPlayer(action.value);
                    break;

                case EnemyActionType.Defend:
                    // 임시 방어력 획득 (소모됨)
                    currentEnemy.AddDefense(action.value);
                    break;

                case EnemyActionType.Buff:
                    // 버프 효과 (예: 금속화, 힘 등)
                    Debug.Log("[Enemy] Buff applied (not implemented yet)");
                    break;

                case EnemyActionType.Debuff:
                    // 디버프 효과 (플레이어에게 적용)
                    Debug.Log("[Enemy] Debuff applied (not implemented yet)");
                    break;
            }
        }

        // ===========================================
        // 데미지/방어/회복 시스템
        // ===========================================

        /// <summary>
        /// 플레이어 → 적 공격
        /// </summary>
        public void DealDamage(int damage)
        {
            if (currentEnemy == null)
            {
                Debug.LogWarning("[Combat] No enemy to attack!");
                return;
            }

            Debug.Log($"[Player] Attacking enemy for {damage} damage");
            currentEnemy.TakeDamage(damage);

            // 데미지 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowDamage(false, damage); // false = 적
            }
        }

        /// <summary>
        /// 적 → 플레이어 공격
        /// </summary>
        void DealDamageToPlayer(int damage)
        {
            Debug.Log($"[Enemy] Attacking player for {damage} damage");
            player.TakeDamage(damage);

            // 데미지 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowDamage(true, damage); // true = 플레이어
            }
        }

        /// <summary>
        /// 플레이어 방어력 추가
        /// </summary>
        public void AddDefense(int amount)
        {
            player.AddDefense(amount);

            // 방어력 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowDefenseGain(true, amount);
            }
        }

        /// <summary>
        /// 플레이어 회복
        /// </summary>
        public void HealPlayer(int amount)
        {
            player.Heal(amount);

            // 회복 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowHeal(amount);
            }
        }

        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(int amount)
        {
            player.AddGold(amount);

            // 골드 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowGoldGain(amount);
            }
        }

        // ===========================================
        // 승패 처리
        // ===========================================

        void HandleVictory()
        {
            currentState = CombatState.Victory;

            Debug.Log("\n========== VICTORY! ==========");
            Debug.Log($"[Player] Defeated {currentEnemy.EnemyName}!");

            // 골드 보상 (임시: 적 최대 HP의 10%)
            int goldReward = Mathf.Max(1, currentEnemy.MaxHP / 10);
            player.AddGold(goldReward);

            player.LogStatus();
            Debug.Log("==============================\n");

            // 승리 화면 표시
            if (combatUI != null)
            {
                combatUI.ShowVictoryScreen(goldReward);
            }

            // TODO: 보상 선택 화면으로 이동
        }

        void HandleDefeat()
        {
            currentState = CombatState.Defeat;

            Debug.Log("\n========== DEFEAT... ==========");
            Debug.Log($"[Player] You were defeated by {currentEnemy.EnemyName}");
            Debug.Log("==============================\n");

            // 패배 화면 표시
            if (combatUI != null)
            {
                combatUI.ShowDefeatScreen();
            }

            // TODO: 게임 오버 화면으로 이동
        }

        // ===========================================
        // 디버그/테스트
        // ===========================================

        void Update()
        {
            // 테스트용 키보드 입력 (블록 드래그가 없을 때만 사용)
            // 이제 보드와 연동되었으므로 주석 처리
            /*
            if (currentState == CombatState.PlayerTurn)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Debug.Log("\n[TEST] Player attacks with 10 damage");
                    DealDamage(10);
                    EndPlayerTurn();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    Debug.Log("\n[TEST] Player gains 5 defense");
                    AddDefense(5);
                    EndPlayerTurn();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    Debug.Log("\n[TEST] Player heals 10 HP");
                    HealPlayer(10);
                    EndPlayerTurn();
                }
            }
            */
        }
    }
}
