using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 런(Run) 관리자 - 플레이어 상태 및 런 진행 관리
    /// </summary>
    public class RunManager : Singleton<RunManager>
    {
        [Header("Player Stats")]
        [SerializeField] private int startingMaxHP = 100;
        [SerializeField] private int startingMaxDefense = 30;
        [SerializeField] private int startingGold = 0;

        // 런 전체에서 유지되는 플레이어 인스턴스
        private Player currentPlayer;

        // 런 상태
        private bool isRunActive = false;

        /// <summary>
        /// 새로운 런 시작
        /// </summary>
        public void StartNewRun()
        {
            Debug.Log("[RunManager] Starting new run");

            // 플레이어 생성
            currentPlayer = new Player(
                maxHP: startingMaxHP,
                maxDefense: startingMaxDefense,
                startingGold: startingGold
            );

            isRunActive = true;

            // 유물 초기화
            if (RelicManager.Instance != null)
            {
                RelicManager.Instance.ResetRelics();
            }

            Debug.Log($"[RunManager] Player created: HP {currentPlayer.CurrentHP}/{currentPlayer.MaxHP}, Gold {currentPlayer.Gold}");
        }

        /// <summary>
        /// 런 종료 (승리)
        /// </summary>
        public void EndRunVictory()
        {
            Debug.Log("[RunManager] Run ended - Victory!");
            isRunActive = false;

            // TODO: 승리 통계 저장, 메인 메뉴로 이동 등
        }

        /// <summary>
        /// 런 종료 (패배)
        /// </summary>
        public void EndRunDefeat()
        {
            Debug.Log("[RunManager] Run ended - Defeat!");
            isRunActive = false;

            // TODO: 패배 통계 저장, 메인 메뉴로 이동 등
        }

        /// <summary>
        /// 현재 플레이어 인스턴스 반환
        /// </summary>
        public Player GetPlayer()
        {
            if (currentPlayer == null)
            {
                Debug.LogWarning("[RunManager] Player is null! Starting new run...");
                StartNewRun();
            }

            return currentPlayer;
        }

        /// <summary>
        /// 런이 진행 중인지 확인
        /// </summary>
        public bool IsRunActive()
        {
            return isRunActive;
        }

        /// <summary>
        /// 플레이어 상태 로그 출력 (디버그용)
        /// </summary>
        public void LogPlayerStatus()
        {
            if (currentPlayer != null)
            {
                currentPlayer.LogStatus();
            }
            else
            {
                Debug.LogWarning("[RunManager] No active player");
            }
        }
    }
}
