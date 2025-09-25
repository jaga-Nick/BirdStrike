using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UniRx;
using Common;
using System.Linq;
using InGame.Presenter;
using InGame.Model;
using InGame.View;

public class GameManager : GlobalMonoSingletonBase<GameManager>
{
    public GAME_STATUS status;
    public GAME_STATUS Status
    {
        get { return status; }
        set
        {
            status = value;
            if (InGame.UI.UIManager.Instance != null) InGame.UI.UIManager.Instance.UpdateUI();
        }
    }
    
    public int Score { get; private set; } = 0;

    [HideInInspector] public PlayerPresenter player;
    
    [Header("Scene Addressable Keys")]
    public string titleSceneKey = "Title";
    public string inGameSceneKey = "InGame";
    public string resultSceneKey = "Result";

    public bool IsGameWon { get; private set; } = false;

    void Start()
    {
        if (Instance() != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Status = GAME_STATUS.READY;
        Score = 0;
    }
    
    public async void StartGame()
    {
        await SceneLoader.Instance().LoadSceneAsync(inGameSceneKey);
        InitializeInGameObjects().Forget();
    }
    
    private async UniTaskVoid InitializeInGameObjects()
    {
        await DataManager.Instance.LoadDataAsync();
        if (!DataManager.Instance.IsDataLoaded) return;
        await BulletManager.Instance().InitializePoolsAsync();
        
        Status = GAME_STATUS.INGAME;
        var token = this.GetCancellationTokenOnDestroy();
        
        var playerData = DataManager.Instance.Player;
        var bossData = DataManager.Instance.Boss;
        
        GameObject playerGO = await Addressables.InstantiateAsync(playerData.addressableKey, playerData.spawnPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        this.player = playerGO.GetComponent<PlayerPresenter>();
        player.Initialize(playerData);
        player.OnGameStart();
        
        var partSpawnTasks = new List<UniTask<GameObject>>();
        foreach (var partData in bossData.parts)
        {
            partSpawnTasks.Add(Addressables.InstantiateAsync(partData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token));
        }
        GameObject[] partGOs = await UniTask.WhenAll(partSpawnTasks);
        List<BossPartPresenter> spawnedParts = new List<BossPartPresenter>();
        for(int i = 0; i < partGOs.Length; i++)
        {
            var part = partGOs[i].GetComponent<BossPartPresenter>();
            part.Initialize(bossData.parts[i], this.player.transform);
            spawnedParts.Add(part);
        }
        GameObject bossGO = await Addressables.InstantiateAsync(bossData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        BossPresenter boss = bossGO.GetComponent<BossPresenter>();
        
        boss.OnDeathAsObservable.Subscribe(_ => OnBossDefeated()).AddTo(boss);
        boss.Initialize(bossData, spawnedParts, this.player.transform);
        
        //InGame.UI.UIManager.Instance().InitializeInGameUI(boss);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        GameEvents.OnScoreUpdated?.Invoke(Score);
    }
    
    /// <summary>
    /// プレイヤーが死亡した際に呼び出される
    /// </summary>
    public void OnPlayerDied()
    {
        // プレイヤーの復活処理はPlayerPresenter自身が行うため、ここではゲームオーバー判定のみ
        // PlayerPresenterからモデルを取得して残機を確認
        var playerModel = player.GetModel(); // PlayerPresenterにGetModel()を追加する必要があります
        if (playerModel.life <= 0)
        {
            IsGameWon = false;
            Status = GAME_STATUS.OVER;
            SceneLoader.Instance().LoadSceneAsync(resultSceneKey).Forget();
        }
    }

    /*
    private void Player_OnDeath(Unit sender)
    {
        if (player.life <= 0)
        {
            IsGameWon = false;
            Status = GAME_STATUS.OVER;
            SceneLoader.Instance().LoadSceneAsync(resultSceneKey).Forget();
        }
        else
        {
            player.Rebirth();
        }
    }
    */
    
    private void OnBossDefeated()
    {
        IsGameWon = true;
        Status = GAME_STATUS.OVER;
        SceneLoader.Instance().LoadSceneAsync(resultSceneKey).Forget();
    }
    
    public void ReturnToTitle()
    {
        Status = GAME_STATUS.READY;
        Score = 0;
        this.player = null;
        SceneLoader.Instance().LoadSceneAsync(titleSceneKey).Forget();
    }
}