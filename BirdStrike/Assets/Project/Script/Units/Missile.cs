using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Element
{
    public Transform target;

    private bool running = false;

    public GameObject fxExpold;
    
    private float homingDuration = 30f; // 誘導が有効な時間（秒）
    private float maxTurnSpeed = 30f;   // 1秒あたりの最大旋回角度（度）
    
    private void Awake()
    {
        // ミサイルの初期耐久力を設定
        this.durability = 3;
    }
    
    public override void OnUpdate()
    {
        if (!running) { return; }
        // 誘導タイマーを減算
        if (homingDuration > 0)
        {
            homingDuration -= Time.deltaTime;
        }
        
        // 誘導が有効、かつターゲットが存在する場合のみ追尾する
        if (target != null && homingDuration > 0)
        {
            // ターゲットへの方向ベクトルを計算
            Vector3 targetDirection = (target.position - transform.position).normalized;

            // 現在の進行方向（左向きを基準とする）
            Vector3 currentDirection = transform.rotation * Vector3.left;

            // 1フレームあたりに回転できる最大角度（ラジアン）
            float maxRadiansDelta = maxTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;

            // 最大旋回角度の範囲で、ターゲットの方向へ滑らかに向きを変える
            Vector3 newDirection = Vector3.RotateTowards(currentDirection, targetDirection, maxRadiansDelta, 0.0f);
            
            // 新しい向きをQuaternionとして設定
            transform.rotation = Quaternion.FromToRotation(Vector3.left, newDirection);
            
            // 新しい向きへ前進
            transform.position += newDirection * speed * Time.deltaTime;
            
            
            // --- ▼▼▼ デバッグ用Rayの追加 ▼▼▼ ---
            // ターゲットへの理想的な方向を緑色で表示
            Debug.DrawRay(transform.position, targetDirection * 5f, Color.green);

            // 旋回限界角度を黄色で表示
            Quaternion leftLimit = Quaternion.AngleAxis(-maxTurnSpeed / 2, Vector3.forward);
            Quaternion rightLimit = Quaternion.AngleAxis(maxTurnSpeed / 2, Vector3.forward);
            Debug.DrawRay(transform.position, (transform.rotation * leftLimit) * Vector3.left * 3f, Color.yellow);
            Debug.DrawRay(transform.position, (transform.rotation * rightLimit) * Vector3.left * 3f, Color.yellow);
            // --- ▲▲▲ デバッグ用Rayの追加 ▲▲▲ ---
        }
        else
        {
            // 誘導が切れたら、現在の向きのまま直進する
            Vector3 currentDirection = transform.rotation * Vector3.left;
            transform.position += currentDirection * speed * Time.deltaTime;
        }
        
        // ターゲットとの距離が近ければ爆発
        if(target != null && Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            Explod();
        }

    }

    public void Launch()
    {
        running = true;
    }

    public void Explod()
    {
        Instantiate(fxExpold, transform.position, Quaternion.identity);
        Destroy(gameObject);

        if(target != null)
        {
            Player p = target.GetComponent<Player>();
            p.Damage(power);
        }
    }
}
