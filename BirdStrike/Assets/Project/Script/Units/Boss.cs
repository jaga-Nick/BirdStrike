using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Boss : Enemy
{
    public enum BossPhase { Normal, Angry, Serious }

    [Header("Boss Parts")]
    public List<BossPart> parts;
    private int initialPartsCount;
    private BossPhase currentPhase;
    private bool isBodyInvincible = true;
    
    private BossData _bossData;
    
    [Header("Boss Movement")]
    public float moveSpeed = 1f;
    public float moveDistance = 0.5f;
    private bool entryComplete = false;
    private bool isSpecialMoving = false; // UltraAttackなどで移動中か
    private Vector3 initialPosition;

    public GameObject missileTemplate;
    public Transform firePoint2;
    public Transform firePoint3;
    public Transform battery;
    public Unit target;
    public float fireRate2 = 10f;
    float fireTimer2 = 0;
    public float UltCD = 10f;
    float fireTimer3 = 0;
    Missile missile = null;

    [Header("Radial Shot Settings")]
    public int radialShotCount = 5;
    public float radialShotSpreadAngle = 90f;

    public override void OnStart()
    {
        Fly();
        
    }
    
    public void Initialize(BossData data, List<BossPart> spawnedParts, Player player)
    {
        // 自身のデータを保持
        _bossData = data;

        // 自身のパラメータをデータから設定
        this.hp = data.maxHp;
        this.moveSpeed = data.moveSpeed;
        this.moveDistance = data.moveDistance;
        
        // 部位とターゲットを設定
        this.target = player;
        this.parts = spawnedParts;
        initialPartsCount = parts.Count;
        
        Debug.Log("Boss initialized with HP: " + this.MaxHP);
        UpdatePhase();

        // 部位のイベント購読
        foreach (var part in parts)
        {
            if (part != null)
            {
                part.SetTarget(player);
                part.OnDeathAsObservable.Subscribe(OnPartDestroyed).AddTo(this.disposables);
            }
        }
        
        // 入場シーケンスを開始
        EnterAsync(data.entryTargetPosition, data.parts.Select(p => p.entryTargetPosition).ToList()).Forget();
    }

    private void OnPartDestroyed(Unit sender)
    {
        Debug.Log(sender.gameObject.name + " destroyed! Boss received death notification.");

        var destroyedPart = sender as BossPart;
        if (destroyedPart != null)
        {
            // OnDeathAsObservableは一度しか呼ばれないので、安全に直接リストから削除できる
            parts.Remove(destroyedPart);
        }
        UpdatePhase();
    }

    private void UpdatePhase()
    {
        int remainingParts = parts.Count(part => part != null);
        BossPhase oldPhase = currentPhase;
        
        if (remainingParts == initialPartsCount)
        {
            currentPhase = BossPhase.Normal;
        }
        else if (remainingParts > 0)
        {
            currentPhase = BossPhase.Angry;
            Debug.Log("BOSS is ANGRY!");
        }
        else
        {
            currentPhase = BossPhase.Serious;
            isBodyInvincible = false;
            Debug.Log("BOSS is SERIOUS! Body is now vulnerable!");
        }
        
        if (oldPhase != currentPhase)
        {
            Debug.Log("Boss phase changed to: " + currentPhase + ". Remaining parts: " + remainingParts);
            if (!isBodyInvincible)
            {
                Debug.Log("Boss body is now vulnerable!");
            }
        }
    }

    async UniTaskVoid EnterAsync(Vector3 bodyTargetPos, List<Vector3> partTargetPositions)
    {
        var token = this.GetCancellationTokenOnDestroy();
        
        // ボス本体と全部位の移動タスクをリスト化
        List<UniTask> moveTasks = new List<UniTask>();

        // ボス本体の移動タスクを追加
        moveTasks.Add(MoveToAsync(bodyTargetPos, speed, token));

        // 各部位の移動タスクを追加
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i] != null)
            {
                moveTasks.Add(parts[i].MoveToAsync(partTargetPositions[i], speed, token));
            }
        }

        // 全ての移動タスクが完了するまで待機
        await UniTask.WhenAll(moveTasks);
        
        // 移動完了後に初期座標を確定
        this.initialPosition = transform.position;
        foreach (var part in parts)
        {
            if (part != null)
            {
                part.SetInitialPosition();
            }
        }
        
        entryComplete = true;
        AttackAsync().Forget(); // 攻撃開始
    }

    async UniTaskVoid AttackAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!death && this.gameObject.activeSelf)
        {
            PhaseData currentPhaseData = _bossData.phases.FirstOrDefault(p => p.phaseName == currentPhase.ToString());

            if (currentPhaseData != null)
            {
                // 行動シーケンスを順番に実行
                foreach (var command in currentPhaseData.attackSequence)
                {
                    if (death) break;
                    await ExecuteCommand(command, token);
                }
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
    
    private async UniTask ExecuteCommand(AttackCommand command, CancellationToken token)
    {
        switch (command.commandType)
        {
            case "WAIT":
                await UniTask.Delay(System.TimeSpan.FromSeconds(command.duration), cancellationToken: token);
                break;
            case "SHOOT_RADIAL":
                // データを元に放射状弾のパラメータを一時的に上書き
                this.radialShotCount = command.count;
                this.radialShotSpreadAngle = command.spreadAngle;
                Fire(); // 1回だけ発射
                break;
            case "SHOOT_TARGET":
                // データを元に連射
                for (int i = 0; i < command.count; i++)
                {
                    if (death) break;
                    Fire2();
                    await UniTask.Delay(System.TimeSpan.FromSeconds(command.interval), cancellationToken: token);
                }
                break;
            case "ULTRA_ATTACK":
                await UltraAttackAsync(token);
                break;
        }
    }

    public override void Fire()
    {
        if (fireTime > 1f / fireRate)
        {
            if (radialShotCount <= 1)
            {
                base.Fire();
                return;
            }
            float angleStep = radialShotSpreadAngle / (radialShotCount - 1);
            float startAngle = -radialShotSpreadAngle / 2;
            for (int i = 0; i < radialShotCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                Vector3 shotDirection = rotation * Vector3.left;
                GameObject go = Instantiate(bulletTemplate);
                go.transform.position = firePoint.position;
                go.transform.rotation = Quaternion.FromToRotation(Vector3.left, shotDirection);
                Element bullet = go.GetComponent<Element>();
                bullet.direction = shotDirection.normalized;
                bullet.side = this.side;
            }
            fireTime = 0f;
        }
    }

    async UniTask UltraAttackAsync(CancellationToken token)
    {
        isSpecialMoving = true;
        await MoveToAsync(new Vector3(5, 4, 0), speed, token);
        await FireMissileAsync(token);
        await MoveToAsync(new Vector3(5, 0, 0), speed, token);
        this.initialPosition = transform.position;
        isSpecialMoving = false;
    }

    async UniTask MoveToAsync(Vector3 pos, float moveSpeed, CancellationToken token)
    {
        while (Vector3.Distance(transform.position, pos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        transform.position = pos;
    }

    async UniTask FireMissileAsync(CancellationToken token)
    {
        ani.SetTrigger("Skill");
        await UniTask.Delay(System.TimeSpan.FromSeconds(3), cancellationToken: token);
    }

    public void Fire2()
    {
        fireTimer2 += Time.deltaTime;
        if (fireTimer2 > 1f / fireRate2)
        {
            GameObject go = Instantiate(bulletTemplate, firePoint2.position, battery.rotation);
            Element bullent = go.GetComponent<Element>();
            bullent.direction = (target.transform.position - firePoint2.position).normalized;
            fireTimer2 = 0f;
        }
    }

    public void OnMissileLoad()
    {
        GameObject go = Instantiate(missileTemplate, firePoint3);
        missile = go.GetComponent<Missile>();
        missile.target = target.transform;
    }

    public void OnMissileLaunch()
    {
        if (missile == null) return;
        missile.transform.SetParent(null);
        missile.Launch();
    }

    public override void OnUpdate()
    {
        if (target != null)
        {
            Vector3 dir = (target.transform.position - battery.position).normalized;
            battery.transform.rotation = Quaternion.FromToRotation(Vector3.left, dir);
        }

        if (entryComplete && !isSpecialMoving)
        {
            float yOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
            transform.position = initialPosition + new Vector3(0, yOffset, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Element bullet = col.gameObject.GetComponent<Element>();
        if (bullet == null) return;
        if (bullet.side == SIDE.PLAYER)
        {
            if (!isBodyInvincible)
            {
                Damage(bullet.power);
            }
            else
            {
                Debug.Log("Boss body is invincible! Destroy the parts!");
            }
        }
    }
}