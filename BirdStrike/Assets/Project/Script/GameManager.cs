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
    
    private List<string> bossPartKeys = new List<string> { "BossPart1", "BossPart2" };
    
    [Header("Boss Settings")]
    public Vector3 bossInitialPosition = new Vector3(15, 1.4f, 0); // ボスと部位の初期出現位置
    public Vector3 bossEntryTargetPosition = new Vector3(5, 1.4f, 0); // ボス本体の最終位置
    public List<Vector3> bossPartEntryTargetPositions = new List<Vector3> { new Vector3(2, 2, 0), new Vector3(2, -2, 0) }; // 各部位の最終位置


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
    
    public async void StartGame()
    {
        Debug.Log("Loading game data...");
        await DataManager.Instance.LoadDataAsync();
        
        if (!DataManager.Instance.IsDataLoaded)
        {
            Debug.LogError("Game start aborted due to data loading failure. Please check console for details.");
            // TODO: ここでユーザーにエラーを通知するUIを表示しても良い
            return;
        }

        Status = GAME_STATUS.INGAME;
        
        var token = this.GetCancellationTokenOnDestroy();
        
        // DataManagerから読み込んだデータを取得
        var playerData = DataManager.Instance.Player;
        var bossData = DataManager.Instance.Boss;


        // プレイヤーをAddressableで生成
        Debug.Log("1. Spawning Player...");
        GameObject playerGO = await Addressables.InstantiateAsync(playerData.addressableKey, Vector3.zero, Quaternion.identity).ToUniTask(cancellationToken: token);
        this.player = playerGO.GetComponent<Player>();
        
        // 購読と初期化
        this.player.OnDeathAsObservable.Subscribe(Player_OnDeath).AddTo(this.player);
        player.Initialize(playerData);
        // player.Init(); // 既存の初期化も呼んでおく
        player.Fly();

        // ボスの部位をAddressableで生成
        Debug.Log("2. Spawning Boss Parts...");
        var partSpawnTasks = new List<UniTask<GameObject>>();
        foreach (var partData in bossData.parts)
        {
            // partDataからAddressableキーと初期位置を取得して生成
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


        // ボス本体をAddressableで生成
        Debug.Log("3. Spawning Boss Body...");
        GameObject bossGO = await Addressables.InstantiateAsync(bossData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
        Boss boss = bossGO.GetComponent<Boss>();

        // ステップ5: ボス本体の初期化（部位リストなどを渡す）
        Debug.Log("4. Registering parts to Boss...");
        boss.Initialize(bossData, spawnedParts, this.player);

        Debug.Log("5. Initialization complete. Game Start!");
    }


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