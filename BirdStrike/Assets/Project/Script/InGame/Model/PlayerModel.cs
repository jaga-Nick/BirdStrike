using System;

namespace InGame.Model
{
    /// <summary>
    /// プレイヤーのデータと状態を管理するピュアC#クラス。
    /// </summary>
    public class PlayerModel
    {
        /// <summary>
        /// 現在のHP。
        /// </summary>
        public float hp { get; private set; }

        /// <summary>
        /// HPの最大値。
        /// </summary>
        public float maxHp { get; private set; }

        /// <summary>
        /// 現在の残機。
        /// </summary>
        public int life { get; private set; }

        /// <summary>
        /// 移動速度。
        /// </summary>
        public float speed { get; private set; }

        /// <summary>
        /// 攻撃の連射速度。
        /// </summary>
        public float fireRate { get; private set; }

        /// <summary>
        /// 復活後の無敵時間。
        /// </summary>
        public float invincibleTime { get; private set; }

        /// <summary>
        /// 低速移動時の速度倍率。
        /// </summary>
        public float slowMagnification { get; private set; }
        
        /// <summary>
        /// ボムの残り回数。
        /// </summary>
        public int bombCount { get; private set; }

        /// <summary>
        /// プレイヤーが撃つ弾の名前。
        /// </summary>
        public string bulletName { get; private set; }

        /// <summary>
        /// プレイヤーの初期位置および復活位置。
        /// </summary>
        public UnityEngine.Vector3 spawnPosition { get; private set; }

        /// <summary>
        /// JSONから読み込んだデータでモデルを初期化する。
        /// </summary>
        /// <param name="data">プレイヤーのパラメータが格納されたデータオブジェクト。</param>
        public PlayerModel(PlayerData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "PlayerData is null.");
            }

            this.speed = data.speed;
            this.fireRate = data.fireRate;
            this.life = data.life;
            this.invincibleTime = data.invincibleTime;
            this.slowMagnification = data.slowMagnification;
            this.bombCount = data.bombCount;
            this.bulletName = data.bulletName;
            this.spawnPosition = data.spawnPosition;
            
            this.maxHp = 1f;
            this.hp = this.maxHp;
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
        /// 死亡処理。残機を1減らす。
        /// </summary>
        public void Die()
        {
            if (life <= 0) return;
            life--;
        }

        /// <summary>
        /// ボムを使用し、カウントを1減らす。
        /// </summary>
        /// <returns>ボムが使用可能であればtrue。</returns>
        public bool TryUseBomb()
        {
            if (bombCount > 0)
            {
                bombCount--;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 復活時にHPをリセットする。
        /// </summary>
        public void ResetHp()
        {
            hp = maxHp;
        }
    }
}