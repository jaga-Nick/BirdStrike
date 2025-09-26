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
using InGame.NonMVP;

namespace Common
{
    /// <summary>
    /// ゲーム全体の進行状態を管理するシングルトンクラス
    /// </summary>
    public class GameManager : GlobalMonoSingletonBase<GameManager>
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
        
        public int Score { get; private set; } = 0;
        public bool IsGameWon { get; private set; } = false;
    
        [HideInInspector] public PlayerPresenter player;
        
        [Header("Scene Addressable Keys")]
        public string titleSceneKey = "Title";
        public string inGameSceneKey = "InGame";
        public string resultSceneKey = "Result";
        
    
        void Start()
        {
            if (Instance() != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
    
            Status = GAME_STATUS.READY;
            ResetScore();
        }
        
        /// <summary>
        /// Gameをスタートする
        /// </summary>
        public async void StartGame()
        {
            await SceneLoader.Instance().LoadSceneAsync(inGameSceneKey);
            InitializeInGameObjects().Forget();
        }
        
        /// <summary>
        /// データ読み込みと初期化
        /// </summary>
        private async UniTaskVoid InitializeInGameObjects()
        {
            // jsonからデータ読み込み
            await DataManager.Instance.LoadDataAsync();
            if (!DataManager.Instance.IsDataLoaded) return;
            await BulletManager.Instance.InitializePoolsAsync();
            
            Status = GAME_STATUS.INGAME;
            var token = this.GetCancellationTokenOnDestroy();
            
            var playerData = DataManager.Instance.Player;
            var bossData = DataManager.Instance.Boss;
            
            // プレイヤー生成
            GameObject playerGO = await Addressables.InstantiateAsync(playerData.addressableKey, playerData.spawnPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
            this.player = playerGO.GetComponent<PlayerPresenter>();
            player.Initialize(playerData);
            player.OnGameStart();
            
            // ボスの部位生成
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
            
            // ボス生成
            GameObject bossGO = await Addressables.InstantiateAsync(bossData.addressableKey, bossData.initialPosition, Quaternion.identity).ToUniTask(cancellationToken: token);
            BossPresenter boss = bossGO.GetComponent<BossPresenter>();
            
            boss.OnDeathAsObservable.Subscribe(_ => OnBossDied()).AddTo(boss);
            boss.Initialize(bossData, spawnedParts, this.player.transform);
            
        }
    
        /// <summary>
        /// スコア加算
        /// </summary>
        public void AddScore(int amount)
        {
            Score += amount;
            GameEvents.OnScoreUpdated?.Invoke(Score);
        }

        /// <summary>
        /// スコアをリセット
        /// </summary>
        public void ResetScore()
        {
            Score = 0;
        }
        
        /// <summary>
        /// プレイヤーが死亡した際に呼び出される
        /// </summary>
        public void OnPlayerDied()
        {
            // プレイヤーの復活処理はPlayerPresenter自身が行うため、ここではゲームオーバー判定のみ
            // PlayerPresenterからモデルを取得して残機を確認
            var playerModel = player.GetModel();
            if (playerModel.life <= 0)
            {
                IsGameWon = false;
                Status = GAME_STATUS.OVER;
                SceneLoader.Instance().LoadSceneAsync(resultSceneKey).Forget();
            }
        }
        
        /// <summary>
        /// ボスが死亡した際に呼び出される
        /// </summary>
        private void OnBossDied()
        {
            IsGameWon = true;
            Status = GAME_STATUS.OVER;
            SceneLoader.Instance().LoadSceneAsync(resultSceneKey).Forget();
        }
        
        /// <summary>
        /// Titleに戻る
        /// </summary>
        public void ReturnToTitle()
        {
            Status = GAME_STATUS.READY;
            Score = 0;
            this.player = null;
            ResetScore();
            SceneLoader.Instance().LoadSceneAsync(titleSceneKey).Forget();
        }
    }
}
