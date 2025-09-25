using UnityEngine;
using System.Collections.Generic;
using UniRx;
using Cysharp.Threading.Tasks;
using InGame.Model;
using InGame.View;
using System.Threading;

namespace InGame.Presenter
{
    /// <summary>
    /// ボスのPresenter
    /// </summary>
    [RequireComponent(typeof(BossView))]
    public class BossPresenter : MonoBehaviour
    {
        // --- MVP ---
        private BossModel _model;
        private BossView _view;

        // --- State ---
        private bool _isSpecialMoving = false; // 特殊な移動中か
        
        // --- External References ---
        private Unit _target;
        private List<BossPartPresenter> _parts;
        private Missile _missile; // OnMissileLoad/Launchで使用

        // --- Events ---
        private readonly Subject<BossPresenter> _onDeathSubject = new Subject<BossPresenter>();
        public System.IObservable<BossPresenter> OnDeathAsObservable => _onDeathSubject;

        /// <summary>
        /// このボスを初期化する。GameManagerから呼び出される。
        /// </summary>
        public void Initialize(BossData data, List<BossPartPresenter> parts, Unit target)
        {
            _view = GetComponent<BossView>();
            if (_view == null)
            {
                Debug.LogError("BossView component not found.", this);
                return;
            }

            try
            {
                _model = new BossModel(data, parts);
            }
            catch (System.ArgumentNullException e)
            {
                Debug.LogError($"Failed to initialize BossModel: {e.Message}", this);
                this.gameObject.SetActive(false);
                return;
            }

            _target = target;
            _parts = parts;

            // 各部位の死亡通知を購読
            foreach (var part in _parts)
            {
                part.OnDeathAsObservable
                    .Subscribe(p => _model.OnPartDestroyed(p as BossPartPresenter))
                    .AddTo(this);
            }

            EnterAsync(data).Forget();
        }

        private void Update()
        {
            if (_model == null) return;
            
            if (_target != null)
            {
                _view.RotateBattery(_target.transform.position);
            }
            
            if (!_isSpecialMoving)
            {
                _view.UpdateNormalMovement(_model.moveSpeed, _model.moveDistance);
            }
        }
        
        private async UniTaskVoid EnterAsync(BossData data)
        {
            var token = this.GetCancellationTokenOnDestroy();
            List<UniTask> moveTasks = new List<UniTask>
            {
                MoveToAsync(data.entryTargetPosition, _model.speed)
            };
            for (int i = 0; i < _parts.Count; i++)
            {
                moveTasks.Add(_parts[i].MoveToAsync(data.parts[i].entryTargetPosition, _model.speed));
            }

            await UniTask.WhenAll(moveTasks);
            
            _view.OnEntryComplete();
            foreach (var part in _parts)
            {
                part.OnEntryComplete();
            }
            
            AttackAsync().Forget();
        }

        private async UniTaskVoid AttackAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();
            while (_model.hp > 0 && this.gameObject.activeSelf)
            {
                var sequence = _model.GetCurrentAttackSequence();
                if (sequence != null)
                {
                    foreach (var command in sequence)
                    {
                        if (_model.hp <= 0) break;
                        await ExecuteCommand(command, token);
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        
        private async UniTask ExecuteCommand(AttackCommand command, CancellationToken token)
        {
            switch (command.commandType)
            {
                case "WAIT":
                    await UniTask.Delay(System.TimeSpan.FromSeconds(command.duration), cancellationToken: token);
                    break;
                case "SHOOT_RADIAL":
                    Fire(command);
                    break;
                case "SHOOT_TARGET":
                    for (int i = 0; i < command.count; i++)
                    {
                        if (_model.hp <= 0) break;
                        Fire2(command);
                        await UniTask.Delay(System.TimeSpan.FromSeconds(command.interval), cancellationToken: token);
                    }
                    break;
                case "ULTRA_ATTACK":
                    await UltraAttackAsync(command, token);
                    break;
            }
        }

        private void Fire(AttackCommand command)
        {
            var bulletData = DataManager.Instance.GetBulletData(command.bulletName);
            if (bulletData == null) return;

            float angleStep = command.spreadAngle / (command.count > 1 ? command.count - 1 : 1);
            float startAngle = command.count > 1 ? -command.spreadAngle / 2 : 0;
        
            for (int i = 0; i < command.count; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                Vector3 shotDirection = rotation * Vector3.left;

                GameObject go = BulletManager.Instance().GetBullet(command.bulletName);
                if (go == null) continue;
            
                go.transform.position = _view.firePoint.position;
                go.transform.rotation = Quaternion.FromToRotation(Vector3.left, shotDirection);
                
                var bullet = go.GetComponent<Bullet>();
                bullet.Initialize(bulletData, SIDE.ENEMY);
                bullet.direction = shotDirection.normalized;
            }
        }

        private void Fire2(AttackCommand command)
        {
            var bulletData = DataManager.Instance.GetBulletData(command.bulletName);
            if (bulletData == null) return;

            GameObject go = BulletManager.Instance().GetBullet(command.bulletName);
            if (go == null) return;

            go.transform.position = _view.firePoint2.position;
            var bullet = go.GetComponent<Bullet>();
            bullet.Initialize(bulletData, SIDE.ENEMY);
            bullet.direction = (_target.transform.position - _view.firePoint2.position).normalized;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.left, bullet.direction);
        }

        private async UniTask UltraAttackAsync(AttackCommand command, CancellationToken token)
        {
            _isSpecialMoving = true;
            await MoveToAsync(new Vector3(5, 4, 0), _model.speed);
            
            await _view.PlayUltraAttackAnimationAsync();

            await MoveToAsync(new Vector3(5, 0, 0), _model.speed);
            _view.OnEntryComplete(); // 移動完了をViewに通知
            _isSpecialMoving = false;
        }

        private async UniTask MoveToAsync(Vector3 pos, float moveSpeed)
        {
            var token = this.GetCancellationTokenOnDestroy();
            while (Vector3.Distance(transform.position, pos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            transform.position = pos;
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (_model.isInvincible) return;

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
            _onDeathSubject.OnNext(this);
            Destroy(gameObject);
        }
        
        // アニメーションイベントから呼ばれる
        public void OnMissileLoad() { /* ... */ }
        public void OnMissileLaunch() { /* ... */ }

        private void OnDestroy()
        {
            _onDeathSubject.Dispose();
        }
    }
}