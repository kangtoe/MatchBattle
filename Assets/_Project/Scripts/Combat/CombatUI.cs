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
        [Header("Player UI")]
        [SerializeField] private TextMeshProUGUI playerHPText;
        [SerializeField] private TextMeshProUGUI playerDefenseText;
        [SerializeField] private TextMeshProUGUI playerGoldText;

        [Header("Enemy UI")]
        [SerializeField] private TextMeshProUGUI enemyNameText;
        [SerializeField] private TextMeshProUGUI enemyHPText;
        [SerializeField] private TextMeshProUGUI enemyDefenseText;
        [SerializeField] private TextMeshProUGUI enemyIntentText;

        [Header("Damage Popup")]
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private Transform playerPopupSpawn;
        [SerializeField] private Transform enemyPopupSpawn;

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

            // 이벤트 구독
            SubscribeToEvents();

            // 초기 UI 업데이트
            UpdatePlayerHP(player.CurrentHP);
            UpdatePlayerDefense(player.Defense);
            UpdatePlayerGold(player.Gold);

            UpdateEnemyName(enemy.EnemyName);
            UpdateEnemyHP(enemy.CurrentHP);
            UpdateEnemyDefense(enemy.Defense);

            // 결과 패널 숨김
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            Debug.Log("[CombatUI] Battle UI setup complete");
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        void SubscribeToEvents()
        {
            if (currentPlayer != null)
            {
                currentPlayer.OnHPChanged.AddListener(UpdatePlayerHP);
                currentPlayer.OnDefenseChanged.AddListener(UpdatePlayerDefense);
                currentPlayer.OnGoldChanged.AddListener(UpdatePlayerGold);
            }

            if (currentEnemy != null)
            {
                currentEnemy.OnHPChanged.AddListener(UpdateEnemyHP);
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        void UnsubscribeFromEvents()
        {
            if (currentPlayer != null)
            {
                currentPlayer.OnHPChanged.RemoveListener(UpdatePlayerHP);
                currentPlayer.OnDefenseChanged.RemoveListener(UpdatePlayerDefense);
                currentPlayer.OnGoldChanged.RemoveListener(UpdatePlayerGold);
            }

            if (currentEnemy != null)
            {
                currentEnemy.OnHPChanged.RemoveListener(UpdateEnemyHP);
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ===========================================
        // 플레이어 UI 업데이트
        // ===========================================

        void UpdatePlayerHP(int currentHP)
        {
            if (playerHPText == null) return;

            if (currentPlayer != null)
            {
                playerHPText.text = $"HP: {currentHP}/{currentPlayer.MaxHP}";

                // HP가 낮을 때 빨간색 강조 (30% 이하)
                if (currentHP <= currentPlayer.MaxHP * 0.3f)
                {
                    playerHPText.color = new Color(1f, 0.27f, 0.27f); // #FF4444
                }
                else
                {
                    playerHPText.color = Color.white;
                }
            }
        }

        void UpdatePlayerDefense(int defense)
        {
            if (playerDefenseText == null) return;

            if (currentPlayer != null)
            {
                playerDefenseText.text = $"방어력: {defense}/{currentPlayer.MaxDefense}";
            }
        }

        void UpdatePlayerGold(int gold)
        {
            if (playerGoldText == null) return;

            playerGoldText.text = $"골드: {gold}";
        }

        // ===========================================
        // 적 UI 업데이트
        // ===========================================

        void UpdateEnemyName(string enemyName)
        {
            if (enemyNameText == null) return;

            enemyNameText.text = enemyName;

            // 분노 상태면 🔥 추가
            if (currentEnemy != null && currentEnemy.isEnraged)
            {
                enemyNameText.text = $"{enemyName} 🔥";
                enemyNameText.color = new Color(1f, 0.84f, 0f); // #FFD700 (노란색)
            }
            else
            {
                enemyNameText.color = Color.white;
            }
        }

        void UpdateEnemyHP(int currentHP)
        {
            if (enemyHPText == null) return;

            if (currentEnemy != null)
            {
                enemyHPText.text = $"HP: {currentHP}/{currentEnemy.MaxHP}";
            }
        }

        void UpdateEnemyDefense(int defense)
        {
            if (enemyDefenseText == null) return;

            // 방어력이 0보다 클 때만 표시
            if (defense > 0)
            {
                enemyDefenseText.gameObject.SetActive(true);
                enemyDefenseText.text = $"방어력: {defense}";
            }
            else
            {
                enemyDefenseText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 적 행동 예고 표시
        /// </summary>
        public void ShowEnemyIntent(EnemyAction action)
        {
            if (enemyIntentText == null) return;

            if (action == null)
            {
                enemyIntentText.text = "다음 행동: ???";
                return;
            }

            // 행동 예고 텍스트 생성
            string intentText = $"다음 행동: {action.description}";

            // 강공격이면 빨간색 강조
            if (action.type == EnemyActionType.HeavyAttack)
            {
                enemyIntentText.text = intentText;
                enemyIntentText.color = new Color(1f, 0.27f, 0.27f); // #FF4444 (빨간색)
            }
            else
            {
                enemyIntentText.text = intentText;
                enemyIntentText.color = Color.white;
            }

            Debug.Log($"[CombatUI] Enemy intent updated: {intentText}");
        }

        // ===========================================
        // 데미지 팝업
        // ===========================================

        /// <summary>
        /// 데미지 팝업 표시 (대상에게)
        /// </summary>
        public void ShowDamage(bool isPlayer, int damage)
        {
            Transform spawnPoint = isPlayer ? playerPopupSpawn : enemyPopupSpawn;
            ShowPopup(spawnPoint, $"-{damage}", new Color(1f, 0.27f, 0.27f)); // 빨간색
        }

        /// <summary>
        /// 회복 팝업 표시
        /// </summary>
        public void ShowHeal(int amount)
        {
            ShowPopup(playerPopupSpawn, $"+{amount}", new Color(0.27f, 1f, 0.27f)); // 초록색
        }

        /// <summary>
        /// 방어력 증가 팝업 표시
        /// </summary>
        public void ShowDefenseGain(bool isPlayer, int amount)
        {
            Transform spawnPoint = isPlayer ? playerPopupSpawn : enemyPopupSpawn;
            ShowPopup(spawnPoint, $"+{amount} 방어력", new Color(0.27f, 0.27f, 1f)); // 파란색
        }

        /// <summary>
        /// 골드 획득 팝업 표시
        /// </summary>
        public void ShowGoldGain(int amount)
        {
            ShowPopup(playerPopupSpawn, $"+{amount} 골드", new Color(1f, 0.84f, 0f)); // 노란색
        }

        /// <summary>
        /// 회피 팝업 표시
        /// </summary>
        public void ShowEvasion()
        {
            ShowPopup(playerPopupSpawn, "회피!", Color.cyan);
        }

        /// <summary>
        /// 범용 팝업 표시
        /// </summary>
        void ShowPopup(Transform spawnPoint, string text, Color color)
        {
            if (damagePopupPrefab == null || spawnPoint == null)
            {
                Debug.LogWarning("[CombatUI] Damage popup prefab or spawn point not set");
                return;
            }

            GameObject popup = Instantiate(damagePopupPrefab, spawnPoint.position, Quaternion.identity, transform);
            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();

            if (popupText != null)
            {
                popupText.text = text;
                popupText.color = color;
            }

            // 1초 후 파괴
            Destroy(popup, 1.0f);
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

        /// <summary>
        /// 적 분노 효과 표시
        /// </summary>
        public void ShowEnrageEffect()
        {
            if (currentEnemy != null)
            {
                UpdateEnemyName(currentEnemy.EnemyName);
                ShowPopup(enemyPopupSpawn, "분노!", new Color(1f, 0.27f, 0.27f));
            }
        }
    }
}
