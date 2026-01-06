using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MatchBattle
{
    /// <summary>
    /// 전투 UI 관리 (Phase 1: 텍스트 기반 MVP)
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        [Header("Character UI")]
        [SerializeField] private CharacterUI playerUI;

        [Header("Enemy UI Slots")]
        [SerializeField] private CharacterUI enemyUI1;
        [SerializeField] private CharacterUI enemyUI2;
        [SerializeField] private CharacterUI enemyUI3;
        [SerializeField] private CharacterUI enemyUI4;

        // 내부 배열 (인덱스 접근용)
        private CharacterUI[] enemyUIs;

        void Awake()
        {
            // 필드를 배열로 매핑
            enemyUIs = new CharacterUI[CombatManager.MAX_ENEMY_SLOTS] { enemyUI1, enemyUI2, enemyUI3, enemyUI4 };
        }

        [Header("Player-specific UI")]
        [SerializeField] private TextMeshProUGUI playerGoldText;

        [Header("Combat Info UI")]
        [SerializeField] private TextMeshProUGUI turnCountText;
        [SerializeField] private TextMeshProUGUI combatStateText;
        [SerializeField] private TextMeshProUGUI stageInfoText;

        [Header("Result Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TextMeshProUGUI victoryGoldText;

        [Header("Stage Selection UI")]
        [SerializeField] private GameObject stageSelectionPanel;
        [SerializeField] private Transform stageButtonContainer;
        [SerializeField] private Button stageButtonPrefab;

        [Header("Reward UI")]
        [SerializeField] private GameObject rewardPanel;
        [SerializeField] private Transform rewardButtonContainer;
        [SerializeField] private Button rewardButtonPrefab;
        [SerializeField] private Button skipRewardButton;

        [Header("Rest UI")]
        [SerializeField] private GameObject restPanel;
        [SerializeField] private TextMeshProUGUI restHealAmountText;
        [SerializeField] private TextMeshProUGUI restCurrentHPText;
        [SerializeField] private Button restButton;
        [SerializeField] private Button skipRestButton;

        private Player currentPlayer;
        private Enemy[] currentEnemies = new Enemy[CombatManager.MAX_ENEMY_SLOTS]; // 고정 슬롯

        // ===========================================
        // 초기화
        // ===========================================

        /// <summary>
        /// 전투 시작 시 UI 초기화 (고정 슬롯)
        /// </summary>
        public void SetupBattle(Player player, Enemy[] enemies)
        {
            currentPlayer = player;

            // 배열 복사
            for (int i = 0; i < enemies.Length; i++)
            {
                currentEnemies[i] = enemies[i];
            }

            // 플레이어 UI 설정
            if (playerUI != null)
            {
                playerUI.Setup(player);
            }

            // 적 UI 설정 (고정 슬롯)
            int enemyCount = 0;
            int slotCount = Mathf.Min(enemyUIs.Length, enemies.Length);
            for (int i = 0; i < slotCount; i++)
            {
                if (enemyUIs[i] != null)
                {
                    if (enemies[i] != null)
                    {
                        // 슬롯에 적 할당
                        enemyUIs[i].Setup(enemies[i]);
                        enemyUIs[i].gameObject.SetActive(true);
                        enemyCount++;
                        Debug.Log($"[Slot {i}] Enemy UI setup: {enemies[i].EnemyName}");
                    }
                    else
                    {
                        // 사용하지 않는 슬롯 비활성화
                        enemyUIs[i].gameObject.SetActive(false);
                    }
                }
            }

            // 나머지 슬롯 비활성화
            for (int i = slotCount; i < enemyUIs.Length; i++)
            {
                if (enemyUIs[i] != null)
                {
                    enemyUIs[i].gameObject.SetActive(false);
                }
            }

            // 플레이어 전용 이벤트 구독
            if (currentPlayer != null)
            {
                currentPlayer.OnGoldChanged.AddListener(UpdatePlayerGold);
            }

            // 초기 골드 표시
            UpdatePlayerGold(player.Gold);

            // 현재 스테이지 정보 표시
            UpdateStageInfo();

            // 결과 패널 숨김
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            if (stageSelectionPanel != null) stageSelectionPanel.SetActive(false);

            Debug.Log($"[CombatUI] Battle UI setup complete: {enemyCount} enemies in {enemyUIs.Length} slots");
        }

        void OnDestroy()
        {
            // 이벤트 구독 해제
            if (currentPlayer != null)
            {
                currentPlayer.OnGoldChanged.RemoveListener(UpdatePlayerGold);
            }

            // CharacterUI 정리
            if (playerUI != null)
            {
                playerUI.Cleanup();
            }

            // 모든 적 UI 정리
            foreach (var enemyUI in enemyUIs)
            {
                if (enemyUI != null)
                {
                    enemyUI.Cleanup();
                }
            }
        }

        // ===========================================
        // 플레이어 전용 UI
        // ===========================================

        void UpdatePlayerGold(int gold)
        {
            if (playerGoldText == null) return;

            playerGoldText.text = $"골드: {gold}";
        }

        // ===========================================
        // 전투 정보 UI
        // ===========================================

        /// <summary>
        /// 턴수 표시 업데이트
        /// </summary>
        public void UpdateTurnCount(int turnCount)
        {
            if (turnCountText == null) return;

            turnCountText.text = $"턴: {turnCount}";
        }

        /// <summary>
        /// 현재 스테이지 정보 표시 업데이트
        /// </summary>
        public void UpdateStageInfo(StageNode node)
        {
            if (stageInfoText == null) return;

            if (node == null)
            {
                stageInfoText.text = "";
                return;
            }

            // "Level 1 - Stage 2" 형식
            stageInfoText.text = $"Level {node.levelIndex} - Stage {node.stageIndex}";
        }

        /// <summary>
        /// 현재 스테이지 정보 표시 (MapManager에서 자동 조회)
        /// </summary>
        public void UpdateStageInfo()
        {
            if (MapManager.Instance != null)
            {
                UpdateStageInfo(MapManager.Instance.GetCurrentNode());
            }
        }

        /// <summary>
        /// 전투 상태 표시 업데이트 (디버그용)
        /// </summary>
        public void UpdateCombatState(CombatState state)
        {
            if (combatStateText == null) return;

            // 상태별 색상 구분
            switch (state)
            {
                case CombatState.None:
                    combatStateText.text = "상태: 대기";
                    combatStateText.color = Color.gray;
                    break;
                case CombatState.Start:
                    combatStateText.text = "상태: 전투 시작";
                    combatStateText.color = Color.white;
                    break;
                case CombatState.PlayerTurn:
                    combatStateText.text = "상태: 플레이어 턴";
                    combatStateText.color = Color.cyan;
                    break;
                case CombatState.EnemyTurn:
                    combatStateText.text = "상태: 적 턴";
                    combatStateText.color = new Color(1f, 0.5f, 0.5f); // 연한 빨강
                    break;
                case CombatState.Victory:
                    combatStateText.text = "상태: 승리!";
                    combatStateText.color = Color.green;
                    break;
                case CombatState.Defeat:
                    combatStateText.text = "상태: 패배...";
                    combatStateText.color = Color.red;
                    break;
            }
        }

        // ===========================================
        // 적 전용 UI
        // ===========================================

        /// <summary>
        /// 적 행동 예고 표시 (특정 적)
        /// </summary>
        public void ShowEnemyIntent(Enemy enemy, EnemyAction action)
        {
            int index = System.Array.IndexOf(currentEnemies, enemy);
            if (index >= 0 && index < enemyUIs.Length && enemyUIs[index] != null)
            {
                enemyUIs[index].ShowIntent(action);
            }
        }

        /// <summary>
        /// 모든 적의 행동 예고 업데이트
        /// </summary>
        public void UpdateAllEnemyIntents()
        {
            for (int i = 0; i < currentEnemies.Length; i++)
            {
                if (currentEnemies[i] != null && enemyUIs[i] != null && currentEnemies[i].IsAlive())
                {
                    enemyUIs[i].ShowIntent(currentEnemies[i].nextAction);
                }
            }
        }

        /// <summary>
        /// 레거시 호환성: 첫 번째 적의 행동 예고 표시
        /// </summary>
        public void ShowEnemyIntent(EnemyAction action)
        {
            if (currentEnemies[0] != null && enemyUIs[0] != null)
            {
                enemyUIs[0].ShowIntent(action);
            }
        }

        // ===========================================
        // 팝업 위임 (CharacterUI로)
        // ===========================================

        /// <summary>
        /// 데미지 팝업 표시 (특정 적에게)
        /// </summary>
        public void ShowDamage(Enemy enemy, int damage)
        {
            int index = System.Array.IndexOf(currentEnemies, enemy);
            if (index >= 0 && index < enemyUIs.Length && enemyUIs[index] != null)
            {
                enemyUIs[index].ShowDamage(damage);
            }
        }

        /// <summary>
        /// 데미지 팝업 표시 (플레이어/적 구분)
        /// </summary>
        public void ShowDamage(bool isPlayer, int damage)
        {
            if (isPlayer && playerUI != null)
            {
                playerUI.ShowDamage(damage);
            }
            else if (!isPlayer && enemyUIs[0] != null)
            {
                // 레거시: 첫 번째 적에게 표시
                enemyUIs[0].ShowDamage(damage);
            }
        }

        /// <summary>
        /// 회복 팝업 표시
        /// </summary>
        public void ShowHeal(int amount)
        {
            if (playerUI != null)
            {
                playerUI.ShowHeal(amount);
            }
        }

        /// <summary>
        /// 방어력 증가 팝업 표시 (특정 적)
        /// </summary>
        public void ShowDefenseGain(Enemy enemy, int amount)
        {
            int index = System.Array.IndexOf(currentEnemies, enemy);
            if (index >= 0 && index < enemyUIs.Length && enemyUIs[index] != null)
            {
                enemyUIs[index].ShowDefenseGain(amount);
            }
        }

        /// <summary>
        /// 방어력 증가 팝업 표시 (플레이어/적 구분)
        /// </summary>
        public void ShowDefenseGain(bool isPlayer, int amount)
        {
            if (isPlayer && playerUI != null)
            {
                playerUI.ShowDefenseGain(amount);
            }
            else if (!isPlayer && enemyUIs[0] != null)
            {
                // 레거시: 첫 번째 적에게 표시
                enemyUIs[0].ShowDefenseGain(amount);
            }
        }

        /// <summary>
        /// 골드 획득 팝업 표시
        /// </summary>
        public void ShowGoldGain(int amount)
        {
            if (playerUI != null)
            {
                playerUI.ShowGoldGain(amount);
            }
        }

        /// <summary>
        /// 회피 팝업 표시
        /// </summary>
        public void ShowEvasion()
        {
            if (playerUI != null)
            {
                playerUI.ShowEvasion();
            }
        }

        /// <summary>
        /// 특수 효과 표시 (특정 적)
        /// </summary>
        public void ShowSpecialEffect(Enemy enemy, string message, Color color)
        {
            int index = System.Array.IndexOf(currentEnemies, enemy);
            if (index >= 0 && index < enemyUIs.Length && enemyUIs[index] != null)
            {
                enemyUIs[index].ShowCustomPopup(message, color);
            }
        }

        /// <summary>
        /// 특수 효과 표시 (첫 번째 적)
        /// </summary>
        public void ShowSpecialEffect(string message, Color color)
        {
            if (enemyUIs[0] != null)
            {
                enemyUIs[0].ShowCustomPopup(message, color);
            }
        }

        // ===========================================
        // 결과 화면
        // ===========================================

        /// <summary>
        /// 승리 화면 표시
        /// </summary>
        public void ShowVictoryScreen(int goldReward)
        {
            if (victoryPanel == null) return;

            victoryPanel.SetActive(true);

            if (victoryGoldText != null)
            {
                victoryGoldText.text = $"획득 골드: +{goldReward}\n현재 골드: {currentPlayer.Gold}";
            }

            Debug.Log("[CombatUI] Victory screen displayed");
        }

        /// <summary>
        /// 패배 화면 표시
        /// </summary>
        public void ShowDefeatScreen()
        {
            if (defeatPanel == null) return;

            defeatPanel.SetActive(true);

            Debug.Log("[CombatUI] Defeat screen displayed");
        }

        // ===========================================
        // 기타 UI
        // ===========================================

        /// <summary>
        /// 플레이어 턴 표시 (선택사항)
        /// </summary>
        public void ShowPlayerTurn()
        {
            // Phase 1에서는 로그만
            Debug.Log("[CombatUI] Player turn started");
        }

        // ===========================================
        // 스테이지 선택 UI
        // ===========================================

        /// <summary>
        /// 다음 스테이지 선택 UI 표시
        /// </summary>
        public void ShowStageSelection(List<StageNode> nextStages)
        {
            if (stageSelectionPanel == null)
            {
                Debug.LogWarning("[CombatUI] Stage selection panel not assigned!");
                return;
            }

            // 승리 패널 숨기고 선택 패널 표시
            if (victoryPanel != null) victoryPanel.SetActive(false);
            stageSelectionPanel.SetActive(true);

            // 기존 버튼들 제거
            if (stageButtonContainer != null)
            {
                foreach (Transform child in stageButtonContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // 새 버튼들 생성
            if (stageButtonPrefab != null && stageButtonContainer != null)
            {
                for (int i = 0; i < nextStages.Count; i++)
                {
                    StageNode stage = nextStages[i];
                    Button buttonInstance = Instantiate(stageButtonPrefab, stageButtonContainer);

                    // 버튼 텍스트 설정 (스테이지 타입만 표시)
                    TextMeshProUGUI buttonText = buttonInstance.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = GetStageTypeDisplayName(stage.stageType);
                    }

                    // 클릭 이벤트 등록 (인덱스 캡처)
                    int capturedIndex = i;
                    buttonInstance.onClick.AddListener(() => OnStageSelected(capturedIndex));
                }
            }

            Debug.Log($"[CombatUI] Stage selection shown with {nextStages.Count} options");
        }

        /// <summary>
        /// 스테이지 타입 표시 이름
        /// </summary>
        private string GetStageTypeDisplayName(StageType type)
        {
            switch (type)
            {
                case StageType.Combat: return "전투";
                case StageType.Elite: return "엘리트";
                case StageType.Shop: return "상점";
                case StageType.Rest: return "휴식";
                case StageType.Event: return "이벤트";
                case StageType.Boss: return "보스";
                default: return "???";
            }
        }

        /// <summary>
        /// 스테이지 선택 시 콜백
        /// </summary>
        private void OnStageSelected(int choiceIndex)
        {
            Debug.Log($"[CombatUI] Stage choice {choiceIndex} selected");

            // 선택 패널 숨김
            if (stageSelectionPanel != null) stageSelectionPanel.SetActive(false);

            // MapManager에 선택 알림
            if (MapManager.Instance != null)
            {
                MapManager.Instance.SelectNextStage(choiceIndex);
            }

            // 다음 전투 시작
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.StartCombatFromCurrentStage();
            }
        }

        /// <summary>
        /// 스테이지 선택 UI 숨김
        /// </summary>
        public void HideStageSelection()
        {
            if (stageSelectionPanel != null)
            {
                stageSelectionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 런 클리어 화면 표시
        /// </summary>
        public void ShowRunClearScreen()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                if (victoryGoldText != null)
                {
                    victoryGoldText.text = $"런 클리어!\n최종 골드: {currentPlayer?.Gold ?? 0}";
                }
            }
            Debug.Log("[CombatUI] Run clear screen displayed");
        }

        // ===========================================
        // 보상 선택 UI
        // ===========================================

        /// <summary>
        /// 보상 선택 UI 표시
        /// </summary>
        public void ShowRewardSelection(List<RewardData> rewards)
        {
            if (rewardPanel == null) return;

            // 승리 패널 숨기고 보상 패널 표시
            if (victoryPanel != null) victoryPanel.SetActive(false);
            rewardPanel.SetActive(true);

            // 기존 버튼들 제거
            if (rewardButtonContainer != null)
            {
                foreach (Transform child in rewardButtonContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // 보상 버튼들 생성
            if (rewardButtonPrefab != null && rewardButtonContainer != null)
            {
                foreach (RewardData reward in rewards)
                {
                    Button buttonInstance = Instantiate(rewardButtonPrefab, rewardButtonContainer);

                    // 버튼 텍스트 설정
                    TextMeshProUGUI buttonText = buttonInstance.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{reward.displayName}\n{reward.description}";
                    }

                    // 클릭 이벤트 등록
                    RewardData capturedReward = reward;
                    buttonInstance.onClick.AddListener(() => OnRewardSelected(capturedReward));
                }
            }

            // 건너뛰기 버튼 이벤트 설정
            if (skipRewardButton != null)
            {
                skipRewardButton.onClick.RemoveAllListeners();
                skipRewardButton.onClick.AddListener(OnRewardSkipped);
            }

            Debug.Log($"[CombatUI] Reward selection shown with {rewards.Count} options");
        }

        /// <summary>
        /// 보상 선택 시 콜백
        /// </summary>
        private void OnRewardSelected(RewardData selectedReward)
        {
            Debug.Log($"[CombatUI] Reward selected: {selectedReward.displayName}");

            // 보상 패널 숨김
            if (rewardPanel != null) rewardPanel.SetActive(false);

            // RewardManager에서 보상 적용
            if (RewardManager.Instance != null && currentPlayer != null)
            {
                RewardManager.Instance.ApplyReward(selectedReward, currentPlayer);
            }
        }

        /// <summary>
        /// 보상 건너뛰기 콜백
        /// </summary>
        private void OnRewardSkipped()
        {
            Debug.Log("[CombatUI] Reward skipped");

            // 보상 패널 숨김
            if (rewardPanel != null) rewardPanel.SetActive(false);

            // RewardManager에서 건너뛰기 처리
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.SkipReward();
            }
        }

        /// <summary>
        /// 보상 선택 UI 숨김
        /// </summary>
        public void HideRewardSelection()
        {
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }
        }

        // ===========================================
        // 휴식 UI
        // ===========================================

        /// <summary>
        /// 휴식 UI 표시
        /// </summary>
        public void ShowRestPanel()
        {
            if (restPanel == null) return;

            restPanel.SetActive(true);

            // 현재 HP 표시
            if (restCurrentHPText != null && currentPlayer != null)
            {
                restCurrentHPText.text = $"현재 HP: {currentPlayer.CurrentHP} / {currentPlayer.MaxHP}";
            }

            // 회복량 표시
            if (restHealAmountText != null && RestManager.Instance != null)
            {
                int healAmount = RestManager.Instance.GetRestHealAmount();
                restHealAmountText.text = $"HP +{healAmount} 회복";
            }

            // 버튼 이벤트 연결
            if (restButton != null)
            {
                restButton.onClick.RemoveAllListeners();
                restButton.onClick.AddListener(OnRestButtonClicked);
            }

            if (skipRestButton != null)
            {
                skipRestButton.onClick.RemoveAllListeners();
                skipRestButton.onClick.AddListener(OnRestSkipped);
            }
        }

        /// <summary>
        /// 휴식 버튼 클릭
        /// </summary>
        private void OnRestButtonClicked()
        {
            Debug.Log("[CombatUI] Rest button clicked");

            // 휴식 패널 숨김
            if (restPanel != null) restPanel.SetActive(false);

            // RestManager에서 휴식 처리
            if (RestManager.Instance != null && currentPlayer != null)
            {
                RestManager.Instance.Rest(currentPlayer);
            }
        }

        /// <summary>
        /// 휴식 건너뛰기
        /// </summary>
        private void OnRestSkipped()
        {
            Debug.Log("[CombatUI] Rest skipped");

            // 휴식 패널 숨김
            if (restPanel != null) restPanel.SetActive(false);

            // RestManager에서 건너뛰기 처리
            if (RestManager.Instance != null)
            {
                RestManager.Instance.Skip();
            }
        }

        /// <summary>
        /// 휴식 UI 숨김
        /// </summary>
        public void HideRestPanel()
        {
            if (restPanel != null)
            {
                restPanel.SetActive(false);
            }
        }
    }
}
