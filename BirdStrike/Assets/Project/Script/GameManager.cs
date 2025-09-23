using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UniRx;
using Common;
using System.Linq;

public class GameManager : SingletonMonoBehaviourBase<GameManager>
{
    public GAME_STATUS status;
    public GAME_STATUS Status
    {
        get { return status; }
        set
        {
            status = value;
            if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
        }
    }

    // --- ▼▼▼ シンプルな変数に戻す ▼▼▼ ---
    public int Score { get; private set; } = 0;
    // --- ▲▲▲ シンプルな変数に戻す ▲▲▲ ---

    [HideInInspector] public Player player;
    
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

        // Player & Boss & Parts Spawn
        GameObject playerGO = await Addressables.InstantiateAsync(playerData.addressableKey, playerData.spawnPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        this.player = playerGO.GetComponent<Player>();
        this.player.OnDeathAsObservable.Subscribe(Player_OnDeath).AddTo(this.player);
        player.Initialize(playerData);
        player.Fly();
        
        var partSpawnTasks = new List<UniTask<GameObject>>();
        foreach (var partData in bossData.parts)
        {
            partSpawnTasks.Add(Addressables.InstantiateAsync(partData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token));
        }
        GameObject[] partGOs = await UniTask.WhenAll(partSpawnTasks);
        List<BossPart> spawnedParts = new List<BossPart>();
        for(int i = 0; i < partGOs.Length; i++)
        {
            var part = partGOs[i].GetComponent<BossPart>();
            part.Initialize(bossData.parts[i]);
            spawnedParts.Add(part);
        }
        GameObject bossGO = await Addressables.InstantiateAsync(bossData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        Boss boss = bossGO.GetComponent<Boss>();
        
        boss.OnDeathAsObservable.Subscribe(_ => OnBossDefeated()).AddTo(boss);
        boss.Initialize(bossData, spawnedParts, this.player);
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

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