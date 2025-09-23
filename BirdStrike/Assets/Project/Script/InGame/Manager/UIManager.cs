using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [Header("UI Panels")]
    public GameObject uiReady;
    public GameObject uiIngame;

    [Header("In-Game UI Elements")]
    public Text lifeCount;
    public Text scoreCount;
    public Image bossHpBar;

    private Boss _boss; // 監視対象のボス

    void Start()
    {
        UpdateUI();
        // ゲーム開始時はHPバーを非表示にしておく
        if (bossHpBar != null) bossHpBar.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// GameManagerから呼び出され、UIの監視対象を設定する
    /// </summary>
    public void InitializeInGameUI(Boss boss)
    {
        _boss = boss;
        if (_boss != null && bossHpBar != null)
        {
            bossHpBar.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (GameManager.Instance().Status != GAME_STATUS.INGAME)
        {
            if(uiIngame.activeSelf) uiIngame.SetActive(false);
            return;
        }
        if(!uiIngame.activeSelf) uiIngame.SetActive(true);

        // 残機表示
        if (GameManager.Instance().player != null)
        {
            lifeCount.text = GameManager.Instance().player.life.ToString();
        }

        // スコア表示
        scoreCount.text = GameManager.Instance().Score.ToString("D6");

        // ボスHPバー表示
        if (_boss != null)
        {
            // ボスのHPを更新
            bossHpBar.fillAmount = _boss.hp / _boss.MaxHP;
        }
    }

    public void UpdateUI()
    {
        uiReady.SetActive(GameManager.Instance().Status == GAME_STATUS.READY);
    }
}