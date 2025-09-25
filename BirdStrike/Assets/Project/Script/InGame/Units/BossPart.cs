using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BossPart : Unit
{
    private float moveSpeed;
    private float moveDistanceY;
    private float moveDistanceX;

    private Vector3 initialPosition;
    private Unit target;
    private bool entryComplete = false;

    public override void OnStart() { }

    public override void OnUpdate()
    {
        if (!entryComplete) { return; }

        float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistanceY;
        // 横にゆらゆら動く処理（Cosを使うと円や楕円のような動きになる）
        float xOffset = Mathf.Cos(Time.time * moveSpeed) * moveDistanceX;
        
        
        transform.position = initialPosition + new Vector3(xOffset, yOffset, 0);

        Fire();
    }
    
    public void Initialize(BossPartData data)
    {
        this.MaxHP = data.maxHp;
        this.hp = data.maxHp;
        this.fireRate = data.fireRate;
        this.moveSpeed = data.moveSpeed;
        this.moveDistanceY = data.moveDistanceY;
        this.moveDistanceX = data.moveDistanceX;
        this.bulletName = data.bulletName;
    }
    
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
            GameObject go = BulletManager.Instance.GetBullet(this.bulletName);
            if(go == null) return;

            var bulletData = DataManager.Instance.GetBulletData(this.bulletName);
            var bulletComp = go.GetComponent<Bullet>();

            bulletComp.Initialize(bulletData, this.side);
            go.transform.position = firePoint.position;
            bulletComp.direction = (target.transform.position - firePoint.position).normalized;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.left, bulletComp.direction);
            
            fireTime = 0f;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (death) return;

        Bullet bullet = col.gameObject.GetComponent<Bullet>();
        if (bullet != null && bullet.side == SIDE.PLAYER)
        {
            Damage(bullet.power);
        }
    }
}