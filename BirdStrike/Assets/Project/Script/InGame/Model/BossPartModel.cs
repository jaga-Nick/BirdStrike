using UnityEngine;

namespace InGame.Model
{
    /// <summary>
    /// ボスの部位のデータと状態を管理するクラス。
    /// </summary>
    public class BossPartModel
    {
        // --- Properties ---
        public float hp { get; private set; }
        public float maxHp { get; private set; }
        public float fireRate { get; private set; }
        public string bulletName { get; private set; }

        // --- Movement Parameters ---
        private readonly float _moveSpeed;
        private readonly float _moveDistanceY;
        private readonly float _moveDistanceX;

        // --- State ---
        private Vector3 _initialPosition;

        /// <summary>
        /// JSONデータからモデルを初期化する。
        /// </summary>
        public BossPartModel(BossPartData data)
        {
            if (data == null)
            {
                throw new System.ArgumentNullException(nameof(data), "BossPartData is null.");
            }

            this.maxHp = data.maxHp;
            this.hp = data.maxHp;
            this.fireRate = data.fireRate;
            this.bulletName = data.bulletName;
            this._moveSpeed = data.moveSpeed;
            this._moveDistanceY = data.moveDistanceY;
            this._moveDistanceX = data.moveDistanceX;
        }

        /// <summary>
        /// ダメージを受け、HPを減らす。
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (hp <= 0) return;
            hp -= amount;
        }

        /// <summary>
        /// 移動完了後の初期座標を設定する。
        /// </summary>
        public void SetInitialPosition(Vector3 position)
        {
            _initialPosition = position;
        }

        /// <summary>
        /// 現在のフレームでの座標を計算して返す。
        /// </summary>
        /// <returns>計算後の座標。</returns>
        public Vector3 CalculateCurrentPosition()
        {
            // 上下にゆらゆら動く処理
            float yOffset = Mathf.Sin(Time.time * _moveSpeed) * _moveDistanceY;
            // 横にゆらゆら動く処理
            float xOffset = Mathf.Cos(Time.time * _moveSpeed) * _moveDistanceX;
            
            return _initialPosition + new Vector3(xOffset, yOffset, 0);
        }
    }
}