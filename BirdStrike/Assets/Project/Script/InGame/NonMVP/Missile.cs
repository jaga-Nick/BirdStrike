using System.Collections;
using System.Collections.Generic;
using InGame.Presenter;
using UnityEngine;


/// <summary>
/// ミサイルの動きを管理するクラス
/// </summary>
public class Missile : Bullet
{
    public Transform target;
    private bool running = false;
    public GameObject fxExpold;
    
    private float homingDuration; // 誘導が有効な時間（秒）
    private float maxTurnSpeed;   // 1秒あたりの最大旋回角度（度）

    /// <summary>
    /// JSONから読み込んだデータで初期化する。
    /// </summary>
    public void Initialize(BulletData data, SIDE ownerSide)
    {
        this.power = data.power;
        this.durability = data.durability;
        this.scoreValue = data.scoreValue;
        this.speed = data.speed;
        this.side = ownerSide;
        this.lifeTime = 35f; // ミサイルは長めに

        // ミサイル固有のパラメータを設定
        this.homingDuration = data.homingDuration;
        this.maxTurnSpeed = data.maxTurnSpeed;
    }

    protected override void OnUpdate()
    {
        if (!running) { return; }

        Homing();
    }

    /// <summary>
    /// ミサイルの誘導
    /// </summary>
    private void Homing()
    {
        if (homingDuration > 0)
        {
            homingDuration -= Time.deltaTime;
        }

        // 誘導が有効な場合のみ、ターゲットの方向を計算
        Vector3 finalDirection;
        if (target != null && homingDuration > 0)
        {
            // ターゲットの方向を向くための「理想の回転」を計算
            Vector3 targetDirection = target.position - transform.position;
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.left, targetDirection);

            // 1フレームあたりに回転できる最大角度を計算
            float maxAngleDelta = maxTurnSpeed * Time.deltaTime;

            // 現在の回転から、理想の回転へ、最大角度の範囲で滑らかに回転
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxAngleDelta);
        }

        // 常に、現在のミサイルの正面（左向き）へ前進
        transform.position += transform.rotation * Vector3.left * speed * Time.deltaTime;
    }

    /// <summary>
    /// ミサイルアニメーション中
    /// </summary>
    public void Launch()
    {
        running = true;
    }

    /// <summary>
    /// 爆発エフェクトを生成
    /// </summary>
    public void Explod()
    {
        if (fxExpold != null)
        {
            Instantiate(fxExpold, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    
    private new void OnTriggerEnter2D(Collider2D col)
    {
        // 親クラス（Element）の相殺処理を先に呼び出す
        base.OnTriggerEnter2D(col);

        // 衝突相手がプレイヤーだった場合
        if (col.CompareTag("Player"))
        {
            Explod(); // 爆発して消滅
        }
    }
}