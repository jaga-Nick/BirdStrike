using System.Collections;
using System.Collections.Generic;
using InGame.Presenter;
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
        if (!running) { return; }

        if (homingDuration > 0)
        {
            homingDuration -= Time.deltaTime;
        }

        // 誘導が有効な場合のみ、ターゲットの方向を計算
        Vector3 finalDirection;
        if (target != null && homingDuration > 0)
        {

            Vector3 currentDirection = transform.rotation * Vector3.left;


            Vector3 targetDirection = (target.position - transform.position).normalized;


            float maxAngleDelta = maxTurnSpeed * Time.deltaTime;


            finalDirection = Vector3.RotateTowards(currentDirection, targetDirection, Mathf.Deg2Rad * maxAngleDelta, 0.0f);
        }
        else
        {
            // 誘導が切れたら、現在の向きのまま直進
            finalDirection = transform.rotation * Vector3.left;
        }
        
        transform.rotation = Quaternion.FromToRotation(Vector3.left, finalDirection);
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
            Explod(); // 爆発して消滅
        }
    }
}