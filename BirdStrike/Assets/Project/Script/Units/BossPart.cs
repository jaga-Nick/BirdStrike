using UnityEngine;

// Unitクラスを継承して、HPや死亡通知などの基本機能を利用します。
public class BossPart : Unit
{
    [Header("Part Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // 上下移動の速さ
    [SerializeField] private float moveDistance = 1f; // 上下移動の距離

    private Vector3 initialPosition;

    // OnStartはUnitクラスから自動で呼ばれます。
    public override void OnStart()
    {
        // 初期位置を記憶
        initialPosition = transform.position;
        // hpやaniの初期化は親のUnitクラスがやってくれます。
        Fly();
    }

    // OnUpdateもUnitクラスから自動で呼ばれます。
    public override void OnUpdate()
    {
        // 上下にゆらゆら動く処理（サインカーブを利用）
        float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = initialPosition + new Vector3(0, yOffset, 0);
    }
    
    // プレイヤーの弾との当たり判定
    private void OnTriggerEnter2D(Collider2D col)
    {
        Element bullet = col.gameObject.GetComponent<Element>();
        if (bullet != null && bullet.side == SIDE.PLAYER)
        {
            // UnitクラスのDamageメソッドを呼び出してHPを減らす
            Damage(bullet.power);
        }
    }
}