using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public GameObject uiReady;
    public GameObject uiIngame;
    public GameObject uiGameOver;

    //public Slider hpbar;
    public Text uiLife;
    //public Text uiLevelName;
    public Text uiLevelStartName;
    public Text uiLevelEndText;

    public GameObject uiLevelStart;
    public GameObject uiLevelEnd;


    // Start is called before the first frame update
    void Start()
    {
        this.uiReady.SetActive(true);
    }

    public void UpdateLife(int score)
    {
        uiLife.text = score.ToString();
    }

    public void ShowLevelStart(string name)
    {
        uiLevelStartName.text = name;
        
        uiLevelStart.SetActive(true);
    }

    public void UILeveLose()
    {
        uiLevelEndText.text = "YOU LOSE";
    }

    public void UILeveClear()
    {
        uiLevelEndText.text = "YOU WIN";
    }
    
    void Update()
    {
        
        // playerが生成済みの場合のみ残機表示を更新する
        if (GameManager.Instance().player != null)
            uiLife.text = GameManager.Instance().player.life.ToString();
    }

    public void UpdateUI()
    {
        this.uiReady.SetActive(GameManager.Instance().Status == GAME_STATUS.READY);
        this.uiIngame.SetActive(GameManager.Instance().Status == GAME_STATUS.INGAME);
        this.uiGameOver.SetActive(GameManager.Instance().Status == GAME_STATUS.OVER);
        
    }
}