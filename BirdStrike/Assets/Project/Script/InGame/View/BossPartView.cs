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
        public Transform firePoint;
        public Transform battery;

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
        /// 砲台を指定されたターゲットの方向に向ける。
        /// </summary>
        public void RotateBattery(Vector3 targetPosition)
        {
            if (battery == null) return;
            Vector3 dir = (targetPosition - battery.position).normalized;
            battery.transform.rotation = Quaternion.FromToRotation(Vector3.left, dir);
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