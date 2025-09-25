using UnityEngine;
using UnityEngine.UI;
//using InGame.Manager; // GameManagerにアクセスするため
using Common;

namespace InGame.UI // UI用のnamespace
{
    public class UIManager : LocalMonoSingletonBase<UIManager>
    {
        [Header("UI Panels")]
        public GameObject uiReady;
        public GameObject uiIngame;

        [Header("In-Game UI")]
        public Text lifeCount;
        public Text scoreCount;
        public Image bossHpBar;

        private void Awake()
        {
            // イベントの購読を開始
            GameEvents.OnLifeUpdated += UpdateLifeText;
            GameEvents.OnScoreUpdated += UpdateScoreText;
            GameEvents.OnBossHpUpdated += UpdateBossHpBar;
        }

        private void OnDestroy()
        {
            // オブジェクトが破棄される際に、必ずイベントの購読を解除する
            GameEvents.OnLifeUpdated -= UpdateLifeText;
            GameEvents.OnScoreUpdated -= UpdateScoreText;
            GameEvents.OnBossHpUpdated -= UpdateBossHpBar;
        }

        void Start()
        {
            // 初期状態ではインゲームUIとHPバーを非表示
            uiIngame.SetActive(false);
            if(bossHpBar != null) bossHpBar.gameObject.SetActive(false);
            UpdateUI();
        }

        private void UpdateLifeText(int life)
        {
            if (lifeCount != null)
            {
                lifeCount.text = life.ToString();
            }
        }

        private void UpdateScoreText(int score)
        {
            if (scoreCount != null)
            {
                scoreCount.text = score.ToString("D6");
            }
        }

        private void UpdateBossHpBar(float currentHp, float maxHp)
        {
            if (bossHpBar != null)
            {
                if (!bossHpBar.gameObject.activeSelf)
                {
                    bossHpBar.gameObject.SetActive(true);
                }
                if (maxHp > 0)
                {
                    bossHpBar.fillAmount = currentHp / maxHp;
                }
            }
        }
        

        public void UpdateUI()
        {
            var gm = GameManager.Instance();
            if (gm != null)
            {
                uiReady.SetActive(gm.Status == GAME_STATUS.READY);
                uiIngame.SetActive(gm.Status == GAME_STATUS.INGAME);
            }
        }
    }
}