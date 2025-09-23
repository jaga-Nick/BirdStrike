using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BossPart : Unit
{
    [Header("Part Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDistance = 1f;
    

    private Vector3 initialPosition;
    private Unit target;
    private bool entryComplete = false;

    // OnStartは空にして、UnitのStart()からの意図しない処理を防ぐ
    public override void OnStart() { }

    public override void OnUpdate()
    {

        if (!entryComplete) { return; }


        // 上下にゆらゆら動く処理
        float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = initialPosition + new Vector3(0, yOffset, 0);

        // プレイヤーを狙って攻撃する
        Fire();
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(BossPartData data)
    {
        this.MaxHP = data.maxHp;
        this.hp = data.maxHp; // Awakeより後に呼ばれるため、hpも上書き
        this.fireRate = data.fireRate;
        this.moveSpeed = data.moveSpeed;
        this.moveDistance = data.moveDistance;

        Debug.Log(this.gameObject.name + " initialized with HP: " + this.MaxHP);
    }
    
    /// <summary>
    /// 指定された位置へ移動する（ボスから呼ばれる）
    /// </summary>
    public async UniTask MoveToAsync(Vector3 targetPosition, float speed, CancellationToken token)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            if (token.IsCancellationRequested) return;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        transform.position = targetPosition;
    }
    
    /// <summary>
    /// 移動完了後に初期位置を確定させ、通常行動を開始する
    /// </summary>
    public void SetInitialPosition()
    {
        this.initialPosition = transform.position;
        this.entryComplete = true;

        Fly(); 

    }
    
    public void SetTarget(Unit target)
    {
        this.target = target;
    }

    public override void Fire()
    {
        if (target == null) return;

        if (fireTime > 1f / fireRate)
        {
            GameObject go = Instantiate(bulletTemplate, firePoint.position, Quaternion.identity);
            Element bullet = go.GetComponent<Element>();
            
            bullet.direction = (target.transform.position - firePoint.position).normalized;
            bullet.side = this.side;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.left, bullet.direction);
            
            fireTime = 0f;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (death) return;

        Element bullet = col.gameObject.GetComponent<Element>();
        if (bullet != null && bullet.side == SIDE.PLAYER)
        {
            Debug.Log(this.gameObject.name + " got hit! Current HP: " + this.hp);
            Damage(bullet.power);
        }
    }
}