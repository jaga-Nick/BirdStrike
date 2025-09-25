using System;

namespace InGame.Model
{
    /// <summary>
    /// プレイヤーのデータと状態を管理するクラス。
    /// </summary>
    public class PlayerModel
    {

        public float hp { get; private set; }
        public float maxHp { get; private set; }
        public int life { get; private set; }
        public float speed { get; private set; }
        public float fireRate { get; private set; }
        public float invincibleTime { get; private set; }// 復活後の無敵時間。
        public float slowMagnification { get; private set; }// 低速移動時の速度倍率。
        public int bombCount { get; private set; }// ボムの残り回数。
        public string bulletName { get; private set; }
        public UnityEngine.Vector3 spawnPosition { get; private set; }

        /// <summary>
        /// JSONから読み込んだデータでモデルを初期化する。
        /// </summary>
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
            
            GameEvents.OnLifeUpdated?.Invoke(this.life);
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
            GameEvents.OnLifeUpdated?.Invoke(this.life);
        }

        /// <summary>
        /// ボムを使用し、カウントを1減らす。
        /// </summary>
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