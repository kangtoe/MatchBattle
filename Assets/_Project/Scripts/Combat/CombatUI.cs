using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        [Header("Result Panels")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TextMeshProUGUI victoryGoldText;

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

            // 결과 패널 숨김
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

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
    }
}
