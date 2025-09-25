using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using Common;
using InGame.Model;
using InGame.View;

namespace InGame.Presenter
{
    /// <summary>
    /// プレイヤーのPresenter。ModelとViewを仲介し、ゲームロジックの司令塔となる。
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class PlayerPresenter : MonoBehaviour
    {
        // Model
        private PlayerModel _model;
        
        // View
        private PlayerView _view;

        // --- 状態管理 ---
        private bool _isDead = false;
        private float _invincibleTimer = 0f;
        private float _fireTimer = 0f;

        // --- 外部依存 ---
        private InputSystem_Actions _actionMap;
        private IDisposable _bombSubscription; // ボム入力の購読を管理

        /// <summary>
        /// このPresenterを初期化する。GameManagerから呼び出される。
        /// </summary>
        public void Initialize(PlayerData data)
        {
            _view = GetComponent<PlayerView>();
            if (_view == null)
            {
                Debug.LogError("PlayerView component not found on this GameObject.", this);
                return;
            }

            try
            {
                _model = new PlayerModel(data);
            }
            catch (ArgumentNullException e)
            {
                Debug.LogError($"Failed to initialize PlayerModel: {e.Message}", this);
                // データがないと動作できないので、オブジェクトを非アクティブ化する
                this.gameObject.SetActive(false);
                return;
            }

            // 初期位置を設定
            _view.SetPosition(_model.spawnPosition);

            InitializeInput();
        }

        /// <summary>
        /// 入力システムの初期化とイベント購読を行う。
        /// </summary>
        private void InitializeInput()
        {
            var inputManager = InputSystemActionsManager.Instance();
            // エラーハンドリング: InputSystemActionsManagerが見つからない場合
            if (inputManager == null)
            {
                Debug.LogError("InputSystemActionsManager instance not found in the scene.");
                return;
            }

            _actionMap = inputManager.GetInputSystem_Actions();
            inputManager.PlayerEnable();

            // 以前の購読が残っていれば解除
            _bombSubscription?.Dispose();
            // ボムボタンの入力をUniRxで購読
            _bombSubscription = Observable.FromEvent<UnityEngine.InputSystem.InputAction.CallbackContext>(
                handler => _actionMap.Player.Bomb.started += handler,
                handler => _actionMap.Player.Bomb.started -= handler)
                .Subscribe(_ => UseBomb())
                .AddTo(this);
        }

        private void Update()
        {
            // 初期化が完了していない、または死亡している場合は処理しない
            if (_model == null || _isDead) return;

            // 無敵タイマーの更新
            if (_invincibleTimer > 0)
            {
                _invincibleTimer -= Time.deltaTime;
            }

            HandleMovement();
            HandleFire();
        }
        
        public PlayerModel GetModel()
        {
            return _model;
        }

        private void HandleMovement()
        {
            Vector2 moveInput = _actionMap.Player.Move.ReadValue<Vector2>();
            
            // 低速状態かを判定
            bool isSlowing = _actionMap.Player.Slow.IsPressed();
            float currentSpeed = _model.speed * (isSlowing ? _model.slowMagnification : 1f);
            
            // 現在位置に移動量を加算
            var newPosition = transform.position + (Vector3)moveInput * currentSpeed * Time.deltaTime;
            
            _view.SetPosition(newPosition);
            _view.ClampPosition();
        }

        private void HandleFire()
        {
            _fireTimer += Time.deltaTime;
            if (_actionMap.Player.Fire.IsPressed() && _fireTimer >= 1f / _model.fireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }
        
        /// <summary>
        /// 弾を発射する。
        /// </summary>
        private void Fire()
        {
            var bulletData = DataManager.Instance.GetBulletData(_model.bulletName);
            if (bulletData == null)
            {
                Debug.LogError($"Bullet data '{_model.bulletName}' not found.");
                return;
            }
            

            GameObject bulletGO = BulletManager.Instance.GetBullet(_model.bulletName);
            if (bulletGO == null) return; // プールが空などの場合

            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Initialize(bulletData, SIDE.PLAYER);
            bullet.transform.position = _view.firePoint.position;
            bullet.direction = Vector3.right;
            bullet.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// ボムの仕様
        /// </summary>
        private void UseBomb()
        {
            if (_model.TryUseBomb())
            {
                // シーン上の全ての敵弾とミサイルを破壊
                int scoreGained = 0;
                var bullets = FindObjectsOfType<Bullet>();
                foreach(var bullet in bullets)
                {
                    if (bullet.side == SIDE.ENEMY)
                    {
                        scoreGained += bullet.scoreValue;
                        BulletManager.Instance.ReturnBullet(bullet.gameObject, bullet.bulletName, bullet.side);
                    }
                }
                
                var missiles = FindObjectsOfType<Missile>();
                foreach(var missile in missiles)
                {
                    scoreGained += missile.scoreValue;
                    missile.Explod(); // ミサイルは専用の破壊処理
                }

                GameManager.Instance().AddScore(scoreGained);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDead || _invincibleTimer > 0) return;

            // 敵または敵弾に接触した場合
            bool isHitByEnemy = other.GetComponent<Boss>() != null || 
                                other.GetComponent<BossPart>() != null;
            var bullet = other.GetComponent<Bullet>();
            bool isHitByEnemyBullet = bullet != null && bullet.side == SIDE.ENEMY;

            if (isHitByEnemy || isHitByEnemyBullet)
            {
                Die();
            }
        }

        private void Die()
        {
            _isDead = true;
            _model.Die();
            _view.PlayDieAnimation();
            
            if (_model.life > 0)
            {
                // 復活処理
                Rebirth().Forget();
            }
            else
            {
                // ゲームオーバー処理 (GameManagerに通知)
                GameManager.Instance().OnPlayerDied();
            }
        }

        /// <summary>
        /// 復活処理をUniTaskで行う。
        /// </summary>
        private async UniTaskVoid Rebirth()
        {
            // 復活までの待機
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 状態をリセット
            _isDead = false;
            _model.ResetHp();
            _invincibleTimer = _model.invincibleTime;
            
            // 見た目をリセット
            _view.SetPosition(_model.spawnPosition);
            _view.PlayFlyAnimation();
        }

        /// <summary>
        /// ゲーム開始時に呼び出される。
        /// </summary>
        public void OnGameStart()
        {
            _view.PlayFlyAnimation();
        }

        private void OnDestroy()
        {
            _bombSubscription?.Dispose();
        }
    }
}