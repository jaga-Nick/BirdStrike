using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks; // UniTaskの名前空間を追加
using UniRx; // UniRxの名前空間を追加

public class GameManager : MonoSingleton<GameManager>
{
    public GAME_STATUS status;
    public GAME_STATUS Status
    {
        get { return status; }
        set
        {
            status = value;
            UIManager.Instance.UpdateUI();
        }
    }

    [HideInInspector] public Player player;
    public int currentLevelId = 1;

    [Header("Addressable Keys")]
    public string playerKey = "Player";
    public string bossBodyKey = "BossBody";
    public List<string> bossPartKeys = new List<string> { "BossPart1", "BossPart2" };


    void Start()
    {
        Status = GAME_STATUS.READY;
    }

    private void Player_OnDeath(Unit sender)
    {
        if (player.life <= 0)
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

    // --- ▼▼▼ StartGameをUniTaskを使った非同期メソッドに修正 ▼▼▼ ---
    public async void StartGame()
    {
        // TODO: ここでJSONを読み込む
        Debug.Log("Loading game data...");

        Status = GAME_STATUS.INGAME;
        
        // CancellationTokenを取得しておくと、シーン遷移時などに非同期処理を安全に中断できます
        var token = this.GetCancellationTokenOnDestroy();

        // 1. プレイヤーをAddressableで生成
        Debug.Log("1. Spawning Player...");
        GameObject playerGO = await Addressables.InstantiateAsync(playerKey, Vector3.zero, Quaternion.identity).ToUniTask(cancellationToken: token);
        this.player = playerGO.GetComponent<Player>();
        this.player.OnDeathAsObservable
            .Subscribe(Player_OnDeath)
            .AddTo(this.player); // 購読をPlayer自身に紐づけて管理
        
        player.Init();
        player.Fly();

        // 2. ボスの部位をAddressableで生成
        Debug.Log("2. Spawning Boss Parts...");
        var partSpawnTasks = new List<UniTask<GameObject>>();
        foreach (var partKey in bossPartKeys)
        {
            // 全ての部位の読み込みを同時に開始する
            partSpawnTasks.Add(Addressables.InstantiateAsync(partKey).ToUniTask(cancellationToken: token));
        }
        // 全ての部位の読み込みが終わるのを待つ
        GameObject[] partGOs = await UniTask.WhenAll(partSpawnTasks);
        List<BossPart> spawnedParts = new List<BossPart>();
        foreach(var pgo in partGOs)
        {
            spawnedParts.Add(pgo.GetComponent<BossPart>());
        }


        // 3. ボス本体をAddressableで生成
        Debug.Log("3. Spawning Boss Body...");
        GameObject bossGO = await Addressables.InstantiateAsync(bossBodyKey).ToUniTask(cancellationToken: token);
        Boss boss = bossGO.GetComponent<Boss>();

        // 4. ボス本体に部位を登録し、初期化
        Debug.Log("4. Registering parts to Boss...");
        boss.InitializeParts(spawnedParts, this.player);

        Debug.Log("5. Initialization complete. Game Start!");
    }
    // --- ▲▲▲ StartGameをUniTaskを使った非同期メソッドに修正 ▲▲▲ ---

    void OnLevelEnd(Level.LEVEL_RESULT result)
    {
        if (result == Level.LEVEL_RESULT.SUCCESS)
        {
            Status = GAME_STATUS.OVER;
            UIManager.Instance.UILeveClear();
        }
    }

    public void Restart()
    {
        Status = GAME_STATUS.READY;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}