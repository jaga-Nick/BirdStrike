using UnityEngine;
using Cysharp.Threading.Tasks;

namespace InGame.View
{
    /// <summary>
    /// ボスの見た目（View）を担当するクラス。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BossView : MonoBehaviour
    {
        private Animator _animator;
        
        [Header("View References")]
        public Transform firePoint;
        public Transform firePoint2;
        public Transform firePoint3;
        public Transform battery;
        
        private Vector3 _initialPosition;
        private bool _entryComplete = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        /// <summary>
        /// 指定された座標にオブジェクトを移動させる。
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// 飛行アニメーションを再生する。
        /// </summary>
        public void PlayFlyAnimation()
        {
            _animator.SetTrigger("Fly");
        }

        /// <summary>
        /// 砲台を指定されたターゲットの方向に向ける。
        /// </summary>
        public void RotateBattery(Vector3 targetPosition)
        {
            if (battery == null) return;
            Vector3 dir = (targetPosition - battery.position).normalized;
            battery.transform.rotation = Quaternion.FromToRotation(Vector3.left, dir);
        }

        /// <summary>
        /// 通常の上下移動を行う。
        /// </summary>
        public void UpdateNormalMovement(float moveSpeed, float moveDistance)
        {
            if (!_entryComplete) return;
            float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
            transform.position = _initialPosition + new Vector3(0, yOffset, 0);
        }
        
        /// <summary>
        /// 移動完了後の初期座標を設定する。
        /// </summary>
        public void OnEntryComplete()
        {
            _initialPosition = transform.position;
            _entryComplete = true;
        }

        /// <summary>
        /// UltraAttackのアニメーションと待機を行う。
        /// </summary>
        public async UniTask PlayUltraAttackAnimationAsync()
        {
            _animator.SetTrigger("Skill");
            // アニメーションイベントでミサイルをロード・発射することを想定
            await UniTask.Delay(System.TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}