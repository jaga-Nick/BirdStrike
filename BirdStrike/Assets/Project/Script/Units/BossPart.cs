using UnityEngine;

// Unitクラスを継承して、HPや死亡通知などの基本機能を利用します。
public class BossPart : Unit
{
    [Header("Part Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // 上下移動の速さ
    [SerializeField] private float moveDistance = 1f; // 上下移動の距離
    
    public Transform battery;

    private Vector3 initialPosition;
    
    private Unit target; // 攻撃ターゲット（プレイヤー）
    
    public override void OnStart()
    {
        // 初期位置を記憶
        initialPosition = transform.position;
        // hpやaniの初期化は親のUnitクラスがやってくれます。
        Fly();
    }
    
    public override void OnUpdate()
    {
        // 上下にゆらゆら動く処理（サインカーブを利用）
        float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = initialPosition + new Vector3(0, yOffset, 0);
        
        if (target != null)
        {
            Vector3 dir = (target.transform.position - battery.position).normalized;
            battery.transform.rotation = Quaternion.FromToRotation(Vector3.left, dir);
        }
        
        Fire();
    }
    
    /// <summary>
    /// ターゲット（プレイヤー）を設定する
    /// </summary>
    public void SetTarget(Unit target)
    {
        this.target = target;
    }

    /// <summary>
    /// プレイヤーを狙って弾を発射する
    /// </summary>
    public override void Fire()
    {
        // ターゲットがいない場合は攻撃しない
        if (target == null) return;

        // fireTimeは親クラスUnitで管理されているタイマー
        if (fireTime > 1f / fireRate)
        {
            GameObject go = Instantiate(bulletTemplate, firePoint.position, battery.rotation);
            Element bullent = go.GetComponent<Element>();
            bullent.direction = (target.transform.position - firePoint.position).normalized;
            fireTime = 0f;
        }
    }
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 無敵状態（復活直後など）や、すでに死んでいる場合は処理しない
        if (death) return;

        Element bullet = col.gameObject.GetComponent<Element>();
        if (bullet != null && bullet.side == SIDE.PLAYER)
        {
            Debug.Log(this.gameObject.name + " got hit! Current HP: " + this.hp);
            Damage(bullet.power);
        }
    }
    
}