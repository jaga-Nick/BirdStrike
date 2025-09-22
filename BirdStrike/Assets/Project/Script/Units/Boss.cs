using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boss : Enemy
{
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
    
    public int radialShotCount = 5; // 放射状に発射する弾の数
    public float radialShotSpreadAngle = 90f; // 弾が広がる全体の角度

    public override void OnStart()
    {
        Fly();
        StartCoroutine(Enter());
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
            fireTimer2 += Time.deltaTime;
            Fire();
            Fire2();

            fireTimer3 += Time.deltaTime;
            if (fireTimer3 >= UltCD)
            {
                
                yield return UltraAttack();
                fireTimer3 = 0;
            }

            yield return null;
        }

        
    }
    
    public override void Fire()
    {
        if (fireTime > 1f / fireRate)
        {
            // 弾が1発だけの場合は中央に発射
            if (radialShotCount <= 1)
            {
                base.Fire(); // 元の直進弾を撃つ処理を呼ぶ
                return;
            }

            // 弾を発射する角度のステップを計算
            float angleStep = radialShotSpreadAngle / (radialShotCount - 1);
            float startAngle = -radialShotSpreadAngle / 2;

            // 指定された数だけ弾を生成
            for (int i = 0; i < radialShotCount; i++)
            {
                // 現在の弾の角度を計算
                float currentAngle = startAngle + (angleStep * i);
                
                // 角度を回転（Quaternion）に変換
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                
                // 左方向のベクトルを回転させて、弾の発射方向を決定
                Vector3 shotDirection = rotation * Vector3.left;

                // 弾を生成し、パラメータを設定
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
            if (dir.magnitude < 0.1)
            {
                break;
            }
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
        if(missile == null)
            return;
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
        if (bullet == null)
        {
            return;
        }

        if (bullet.side == SIDE.PLAYER)
        {
            Damage(bullet.power);
        }
    }
}
