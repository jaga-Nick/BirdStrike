using UnityEngine;

namespace InGame.View
{
    /// <summary>
    /// プレイヤーの見た目（View）を担当するクラス。
    /// オブジェクトの移動やアニメーション再生など、Unityコンポーネントの操作に特化する。
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    public class PlayerView : MonoBehaviour
    {
        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Vector2 _worldPosLeftBottom;
        private Vector2 _worldPosTopRight;

        /// <summary>
        /// 弾の発射口のTransform。Presenterが位置を取得するために使用する。
        /// </summary>
        public Transform firePoint;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();

            // エラーハンドリング: MainCameraが見つからない場合
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera is not found in the scene. Please ensure a camera is tagged as 'MainCamera'.");
                return;
            }
            _worldPosLeftBottom = Camera.main.ViewportToWorldPoint(Vector2.zero);
            _worldPosTopRight = Camera.main.ViewportToWorldPoint(Vector2.one);
        }
        
        /// <summary>
        /// 指定された座標にオブジェクトを移動させる。
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// 画面端から出ないように座標を制限する。
        /// </summary>
        public void ClampPosition()
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _worldPosLeftBottom.x, _worldPosTopRight.x);
            pos.y = Mathf.Clamp(pos.y, _worldPosLeftBottom.y, _worldPosTopRight.y);
            transform.position = pos;
        }

        /// <summary>
        /// 飛行アニメーションを再生する。
        /// </summary>
        public void PlayFlyAnimation()
        {
            _rigidbody.simulated = true;
            _animator.SetTrigger("Fly");
        }

        /// <summary>
        /// 待機アニメーションを再生する。
        /// </summary>
        public void PlayIdleAnimation()
        {
            _rigidbody.simulated = false;
            _animator.SetTrigger("Idle");
        }

        /// <summary>
        /// 死亡アニメーションを再生する。
        /// </summary>
        public void PlayDieAnimation()
        {
            _animator.SetTrigger("Die");
        }
    }
}