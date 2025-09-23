using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Bullet
{
    public Transform target;
    private bool running = false;
    public GameObject fxExpold;
    
    private float homingDuration; // 誘導が有効な時間（秒）
    private float maxTurnSpeed;   // 1秒あたりの最大旋回角度（度）

    public void Initialize(BulletData data, SIDE ownerSide)
    {
        // 親クラス（Element/Bullet）のパラメータを設定
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

    public override void OnUpdate()
    {
        if (!running)
        {
            return;
        }

        if (homingDuration > 0)
        {
            homingDuration -= Time.deltaTime;
        }
        

        // 1. 現在の進行方向を、回転に基づいて正しく計算する
        //    （スプライトの正面が左向きなので、Vector3.leftを基準にする）
        Vector3 currentDirection = transform.rotation * Vector3.left;

        Vector3 finalDirection;

        // 2. 誘導が有効な場合、ターゲットへの方向を計算する
        if (target != null && homingDuration > 0)
        {
            Vector3 targetDirection = (target.position - transform.position).normalized;
            float maxRadiansDelta = maxTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            
            // 現在の進行方向からターゲットの方向へ、旋回性能の範囲で向きを変える
            finalDirection = Vector3.RotateTowards(currentDirection, targetDirection, maxRadiansDelta, 0.0f);
        }
        else
        {
            // 誘導が切れたら直進
            finalDirection = currentDirection;
        }

        // 3. 計算された新しい進行方向に、ミサイルの正面（左向き）が合うように回転を更新する
        transform.rotation = Quaternion.FromToRotation(Vector3.left, finalDirection);
        
        // 4. 新しい進行方向へ前進する
        transform.position += finalDirection * speed * Time.deltaTime;
    }

    public void Launch()
    {
        running = true;
    }

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
            Player p = col.GetComponent<Player>();
            if (p != null)
            {
                p.Damage(power);
            }
            Explod(); // 爆発して消滅
        }
    }
}