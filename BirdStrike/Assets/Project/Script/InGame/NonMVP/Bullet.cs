using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using InGame.Presenter;

/// <summary>
/// 弾のクラス
/// </summary>
public class Bullet : MonoBehaviour
{
    public string bulletName;
    public float speed;
    public Vector3 direction = Vector3.zero;
    public SIDE side;
    public float power;
    public float lifeTime;
    public int durability;
    public int scoreValue;
    private CancellationTokenSource _lifeTimeCts;

    /// <summary>
    /// OnEnableは、オブジェクトがアクティブになるたびに呼ばれます
    /// </summary>
    void OnEnable()
    {
        // 寿命タイマーを開始
        StartLifeTimeCountDown().Forget();
    }

    /// <summary>
    /// OnDisableは、オブジェクトが非アクティブになるたびに呼ばれます
    /// </summary>
    private void OnDisable()
    {
        // タイマーをキャンセルして、リソースリークを防ぎます
        _lifeTimeCts?.Cancel();
        _lifeTimeCts?.Dispose();
        _lifeTimeCts = null;
    }
    
    /// <summary>
    /// JSONから読み込んだデータで初期化する。
    /// </summary>
    public void Initialize(BulletData data, SIDE ownerSide)
    {
        this.bulletName = data.bulletName;
        this.power = data.power;
        this.durability = data.durability;
        this.scoreValue = data.scoreValue;
        this.speed = data.speed;
        this.side = ownerSide;
        this.lifeTime = 5f;
    }
    
    private void Update()
    {
        OnUpdate();
    }
    
    protected virtual void OnUpdate()
    {
        transform.position += speed * Time.deltaTime * direction;
        if (!GameUtil.Instance.InScreen(transform.position))
        {
            BulletManager.Instance.ReturnBullet(this.gameObject, this.bulletName, this.side);
        }
    }

    /// <summary>
    /// 弾の寿命計測
    /// </summary>
    private async UniTaskVoid StartLifeTimeCountDown()
    {
        _lifeTimeCts = new CancellationTokenSource();
        
        // lifeTime秒待機します。オブジェクトが非アクティブになったら自動で中断されます。
        bool cancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTime), ignoreTimeScale: false, cancellationToken: _lifeTimeCts.Token).SuppressCancellationThrow();

        // Canceledでなければ（つまり寿命が尽きたら）プールに戻す
        if (!cancelled)
        {
            BulletManager.Instance.ReturnBullet(this.gameObject, this.bulletName, this.side);
        }
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int damage)
    {
        durability -= damage;
        if (durability <= 0)
        {
            BulletManager.Instance.ReturnBullet(this.gameObject, this.bulletName, this.side);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        Bullet otherBullet = col.gameObject.GetComponent<Bullet>();
        if (otherBullet != null)
        {
            if (this.side != otherBullet.side && this.side != SIDE.NONE && otherBullet.side != SIDE.NONE)
            {
                this.TakeDamage(1);
                otherBullet.TakeDamage(1);
            }
        }
        
        if (this.side == SIDE.PLAYER)
        {
            // 衝突相手がEnemyタグを持っているか、BossPartコンポーネントを持っているか
            if (col.CompareTag("Enemy") || col.GetComponent<BossPresenter>() != null || col.GetComponent<BossPartPresenter>() != null)
            {
                // 自分自身（弾）をプールに戻す
                BulletManager.Instance.ReturnBullet(this.gameObject, this.bulletName, this.side);
            }
        }
    }
}
