using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : Unit
{
    public float lifetime = 4f;
    public Vector2 range;
    public ENENY_TYPE enemyType;

    float initY = 0;

    public override void OnStart()
    {
        hp = MaxHP;
        Destroy(gameObject, lifetime);
        initY = Random.Range(range.x, range.y);
        this.transform.localPosition = new Vector3(0, initY, 0);
        Fly();
    }

    public override void OnUpdate()
    {
        float y = 0;

        if (enemyType == ENENY_TYPE.SWING_ENEMY)
        {
            y = Mathf.Sin(Time.timeSinceLevelLoad) * 3f;
        }

        transform.position = new Vector3(transform.position.x - Time.deltaTime * speed, initY + y);

        Fire();
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

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.name.Equals("ScoreArea"))
        {
            if (OnScore != null)
            {
                OnScore(1);
            }
        }
    }
}
