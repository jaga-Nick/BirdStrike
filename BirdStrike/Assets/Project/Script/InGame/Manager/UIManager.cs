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

    private Boss _boss; // InGameシーンのボスを保持する

    void Start()
    {
        UpdateUI();
    }
    
    void Update()
    {
        // ゲーム中でなければUI更新はしない
        if (GameManager.Instance().Status != GAME_STATUS.INGAME)
        {
            if(uiIngame.activeSelf) uiIngame.SetActive(false);
            return;
        }

        // ゲーム中になったら一度だけインゲームUIを有効化
        if(!uiIngame.activeSelf) uiIngame.SetActive(true);
        

        // 残機表示
        if (GameManager.Instance().player != null)
        {
            lifeCount.text = GameManager.Instance().player.life.ToString();
        }

        // スコア表示
        scoreCount.text = GameManager.Instance().Score.ToString("D6");

        // ボスHPバー表示
        if (_boss == null)
        {
            // シーンからボスを探してきて保持する
            _boss = FindObjectOfType<Boss>();
            if (_boss != null)
            {
                bossHpBar.gameObject.SetActive(true);
            }
        }
        else
        {
            // ボスが見つかっていればHPを更新
            bossHpBar.fillAmount = _boss.hp / _boss.MaxHP;
        }
    }

    public void UpdateUI()
    {
        uiReady.SetActive(GameManager.Instance().Status == GAME_STATUS.READY);
        // インゲームUIの表示はUpdateに任せる
        // uiIngame.SetActive(GameManager.Instance().Status == GAME_STATUS.INGAME);
    }
}