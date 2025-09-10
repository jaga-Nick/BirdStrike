using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SearchService;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    

    public GAME_STATUS status;

    public GAME_STATUS Status
    {
        get { return status; }
        set { 
            status = value;
            UIManager.Instance.UpdateUI();
        }
    }

    public Player player;   
    public int currentLevelId = 1;

    void Start()
    {
        
        Status = GAME_STATUS.READY;
        player.OnDeath += Player_OnDeath;

    }

    private void Player_OnDeath(Unit sender)
    {
        if(player.life <= 0)
        {
            Status = GAME_STATUS.OVER;
            UIManager.Instance.UILeveLose();
            UnitManager.Instance.Clear();

        }
        else
        {
            player.Rebirth();
        }
    }

    public void StartGame()
    {
        InitPlayer();
        this.Status = GAME_STATUS.INGAME;
        player.Fly();
        LoadLevel();

    }

    private void LoadLevel()
    {
        LevelManager.Instance.LoadLevel(currentLevelId);        
        LevelManager.Instance.level.OnLevelEnd = OnLevelEnd;

    }

    void OnLevelEnd(Level.LEVEL_RESULT result)
    {
        if(result == Level.LEVEL_RESULT.SUCCESS && currentLevelId <3)
        {
            currentLevelId++;
            LoadLevel();
        }
        else
        {
            Status = GAME_STATUS.OVER;
            UIManager.Instance.UILeveClear();
        }

    }

    public void Restart()
    {
        Status = GAME_STATUS.READY;
        player.Init();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    

    void InitPlayer()
    {
        player.Init();
    }
}
