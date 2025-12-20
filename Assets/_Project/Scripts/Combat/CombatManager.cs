using System;
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
    /// 공격 그룹 (같은 타겟의 블록들을 그룹화)
    /// </summary>
    public class AttackGroup
    {
        public List<Block> blocks;          // 그룹에 속한 블록들
        public AttackTarget target;         // 공격 타겟
        public BlockColor color;            // 블록 색상
        public int firstBlockIndex;         // 원본 경로에서 첫 블록의 인덱스 (정렬용)

        public AttackGroup(AttackTarget target, BlockColor color, int firstBlockIndex)
        {
            this.blocks = new List<Block>();
            this.target = target;
            this.color = color;
            this.firstBlockIndex = firstBlockIndex;
        }

        /// <summary>
        /// 그룹의 총 공격력 계산
        /// </summary>
        public int GetTotalAttackValue()
        {
            int total = 0;
            foreach (Block block in blocks)
            {
                total += block.attackValue;
            }
            return total;
        }

        /// <summary>
        /// 그룹의 총 방어력 계산
        /// </summary>
        public int GetTotalDefenseValue()
        {
            int total = 0;
            foreach (Block block in blocks)
            {
                total += block.defenseValue;
            }
            return total;
        }

        /// <summary>
        /// 그룹의 총 회복량 계산
        /// </summary>
        public int GetTotalHealValue()
        {
            int total = 0;
            foreach (Block block in blocks)
            {
                total += block.healValue;
            }
            return total;
        }

        /// <summary>
        /// 그룹의 총 골드 계산
        /// </summary>
        public int GetTotalGoldValue()
        {
            int total = 0;
            foreach (Block block in blocks)
            {
                total += block.goldValue;
            }
            return total;
        }

        /// <summary>
        /// 디버그용 그룹 정보 출력
        /// </summary>
        public string GetDebugInfo()
        {
            string blockNames = string.Join(", ", blocks.ConvertAll(b => b.data?.displayName ?? "Unknown"));
            return $"[{color} x{blocks.Count}] Target: {target} - Blocks: {blockNames}";
        }
    }

    /// <summary>
    /// 전투 시스템 관리 (렌더링 없는 순수 로직)
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        // 전투 설정
        public const int MAX_ENEMY_SLOTS = 4;  // 최대 적 슬롯 수 (고정)

        // 전투 참가자
        public Player player;
        [NonSerialized] public Enemy[] enemies;  // 전투 중인 적들 (고정 슬롯, 빈 슬롯은 null)

        // 전투 상태
        public CombatState currentState = CombatState.None;
        public int turnCount = 0;

        // 참조
        private BoardManager boardManager;
        private BoardInputHandler boardInputHandler;
        private CombatUI combatUI;

        // 테스트용 적 데이터
        [Header("Test Enemy Data")]
        [Tooltip("슬롯 1→2→3→4 순서로 왼쪽(전방)에서 오른쪽(후방)으로 배치 (MAX_ENEMY_SLOTS = 4)")]
        [SerializeField] private EnemyData testEnemy1;  // 가장 왼쪽 (전방)
        [SerializeField] private EnemyData testEnemy2;
        [SerializeField] private EnemyData testEnemy3;
        [SerializeField] private EnemyData testEnemy4;  // 가장 오른쪽 (후방)

        // 턴 딜레이 설정
        [Header("Turn Timing")]
        [SerializeField] private float playerTurnEndDelay = 0.5f;   // 플레이어 턴 종료 후 대기 시간
        [SerializeField] private float enemyActionDelay = 1.0f;     // 적 행동 실행 후 대기 시간
        [SerializeField] private float intentDisplayDelay = 0.5f;   // 적 의도 표시 후 대기 시간

        // 코루틴 중복 실행 방지
        private bool isEnemyTurnRunning = false;

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

            // 적 배열 초기화 (고정 슬롯)
            enemies = new Enemy[MAX_ENEMY_SLOTS];
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

                // 전투 시작 전에는 입력 비활성화
                boardInputHandler.DisableInput();
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
        // 공격 처리 시스템
        // ===========================================

        /// <summary>
        /// 블록들을 공격 그룹으로 그룹핑 (같은 타겟의 블록들을 합산)
        /// </summary>
        List<AttackGroup> GroupBlocks(List<Block> path)
        {
            List<AttackGroup> groups = new List<AttackGroup>();
            Dictionary<string, AttackGroup> targetGroups = new Dictionary<string, AttackGroup>(); // 타겟별 그룹 (key: target_color)

            for (int i = 0; i < path.Count; i++)
            {
                Block block = path[i];

                if (block.data == null)
                {
                    Debug.LogWarning($"[Combat] Block at index {i} has no data, skipping");
                    continue;
                }

                AttackTarget target = block.data.attackTarget;
                BlockColor color = block.data.color;

                // 같은 타겟의 블록들을 그룹에 합산
                string groupKey = $"{target}_{color}";

                if (!targetGroups.ContainsKey(groupKey))
                {
                    // 새 그룹 생성
                    AttackGroup group = new AttackGroup(target, color, i);
                    group.blocks.Add(block);
                    targetGroups[groupKey] = group;
                    Debug.Log($"[Combat] Group created with key {groupKey} at index {i}");
                }
                else
                {
                    // 기존 그룹에 추가
                    targetGroups[groupKey].blocks.Add(block);
                    Debug.Log($"[Combat] Block added to existing group {groupKey}");
                }
            }

            // 그룹들을 결과에 추가
            foreach (var group in targetGroups.Values)
            {
                groups.Add(group);
                Debug.Log($"[Combat] Final group: {group.GetDebugInfo()}");
            }

            return groups;
        }

        /// <summary>
        /// 블록 처리 순서 결정: 종류별 첫 블록 사용 순서
        /// </summary>
        List<AttackGroup> SortAttackGroups(List<AttackGroup> groups)
        {
            // firstBlockIndex로 정렬 (종류별 첫 블록 등장 순서)
            groups.Sort((a, b) => a.firstBlockIndex.CompareTo(b.firstBlockIndex));

            Debug.Log("[Combat] Attack groups sorted by first block index:");
            for (int i = 0; i < groups.Count; i++)
            {
                Debug.Log($"  {i + 1}. {groups[i].GetDebugInfo()}");
            }

            return groups;
        }

        /// <summary>
        /// 공격 그룹들을 순서대로 실행
        /// </summary>
        void ExecuteAttackGroups(List<AttackGroup> groups)
        {
            Debug.Log($"\n[Combat] Executing {groups.Count} attack groups:");

            for (int i = 0; i < groups.Count; i++)
            {
                AttackGroup group = groups[i];
                Debug.Log($"\n[Combat] --- Group {i + 1}/{groups.Count} ---");
                Debug.Log($"[Combat] {group.GetDebugInfo()}");

                ExecuteSingleGroup(group);
            }

            Debug.Log($"\n[Combat] All {groups.Count} groups executed\n");
        }

        /// <summary>
        /// 단일 그룹 실행
        /// </summary>
        void ExecuteSingleGroup(AttackGroup group)
        {
            // 공격력 처리
            int attackValue = group.GetTotalAttackValue();
            if (attackValue > 0)
            {
                Debug.Log($"[Combat] Attack: {attackValue} damage (from {group.blocks.Count} blocks)");

                // 플레이어 공격력 추가
                int totalDamage = attackValue + player.CurrentAttackPower;
                Debug.Log($"[Combat] Total damage: {attackValue} (blocks) + {player.CurrentAttackPower} (player attack) = {totalDamage}");

                // 타겟 선택하여 공격
                DealDamage(group.target, totalDamage);
            }

            // 방어력 처리
            int defenseValue = group.GetTotalDefenseValue();
            if (defenseValue > 0)
            {
                Debug.Log($"[Combat] Defense: +{defenseValue} (from {group.blocks.Count} blocks)");
                AddDefense(defenseValue);
            }

            // 회복 처리
            int healValue = group.GetTotalHealValue();
            if (healValue > 0)
            {
                Debug.Log($"[Combat] Heal: +{healValue} HP (from {group.blocks.Count} blocks)");
                HealPlayer(healValue);
            }

            // 골드 처리
            int goldValue = group.GetTotalGoldValue();
            if (goldValue > 0)
            {
                Debug.Log($"[Combat] Gold: +{goldValue} (from {group.blocks.Count} blocks)");
                AddGold(goldValue);
            }
        }

        // ===========================================
        // 타겟 선택 시스템
        // ===========================================

        /// <summary>
        /// 생존한 적들만 반환
        /// </summary>
        List<Enemy> GetLivingEnemies()
        {
            List<Enemy> livingEnemies = new List<Enemy>();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && enemies[i].IsAlive())
                {
                    livingEnemies.Add(enemies[i]);
                }
            }
            return livingEnemies;
        }

        /// <summary>
        /// 타겟 선택: Front (전방 = 왼쪽부터 = 배열 앞에서부터)
        /// </summary>
        Enemy SelectFrontTarget()
        {
            // 배열 앞에서부터 첫 번째 살아있는 적
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && enemies[i].IsAlive())
                {
                    return enemies[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 타겟 선택: Back (후방 = 오른쪽부터 = 배열 뒤에서부터)
        /// </summary>
        Enemy SelectBackTarget()
        {
            // 배열 뒤에서부터 첫 번째 살아있는 적
            for (int i = enemies.Length - 1; i >= 0; i--)
            {
                if (enemies[i] != null && enemies[i].IsAlive())
                {
                    return enemies[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 타겟 선택: Random (무작위 적)
        /// </summary>
        Enemy SelectRandomTarget()
        {
            List<Enemy> livingEnemies = GetLivingEnemies();
            if (livingEnemies.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, livingEnemies.Count);
            return livingEnemies[randomIndex];
        }

        /// <summary>
        /// 타겟 선택: AttackTarget enum 기반
        /// </summary>
        Enemy SelectTarget(AttackTarget targetType)
        {
            switch (targetType)
            {
                case AttackTarget.Front:
                    return SelectFrontTarget();
                case AttackTarget.Back:
                    return SelectBackTarget();
                case AttackTarget.Random:
                    return SelectRandomTarget();
                case AttackTarget.All:
                    // All의 경우 단일 타겟 반환 불가, GetLivingEnemies() 사용
                    return null;
                default:
                    Debug.LogWarning($"[Combat] Unknown target type: {targetType}");
                    return SelectFrontTarget();
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

            // 보드 입력 즉시 비활성화 (턴당 1번만 행동)
            if (boardInputHandler != null)
            {
                boardInputHandler.DisableInput();
            }

            Debug.Log($"\n[Combat] ========== PATH COMPLETED ==========");
            Debug.Log($"[Combat] {args.BlockCount} {args.Color} blocks connected");

            // 1. 블록들을 공격 그룹으로 그룹핑
            List<AttackGroup> groups = GroupBlocks(args.Path);

            // 2. 공격 그룹 처리 순서 결정
            groups = SortAttackGroups(groups);

            // 3. 공격 그룹 실행
            ExecuteAttackGroups(groups);

            // 4. 블록 상태 효과 적용
            ApplyBlockStatusEffects(args.Path);

            Debug.Log($"[Combat] ========== PATH PROCESSING DONE ==========\n");

            // 플레이어 턴 종료
            EndPlayerTurn();
        }

        /// <summary>
        /// 블록 색상별 효과 적용
        /// </summary>
        void ApplyBlockEffect(BlockColor color, int blockCount)
        {
            // 기본 효과값 (블록 1개당)
            int baseValue = 0;

            switch (color)
            {
                case BlockColor.Red:
                    // 공격: 블록 1개당 5 데미지 + 플레이어 공격력
                    baseValue = 5;
                    int blockDamage = baseValue * blockCount;
                    int totalDamage = blockDamage + player.CurrentAttackPower;
                    Debug.Log($"[Combat] Red blocks → Attack {blockDamage} (blocks) + {player.CurrentAttackPower} (attack power) = {totalDamage} damage");
                    DealDamage(AttackTarget.Front, totalDamage);
                    break;

                case BlockColor.Blue:
                    // 방어: 블록 1개당 5 방어력
                    baseValue = 5;
                    int defense = baseValue * blockCount;
                    Debug.Log($"[Combat] Blue blocks → Defense +{defense}");
                    AddDefense(defense);
                    break;

                case BlockColor.Yellow:
                    // 골드: 블록 1개당 1 골드
                    baseValue = 1;
                    int gold = baseValue * blockCount;
                    Debug.Log($"[Combat] Yellow blocks → Gold +{gold}");
                    AddGold(gold);
                    break;

                case BlockColor.Brown:
                    // 회복: 블록 1개당 10 HP (임시, 나중에 BlockType별로 구분)
                    baseValue = 10;
                    int heal = baseValue * blockCount;
                    Debug.Log($"[Combat] Brown blocks → Heal +{heal} HP");
                    HealPlayer(heal);
                    break;

                case BlockColor.Purple:
                    // 와일드카드: 아직 효과가 기획되지 않음
                    Debug.LogWarning($"[Combat] Purple blocks (wildcard) have no effect yet - not implemented");
                    break;

                default:
                    Debug.LogWarning($"[Combat] Unknown block color: {color}");
                    break;
            }
        }

        /// <summary>
        /// 블록의 상태 효과 적용
        /// </summary>
        void ApplyBlockStatusEffects(List<Block> path)
        {
            if (path == null || path.Count == 0)
                return;

            foreach (Block block in path)
            {
                if (block.data == null || block.data.statusEffects == null)
                    continue;

                foreach (var statusEffect in block.data.statusEffects)
                {
                    // 상태 효과 생성
                    StatusEffect effect = new StatusEffect(
                        statusEffect.effectType,
                        statusEffect.stacks,
                        -1  // 영구 지속 (duration based는 나중에 추가 가능)
                    );

                    // 대상에 따라 적용
                    if (statusEffect.target == StatusEffectTarget.Self)
                    {
                        player.AddStatusEffect(effect);
                        Debug.Log($"[Combat] Block effect → Player: {effect.GetDisplayText()} {effect.GetDescription()}");
                    }
                    else if (statusEffect.target == StatusEffectTarget.Enemy)
                    {
                        // 모든 생존 적에게 적용 (TODO: 특정 타겟 지정 기능 추가 가능)
                        foreach (Enemy enemy in GetLivingEnemies())
                        {
                            enemy.AddStatusEffect(effect);
                            Debug.Log($"[Combat] Block effect → {enemy.EnemyName}: {effect.GetDisplayText()} {effect.GetDescription()}");
                        }
                    }
                }
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

            // 적 생성 (고정 슬롯, null 허용)
            Enemy[] testEnemies = new Enemy[MAX_ENEMY_SLOTS];

            // 슬롯 0 (가장 왼쪽, 전방)
            if (testEnemy1 != null)
            {
                Debug.Log($"[StartTestCombat] Creating enemy from testEnemy1: {testEnemy1.displayName}");
                testEnemies[0] = testEnemy1.CreateEnemy();
                Debug.Log($"[StartTestCombat] Slot 0 result: {(testEnemies[0] != null ? testEnemies[0].EnemyName : "NULL")}");
            }
            else
            {
                Debug.Log("[StartTestCombat] testEnemy1 is null");
            }

            // 슬롯 1
            if (testEnemy2 != null)
            {
                Debug.Log($"[StartTestCombat] Creating enemy from testEnemy2: {testEnemy2.displayName}");
                testEnemies[1] = testEnemy2.CreateEnemy();
                Debug.Log($"[StartTestCombat] Slot 1 result: {(testEnemies[1] != null ? testEnemies[1].EnemyName : "NULL")}");
            }
            else
            {
                Debug.Log("[StartTestCombat] testEnemy2 is null");
            }

            // 슬롯 2
            if (testEnemy3 != null)
            {
                Debug.Log($"[StartTestCombat] Creating enemy from testEnemy3: {testEnemy3.displayName}");
                testEnemies[2] = testEnemy3.CreateEnemy();
                Debug.Log($"[StartTestCombat] Slot 2 result: {(testEnemies[2] != null ? testEnemies[2].EnemyName : "NULL")}");
            }
            else
            {
                Debug.Log("[StartTestCombat] testEnemy3 is null");
            }

            // 슬롯 3 (가장 오른쪽, 후방)
            if (testEnemy4 != null)
            {
                Debug.Log($"[StartTestCombat] Creating enemy from testEnemy4: {testEnemy4.displayName}");
                testEnemies[3] = testEnemy4.CreateEnemy();
                Debug.Log($"[StartTestCombat] Slot 3 result: {(testEnemies[3] != null ? testEnemies[3].EnemyName : "NULL")}");
            }
            else
            {
                Debug.Log("[StartTestCombat] testEnemy4 is null");
            }

            // 최소 1개 이상의 적이 있는지 확인
            bool hasEnemy = false;
            for (int i = 0; i < testEnemies.Length; i++)
            {
                if (testEnemies[i] != null)
                {
                    hasEnemy = true;
                    break;
                }
            }

            if (hasEnemy)
            {
                StartCombat(testEnemies);
            }
            else
            {
                Debug.LogError("CombatManager: No test enemies assigned! Please assign at least one EnemyData in Inspector.");
            }
        }

        /// <summary>
        /// 전투 시작 (고정 4슬롯, null 허용)
        /// </summary>
        public void StartCombat(Enemy[] enemyArray)
        {
            turnCount = 0;
            currentState = CombatState.Start;

            Debug.Log($"[StartCombat] Input array length: {enemyArray.Length}, enemies field length: {enemies.Length}");

            // 배열 복사 전 상태 확인
            for (int i = 0; i < enemyArray.Length; i++)
            {
                Debug.Log($"[StartCombat] Input enemyArray[{i}]: {(enemyArray[i] != null ? enemyArray[i].EnemyName : "NULL")}");
            }

            // 배열 복사
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = enemyArray[i];
                Debug.Log($"[StartCombat] Copied to enemies[{i}]: {(enemies[i] != null ? enemies[i].EnemyName : "NULL")}");
            }

            Debug.Log($"\n========== COMBAT START ==========");
            int enemyCount = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemyCount++;
                    Debug.Log($"  - Slot {i}: {enemies[i].EnemyName}");
                    enemies[i].LogStatus();
                }
                else
                {
                    Debug.Log($"  - Slot {i}: NULL");
                }
            }
            Debug.Log($"Total enemies: {enemyCount}");
            player.LogStatus();

            // UI 초기화
            if (combatUI != null)
            {
                combatUI.SetupBattle(player, enemies);
                combatUI.UpdateCombatState(currentState);
            }

            // 모든 적의 첫 행동 결정
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].SelectNextAction();
                    Debug.Log($"[{enemies[i].EnemyName}] First action: {enemies[i].nextAction}");
                }
            }

            // 모든 적의 행동 예고 UI 표시
            if (combatUI != null)
            {
                combatUI.UpdateAllEnemyIntents();
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

            // 플레이어 턴 시작 시 상태 효과 처리
            player.ProcessTurnStart();

            // 보드 입력 활성화 (플레이어가 1번 행동할 수 있음)
            if (boardInputHandler != null)
            {
                boardInputHandler.EnableInput();
            }

            // UI 업데이트
            if (combatUI != null)
            {
                combatUI.ShowPlayerTurn();
                combatUI.UpdateTurnCount(turnCount);
                combatUI.UpdateCombatState(currentState);
            }
        }

        /// <summary>
        /// 플레이어 턴 종료
        /// </summary>
        public void EndPlayerTurn()
        {
            Debug.Log("[Player] Turn ended");

            // 플레이어 턴 종료 시 상태 효과 처리
            player.ProcessTurnEnd();

            // 코루틴 중복 실행 방지
            if (isEnemyTurnRunning)
            {
                Debug.LogWarning("[CombatManager] Enemy turn already running! Ignoring duplicate call.");
                return;
            }

            // 적 턴 시작 (코루틴으로 딜레이 포함)
            StartCoroutine(StartEnemyTurnCoroutine());
        }

        /// <summary>
        /// 적 턴 시작 (코루틴 - 다수 적 지원)
        /// </summary>
        IEnumerator StartEnemyTurnCoroutine()
        {
            // 플레이어 턴 종료 후 짧은 딜레이
            yield return new WaitForSeconds(playerTurnEndDelay);

            isEnemyTurnRunning = true;
            currentState = CombatState.EnemyTurn;

            // UI 상태 업데이트
            if (combatUI != null)
            {
                combatUI.UpdateCombatState(currentState);
            }

            Debug.Log($"\n========== TURN {turnCount} - ENEMY TURN ==========");

            // 모든 생존한 적들의 행동 실행
            List<Enemy> livingEnemies = GetLivingEnemies();

            foreach (Enemy enemy in livingEnemies)
            {
                Debug.Log($"\n[{enemy.EnemyName}] Turn start");

                // 적 턴 시작 시 상태 효과 처리
                enemy.ProcessTurnStart();

                // 1. 적 행동 실행
                ExecuteEnemyAction(enemy, enemy.nextAction);

                // 2. 행동 실행 후 딜레이 (플레이어가 결과를 볼 시간)
                yield return new WaitForSeconds(enemyActionDelay);

                // 3. 플레이어 패배 체크 (각 적 행동 후)
                if (!player.IsAlive())
                {
                    isEnemyTurnRunning = false;
                    HandleDefeat();
                    yield break;
                }

                // 적 턴 종료 시 상태 효과 처리
                enemy.ProcessTurnEnd();
            }

            // 4. 모든 적 행동 완료 후 승리 체크
            if (GetLivingEnemies().Count == 0)
            {
                isEnemyTurnRunning = false;
                HandleVictory();
                yield break;
            }

            // 5. 다음 행동 선택 및 예고 (모든 생존 적)
            livingEnemies = GetLivingEnemies();
            foreach (Enemy enemy in livingEnemies)
            {
                enemy.SelectNextAction();
                Debug.Log($"[{enemy.EnemyName}] Next action: {enemy.nextAction}");
            }

            // 모든 적의 다음 행동 예고 UI 표시
            if (combatUI != null)
            {
                combatUI.UpdateAllEnemyIntents();
            }

            // 6. 예고 표시 후 짧은 딜레이
            yield return new WaitForSeconds(intentDisplayDelay);

            // 7. 플레이어 턴으로 복귀
            isEnemyTurnRunning = false;
            StartPlayerTurn();
        }

        // ===========================================
        // 적 AI
        // ===========================================

        /// <summary>
        /// 적 행동 실행
        /// </summary>
        void ExecuteEnemyAction(Enemy enemy, EnemyAction action)
        {
            switch (action.type)
            {
                case EnemyActionType.Attack:
                    // 행동 값 + 적의 공격력
                    int totalDamage = action.value + enemy.CurrentAttackPower;
                    Debug.Log($"[{enemy.EnemyName}] Attack: {action.value} (action) + {enemy.CurrentAttackPower} (attack power) = {totalDamage} damage");
                    DealDamageToPlayer(enemy, totalDamage);
                    break;

                case EnemyActionType.Defend:
                    // 임시 방어력 획득 (소모됨)
                    enemy.AddDefense(action.value);
                    Debug.Log($"[{enemy.EnemyName}] Defend: +{action.value} defense");

                    // 방어력 획득 팝업 표시
                    if (combatUI != null)
                    {
                        combatUI.ShowDefenseGain(enemy, action.value);
                    }
                    break;

                case EnemyActionType.Buff:
                    // 버프 효과 (적 자신에게 적용)
                    ApplyStatusEffectToEnemy(enemy, action);
                    break;

                case EnemyActionType.Debuff:
                    // 디버프 효과 (플레이어에게 적용)
                    ApplyStatusEffectToPlayer(action);
                    break;
            }
        }

        /// <summary>
        /// 적 자신에게 버프 적용
        /// </summary>
        void ApplyStatusEffectToEnemy(Enemy enemy, EnemyAction action)
        {
            if (action.statusEffects == null || action.statusEffects.Length == 0)
            {
                Debug.LogWarning($"[{enemy.EnemyName}] Buff action has no status effects!");
                return;
            }

            foreach (var effectConfig in action.statusEffects)
            {
                var effect = effectConfig.ToStatusEffect();
                enemy.AddStatusEffect(effect);
                Debug.Log($"[{enemy.EnemyName}] Applied buff to self: {effect.GetDisplayText()} - {effect.GetDescription()}");
            }
        }

        /// <summary>
        /// 플레이어에게 디버프 적용
        /// </summary>
        void ApplyStatusEffectToPlayer(EnemyAction action)
        {
            if (action.statusEffects == null || action.statusEffects.Length == 0)
            {
                Debug.LogWarning("[Enemy] Debuff action has no status effects!");
                return;
            }

            foreach (var effectConfig in action.statusEffects)
            {
                var effect = effectConfig.ToStatusEffect();
                player.AddStatusEffect(effect);
                Debug.Log($"[Enemy] Applied debuff to player: {effect.GetDisplayText()} - {effect.GetDescription()}");
            }
        }

        // ===========================================
        // 데미지/방어/회복 시스템
        // ===========================================

        /// <summary>
        /// 플레이어 → 적 공격 (타겟 선택 기반)
        /// </summary>
        public void DealDamage(AttackTarget targetType, int damage)
        {
            if (GetLivingEnemies().Count == 0)
            {
                Debug.LogWarning("[Combat] No living enemies to attack!");
                return;
            }

            // All 타겟: 모든 적에게 데미지
            if (targetType == AttackTarget.All)
            {
                List<Enemy> livingEnemies = GetLivingEnemies();
                Debug.Log($"[Combat] AOE Attack: {damage} damage to {livingEnemies.Count} enemies");

                foreach (Enemy enemy in livingEnemies)
                {
                    DealDamageToEnemy(enemy, damage);
                }
            }
            // 단일 타겟: Front/Back/Random
            else
            {
                Enemy target = SelectTarget(targetType);
                if (target == null)
                {
                    Debug.LogWarning($"[Combat] No valid target for {targetType}");
                    return;
                }

                Debug.Log($"[Combat] Single Attack: {damage} damage to {target.EnemyName} ({targetType})");
                DealDamageToEnemy(target, damage);
            }
        }

        /// <summary>
        /// 단일 적에게 데미지 적용 (상태 효과 포함)
        /// </summary>
        void DealDamageToEnemy(Enemy enemy, int damage)
        {
            int originalDamage = damage;

            // WEAK 적용: 플레이어가 약화 상태면 공격력 -25%
            if (player.HasWEAK())
            {
                damage = Mathf.RoundToInt(damage * 0.75f);
                Debug.Log($"[Player] WEAK applied: {originalDamage} → {damage} damage (-25%)");
            }

            // VULNERABLE 적용: 적이 취약 상태면 받는 데미지 +50%
            if (enemy.HasVULNERABLE())
            {
                int beforeVulnerable = damage;
                damage = Mathf.RoundToInt(damage * 1.5f);
                Debug.Log($"[{enemy.EnemyName}] VULNERABLE applied: {beforeVulnerable} → {damage} damage (+50%)");
            }

            enemy.TakeDamage(damage);

            // 데미지 팝업 표시
            if (combatUI != null)
            {
                combatUI.ShowDamage(enemy, damage);
            }
        }

        /// <summary>
        /// 적 → 플레이어 공격
        /// </summary>
        void DealDamageToPlayer(Enemy enemy, int damage)
        {
            int originalDamage = damage;

            // WEAK 적용: 적이 약화 상태면 공격력 -25%
            if (enemy.HasWEAK())
            {
                damage = Mathf.RoundToInt(damage * 0.75f);
                Debug.Log($"[{enemy.EnemyName}] WEAK applied: {originalDamage} → {damage} damage (-25%)");
            }

            // VULNERABLE 적용: 플레이어가 취약 상태면 받는 데미지 +50%
            if (player.HasVULNERABLE())
            {
                int beforeVulnerable = damage;
                damage = Mathf.RoundToInt(damage * 1.5f);
                Debug.Log($"[Player] VULNERABLE applied: {beforeVulnerable} → {damage} damage (+50%)");
            }

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
            Debug.Log("[Player] Defeated all enemies!");

            // 골드 보상 (임시: 모든 적 최대 HP 합의 10%)
            int totalMaxHP = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    totalMaxHP += enemies[i].MaxHP;
                }
            }
            int goldReward = Mathf.Max(1, totalMaxHP / 10);
            player.AddGold(goldReward);

            player.LogStatus();
            Debug.Log("==============================\n");

            // 승리 화면 표시
            if (combatUI != null)
            {
                combatUI.UpdateCombatState(currentState);
                combatUI.ShowVictoryScreen(goldReward);
            }

            // TODO: 보상 선택 화면으로 이동
        }

        void HandleDefeat()
        {
            currentState = CombatState.Defeat;

            Debug.Log("\n========== DEFEAT... ==========");
            Debug.Log("[Player] You were defeated!");
            Debug.Log("==============================\n");

            // 패배 화면 표시
            if (combatUI != null)
            {
                combatUI.UpdateCombatState(currentState);
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
                    DealDamage(AttackTarget.Front, 10);
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
