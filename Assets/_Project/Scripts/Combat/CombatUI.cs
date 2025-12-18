using System.Collections;
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
        [SerializeField] private CharacterUI enemyUI;

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
        private Enemy currentEnemy;

        // ===========================================
        // 초기화
        // ===========================================

        /// <summary>
        /// 전투 시작 시 UI 초기화
        /// </summary>
        public void SetupBattle(Player player, Enemy enemy)
        {
            currentPlayer = player;
            currentEnemy = enemy;

            // Character UI 설정
            if (playerUI != null)
            {
                playerUI.Setup(player);
            }

            if (enemyUI != null)
            {
                enemyUI.Setup(enemy);
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

            Debug.Log("[CombatUI] Battle UI setup complete");
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

            if (enemyUI != null)
            {
                enemyUI.Cleanup();
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
        /// 적 행동 예고 표시
        /// </summary>
        public void ShowEnemyIntent(EnemyAction action)
        {
            if (enemyUI != null)
            {
                enemyUI.ShowIntent(action);
            }
        }

        // ===========================================
        // 팝업 위임 (CharacterUI로)
        // ===========================================

        /// <summary>
        /// 데미지 팝업 표시 (대상에게)
        /// </summary>
        public void ShowDamage(bool isPlayer, int damage)
        {
            if (isPlayer && playerUI != null)
            {
                playerUI.ShowDamage(damage);
            }
            else if (!isPlayer && enemyUI != null)
            {
                enemyUI.ShowDamage(damage);
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
        /// 방어력 증가 팝업 표시
        /// </summary>
        public void ShowDefenseGain(bool isPlayer, int amount)
        {
            if (isPlayer && playerUI != null)
            {
                playerUI.ShowDefenseGain(amount);
            }
            else if (!isPlayer && enemyUI != null)
            {
                enemyUI.ShowDefenseGain(amount);
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
        /// 특수 효과 표시 (범용)
        /// </summary>
        public void ShowSpecialEffect(string message, Color color)
        {
            if (enemyUI != null)
            {
                enemyUI.ShowCustomPopup(message, color);
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
