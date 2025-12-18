using UnityEngine;
using TMPro;

namespace MatchBattle
{
    /// <summary>
    /// 캐릭터(플레이어/적) UI 관리 클래스
    /// </summary>
    public class CharacterUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI defenseText;

        [Header("Enemy Only")]
        [SerializeField] private TextMeshProUGUI intentText; // 적 전용        

        [Header("Popup")]
        [SerializeField] private Transform popupPoint;
        [SerializeField] private GameObject popupPrefab;

        private Player player;
        private Enemy enemy;
        private bool isPlayer;

        // ===========================================
        // 초기화
        // ===========================================

        /// <summary>
        /// 플레이어 UI 설정
        /// </summary>
        public void Setup(Player player)
        {
            this.player = player;
            this.isPlayer = true;
            this.enemy = null;

            // 이벤트 구독
            player.OnHPChanged.AddListener(UpdateHP);
            player.OnDefenseChanged.AddListener(UpdateDefense);

            // 초기 UI 업데이트
            if (nameText != null)
                nameText.text = "플레이어";

            UpdateHP(player.CurrentHP);
            UpdateDefense(player.Defense);

            Debug.Log("[CharacterUI] Player UI setup complete");
        }

        /// <summary>
        /// 적 UI 설정
        /// </summary>
        public void Setup(Enemy enemy)
        {
            this.enemy = enemy;
            this.isPlayer = false;
            this.player = null;

            // 이벤트 구독
            enemy.OnHPChanged.AddListener(UpdateHP);
            enemy.OnDefenseChanged.AddListener(UpdateDefense);

            // 초기 UI 업데이트
            if (nameText != null)
                nameText.text = enemy.EnemyName;

            UpdateHP(enemy.CurrentHP);
            UpdateDefense(enemy.Defense);

            // 적 의도 텍스트 활성화
            if (intentText != null)
                intentText.gameObject.SetActive(true);

            Debug.Log($"[CharacterUI] Enemy UI setup complete: {enemy.EnemyName}");
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        public void Cleanup()
        {
            if (player != null)
            {
                player.OnHPChanged.RemoveListener(UpdateHP);
                player.OnDefenseChanged.RemoveListener(UpdateDefense);
            }

            if (enemy != null)
            {
                enemy.OnHPChanged.RemoveListener(UpdateHP);
                enemy.OnDefenseChanged.RemoveListener(UpdateDefense);
            }
        }

        void OnDestroy()
        {
            Cleanup();
        }

        // ===========================================
        // UI 업데이트
        // ===========================================

        void UpdateHP(int currentHP)
        {
            if (hpText == null) return;

            if (isPlayer && player != null)
            {
                hpText.text = $"HP: {currentHP}/{player.MaxHP}";

                // HP가 낮을 때 빨간색 강조 (30% 이하)
                if (currentHP <= player.MaxHP * 0.3f)
                {
                    hpText.color = new Color(1f, 0.27f, 0.27f);
                }
                else
                {
                    hpText.color = Color.white;
                }
            }
            else if (!isPlayer && enemy != null)
            {
                hpText.text = $"HP: {currentHP}/{enemy.MaxHP}";
            }
        }

        void UpdateDefense(int defense)
        {
            if (defenseText == null) return;

            if (isPlayer && player != null)
            {
                defenseText.text = $"방어력: {defense}/{player.MaxDefense}";
            }
            else if (!isPlayer && enemy != null)
            {
                // 적은 Defense와 PLATED를 함께 표시
                int plated = enemy.GetPLATED();
                int maxDefense = enemy.MaxDefense;

                if (defense > 0 && plated > 0)
                {
                    defenseText.text = $"방어: {defense}/{maxDefense} | 금속화: {plated}";
                }
                else if (defense > 0)
                {
                    defenseText.text = $"방어: {defense}/{maxDefense}";
                }
                else if (plated > 0)
                {
                    defenseText.text = $"방어: {defense}/{maxDefense} | 금속화: {plated}";
                }
                else
                {
                    defenseText.text = $"방어: {defense}/{maxDefense}";
                }
            }
        }

        // ===========================================
        // 적 의도 표시 (적 전용)
        // ===========================================

        /// <summary>
        /// 적 행동 예고 표시
        /// </summary>
        public void ShowIntent(EnemyAction action)
        {
            if (!isPlayer && intentText != null)
            {
                if (action == null)
                {
                    intentText.text = "다음 행동: ???";
                    return;
                }

                // 행동 예고 텍스트 생성
                string text = $"다음 행동: {action.GetDisplayText()}";
                intentText.text = text;
                intentText.color = Color.white;

                Debug.Log($"[CharacterUI] Enemy intent updated: {text}");
            }
        }

        // ===========================================
        // 팝업 표시
        // ===========================================

        /// <summary>
        /// 데미지 팝업 표시
        /// </summary>
        public void ShowDamage(int damage)
        {
            ShowPopup($"-{damage}", new Color(1f, 0.27f, 0.27f)); // 빨간색
        }

        /// <summary>
        /// 회복 팝업 표시
        /// </summary>
        public void ShowHeal(int amount)
        {
            ShowPopup($"+{amount}", new Color(0.27f, 1f, 0.27f)); // 초록색
        }

        /// <summary>
        /// 방어력 증가 팝업 표시
        /// </summary>
        public void ShowDefenseGain(int amount)
        {
            ShowPopup($"+{amount} 방어력", new Color(0.27f, 0.27f, 1f)); // 파란색
        }

        /// <summary>
        /// 골드 획득 팝업 표시
        /// </summary>
        public void ShowGoldGain(int amount)
        {
            ShowPopup($"+{amount} 골드", new Color(1f, 0.84f, 0f)); // 노란색
        }

        /// <summary>
        /// 회피 팝업 표시
        /// </summary>
        public void ShowEvasion()
        {
            ShowPopup("회피!", Color.cyan);
        }

        /// <summary>
        /// 범용 커스텀 팝업 표시
        /// </summary>
        public void ShowCustomPopup(string text, Color color)
        {
            ShowPopup(text, color);
        }

        /// <summary>
        /// 범용 팝업 표시 (내부용)
        /// </summary>
        void ShowPopup(string text, Color color)
        {
            if (popupPrefab == null || popupPoint == null)
            {
                Debug.LogWarning("[CharacterUI] Popup prefab or spawn point not set");
                return;
            }

            GameObject popup = Instantiate(popupPrefab, popupPoint.position, Quaternion.identity, transform);
            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();

            if (popupText != null)
            {
                popupText.text = text;
                popupText.color = color;
            }

            // 1초 후 파괴
            Destroy(popup, 1.0f);
        }
    }
}
