using System;
using System.Collections.Generic;
using System.Linq;
using InGame.Presenter;
using Common;

namespace InGame.Model
{
    public enum BossPhase { Normal, Angry, Serious }
    
    /// <summary>
    /// ボスのデータと状態を管理するクラス。
    /// </summary>
    public class BossModel
    {
        public float hp { get; private set; }
        public float maxHp { get; private set; }
        public float speed { get; private set; }
        public float moveSpeed { get; private set; }
        public float moveDistance { get; private set; }
        public BossPhase currentPhase { get; private set; }
        public bool isInvincible { get; private set; } = true;
        
        private readonly BossData _data;
        private readonly int _initialPartsCount;
        private readonly List<BossPartPresenter> _parts;

        /// <summary>
        /// JSONから読み込んだデータでモデルを初期化する。
        /// </summary>
        public BossModel(BossData data, List<BossPartPresenter> parts)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            _data = data;
            _parts = parts;
            _initialPartsCount = parts.Count;

            this.maxHp = data.maxHp;
            this.hp = data.maxHp;
            this.speed = data.moveSpeed;
            this.moveSpeed = data.moveSpeed;
            this.moveDistance = data.moveDistance;
            
            UpdatePhase();
        }

        /// <summary>
        /// ダメージを受け、HPを減らす。
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (isInvincible || hp <= 0) return;
            hp -= amount;
            AudioManager.Instance().PlaySe("Damage");
            GameEvents.OnBossHpUpdated?.Invoke(this.hp, this.maxHp);
        }
        
        /// <summary>
        /// 部位の破壊を検知し、状態を更新する。
        /// </summary>
        public void OnPartDestroyed(BossPartPresenter destroyedPart)
        {
            _parts.Remove(destroyedPart);
            UpdatePhase();
        }

        /// <summary>
        /// 残りの部位数に応じてフェーズを更新する。
        /// </summary>
        private void UpdatePhase()
        {
            int remainingParts = _parts.Count;
            BossPhase oldPhase = currentPhase;
            
            if (remainingParts == _initialPartsCount) currentPhase = BossPhase.Normal;
            else if (remainingParts > 0) currentPhase = BossPhase.Angry;
            else
            {
                currentPhase = BossPhase.Serious;
                isInvincible = false;
            }
            
            if (oldPhase != currentPhase)
            {
                UnityEngine.Debug.Log("Boss phase changed to: " + currentPhase);
            }
        }

        /// <summary>
        /// 現在のフェーズに対応する攻撃シーケンスを取得する。
        /// </summary>
        public List<AttackCommand> GetCurrentAttackSequence()
        {
            return _data.phases.FirstOrDefault(p => p.phaseName == currentPhase.ToString())?.attackSequence;
        }
    }
}