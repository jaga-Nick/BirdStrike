using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UniRx;

public class GameManager : MonoSingleton<GameManager>
{
    
    public int Score { get; private set; } = 0;
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

    [HideInInspector] public Player player;

    void Start()
    {
        Status = GAME_STATUS.READY;
        Score = 0;
    }

    private void Player_OnDeath(Unit sender)
    {
        if (player.life <= 0)
        {
            Status = GAME_STATUS.OVER;
            UIManager.Instance.UILeveLose();
            if (UnitManager.Instance != null) UnitManager.Instance.Clear();
        }
        else
        {
            player.Rebirth();
        }
    }
    
    public async void StartGame()
    {
        Debug.Log("Loading game data...");
        await DataManager.Instance.LoadDataAsync();

        if (!DataManager.Instance.IsDataLoaded)
        {
            Debug.LogError("Game start aborted due to data loading failure.");
            return;
        }
        
        // --- ▼▼▼ 修正 ▼▼▼ ---
        // BulletManagerにプールの初期化を指示
        await BulletManager.Instance.InitializePoolsAsync();
        // --- ▲▲▲ 修正 ▲▲▲ ---

        Status = GAME_STATUS.INGAME;
        var token = this.GetCancellationTokenOnDestroy();
        
        var playerData = DataManager.Instance.Player;
        var bossData = DataManager.Instance.Boss;

        // Player Spawn
        GameObject playerGO = await Addressables.InstantiateAsync(playerData.addressableKey, Vector3.zero, Quaternion.identity).ToUniTask(cancellationToken: token);
        this.player = playerGO.GetComponent<Player>();
        this.player.OnDeathAsObservable.Subscribe(Player_OnDeath).AddTo(this.player);
        player.Initialize(playerData);
        player.Fly();

        // Boss Parts Spawn
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

        // Boss Body Spawn
        GameObject bossGO = await Addressables.InstantiateAsync(bossData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        Boss boss = bossGO.GetComponent<Boss>();

        // Boss Initialization
        boss.Initialize(bossData, spawnedParts, this.player);

        Debug.Log("5. Initialization complete. Game Start!");
    }

    public void Restart()
    {
        Status = GAME_STATUS.READY;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void AddScore(int amount)
    {
        Score += amount;
        Debug.Log("Score: " + Score);
        // TODO: ここでUIManagerを呼び出してスコア表示を更新する
    }
}