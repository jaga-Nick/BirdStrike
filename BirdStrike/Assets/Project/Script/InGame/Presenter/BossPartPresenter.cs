using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using InGame.Model;
using InGame.View;

namespace InGame.Presenter
{
    /// <summary>
    /// ボスの部位のPresenter、ModelとViewを仲介し、ゲームロジックの司令塔となる。
    /// </summary>
    [RequireComponent(typeof(BossPartView))]
    public class BossPartPresenter : MonoBehaviour
    {
        // --- MVP ---
        private BossPartModel _model;
        private BossPartView _view;

        // --- State ---
        private bool _isInitialized = false;
        private bool _entryComplete = false;
        private float _fireTimer = 0f;
        private Transform _target; // 攻撃対象

        // --- Events ---
        private readonly Subject<BossPartPresenter> _onDeathSubject = new Subject<BossPartPresenter>();
        public System.IObservable<BossPartPresenter> OnDeathAsObservable => _onDeathSubject;

        /// <summary>
        /// この部位をJSONデータで初期化する。
        /// </summary>
        public void Initialize(BossPartData data, Transform target)
        {
            _view = GetComponent<BossPartView>();
            if (_view == null)
            {
                Debug.LogError("BossPartView component not found.", this);
                return;
            }

            try
            {
                _model = new BossPartModel(data);
            }
            catch (System.ArgumentNullException e)
            {
                Debug.LogError($"Failed to initialize BossPartModel: {e.Message}", this);
                this.gameObject.SetActive(false);
                return;
            }

            _target = target;
            _isInitialized = true;
        }
        
        private void Update()
        {
            if (!_entryComplete) return;

            // Modelに座標計算を依頼し、Viewに反映
            _view.SetPosition(_model.CalculateCurrentPosition());
            
            HandleFire();
        }

        private void HandleFire()
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / _model.fireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }

        private void Fire()
        {
            if (_target == null) return;
            
            var bulletData = DataManager.Instance.GetBulletData(_model.bulletName);
            if (bulletData == null) return;

            GameObject bulletGO = BulletManager.Instance.GetBullet(_model.bulletName);
            if (bulletGO == null) return;

            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Initialize(bulletData, SIDE.ENEMY);
            bullet.transform.position = _view.firePoint.position;
            bullet.direction = (_target.transform.position - _view.firePoint.position).normalized;
            bullet.transform.rotation = Quaternion.FromToRotation(Vector3.left, bullet.direction);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_isInitialized || _model.hp <= 0) return;

            var bullet = col.GetComponent<Bullet>();
            if (bullet != null && bullet.side == SIDE.PLAYER)
            {
                _model.TakeDamage(bullet.power);
                if (_model.hp <= 0)
                {
                    Die();
                }
            }
        }

        private void Die()
        {
            _onDeathSubject.OnNext(this); // 死亡を通知
            _view.PlayDieAnimationAndDestroy();
            this.enabled = false; // Updateを停止
        }
        
        /// <summary>
        /// 指定座標へのエントリー移動を行う。
        /// </summary>
        public async UniTask MoveToAsync(Vector3 targetPosition, float speed)
        {
            var token = this.GetCancellationTokenOnDestroy();
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                if (token.IsCancellationRequested) return;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            transform.position = targetPosition;
        }

        /// <summary>
        /// 移動完了を通知され、通常行動を開始する。
        /// </summary>
        public void OnEntryComplete()
        {
            _model.SetInitialPosition(transform.position);
            _entryComplete = true;
            _view.PlayFlyAnimation();
        }

        private void OnDestroy()
        {
            _onDeathSubject.Dispose();
        }
    }
}