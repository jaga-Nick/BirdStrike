using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Element : MonoBehaviour
{
    public float speed;

    public Vector3 direction = Vector3.zero;

    public SIDE side;

    public float power = 1;

    public float lifeTime;
    
    public int durability = 1;
    
    
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        transform.position += speed * Time.deltaTime * direction;

        if (!GameUtil.Instance.InScreen(transform.position))
        {
            Destroy(gameObject, 1f);
        }
    }
    
    /// <summary>
    /// ダメージを受け、耐久力を減少させる
    /// </summary>
    public void TakeDamage(int damage)
    {
        durability -= damage;
        if (durability <= 0)
        {
            // TODO: ここに消滅時のエフェクト再生処理などを追加しても良い
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 相手が弾（またはミサイル）の場合の相殺処理
        Element otherElement = col.gameObject.GetComponent<Element>();
        if (otherElement != null)
        {
            // 敵と味方の弾同士の場合のみ相殺処理を行う
            if (this.side != otherElement.side && this.side != SIDE.NONE && otherElement.side != SIDE.NONE)
            {
                this.TakeDamage(1);
                otherElement.TakeDamage(1);
            }
        }
    }
}
