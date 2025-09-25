using UnityEngine;

namespace InGame.View
{
    /// <summary>
    /// ボスの部位の見た目（View）を担当するクラス。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BossPartView : MonoBehaviour
    {
        private Animator _animator;
        /// <summary>
        /// 弾の発射口。Presenterが使用する。
        /// </summary>
        public Transform firePoint;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 指定された座標にオブジェクトを移動
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
        /// 死亡アニメーションを再生し、オブジェクトを破棄する。
        /// </summary>
        public void PlayDieAnimationAndDestroy()
        {
            _animator.SetTrigger("Die");
            // アニメーションの長さに合わせて破棄（ここでは固定値）
            Destroy(gameObject, 0.2f); 
        }
    }
}