using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx; // UniRxの名前空間を追加

public class Boss : Enemy
{
    public enum BossPhase { Normal, Angry, Serious }

    [Header("Boss Parts")]
    public List<BossPart> parts;
    private int initialPartsCount;
    private BossPhase currentPhase;
    private bool isBodyInvincible = true;

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
        StartCoroutine(Enter());
    }
    
    public void InitializeParts(List<BossPart> spawnedParts, Player player)
    {
        this.target = player;
        this.parts = spawnedParts;
        initialPartsCount = parts.Count;
        
        // ログを追加して、初期状態を明確にする
        Debug.Log("Boss initialized with " + initialPartsCount + " parts.");
        
        UpdatePhase();

        // 各部位のOnDeathAsObservableをUniRxで購読する
        foreach (var part in parts)
        {
            if (part != null)
            {
                part.SetTarget(player);
                part.OnDeathAsObservable
                    .Subscribe(OnPartDestroyed)
                    .AddTo(this.disposables); // 親クラスのdisposablesを利用
            }
        }
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

    IEnumerator Enter()
    {
        transform.position = new Vector3(15, 1.4f, 0);
        yield return MoveTo(new Vector3(5, 1.4f, 0));
        yield return Attack();
    }

    new IEnumerator Attack()
    {
        while (true)
        {
            
            switch (currentPhase)
            {
                case BossPhase.Normal:
                    Fire();

                    break;
                case BossPhase.Angry:
                    Fire2();
                    break;
                case BossPhase.Serious:
                    fireTimer3 += Time.deltaTime;
                    if (fireTimer3 >= UltCD)
                    {
                        yield return UltraAttack();
                        fireTimer3 = 0;
                    }
                    break;
            }
            yield return null;
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

    IEnumerator UltraAttack()
    {
        yield return MoveTo(new Vector3(5, 4, 0));
        yield return FireMissile();
        yield return MoveTo(new Vector3(5, 0, 0));
    }

    IEnumerator MoveTo(Vector3 pos)
    {
        while (true)
        {
            Vector3 dir = (pos - transform.position);
            if (dir.magnitude < 0.1) break;
            transform.position += dir.normalized * speed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FireMissile()
    {
        ani.SetTrigger("Skill");
        yield return new WaitForSeconds(3f);
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