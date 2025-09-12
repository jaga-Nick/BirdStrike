using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public SIDE side;

    public int life = 3;

    public Rigidbody2D rigidbodyBird;

    public Animator ani;

    protected bool death = false;

    public float speed = 5f;
    public float fireRate = 10f;

    public delegate void DeathNotify(Unit sender);
    public event DeathNotify OnDeath;

    public UnityAction<int> OnScore;

    public GameObject bulletTemplate;
    public Transform firePoint;

    protected Vector3 initPos;

    protected bool isFlying = false;

    public float hp = 10f;

    public float HP
    {
        get { return this.hp; }
    }
    public float MaxHP = 10f;

    public float Attack;

    protected float fireTime = 0;

    public bool desoryOnDeath = false;


    void Start()
    {
        ani = GetComponent<Animator>();
        Idle();
        initPos = transform.position;
        OnStart();
    }

    public virtual void OnStart()
    {

    }

    void Update()
    {
        if (death) return;
        if (!isFlying) return;
        fireTime += Time.deltaTime;

        OnUpdate();
    }

    public virtual void OnUpdate()
    {

    }

    public void Init()
    {
        transform.position = initPos;
        Idle();
        death = false;
        hp = MaxHP;
    }

    public void Fire()
    {
        if (fireTime > 1f / fireRate)
        {
            GameObject go = Instantiate(bulletTemplate);
            go.transform.position = firePoint.position;
            go.GetComponent<Element>().direction = side == SIDE.PLAYER ? Vector3.right: Vector3.left;
            fireTime = 0f;
        }

    }

    public void Idle()
    {
        rigidbodyBird.simulated = false;
        ani.SetTrigger("Idle");
        isFlying = false;
    }

    public void Fly()
    {
        rigidbodyBird.simulated = true;

        ani.SetTrigger("Fly");
        isFlying = true;
    }

    public void Die()
    {
        if(death) return;

        life--;
        hp = 0;
        death = true;
        ani.SetTrigger("Die");
        if (OnDeath != null)
        {
            OnDeath(this);
        }
        if(desoryOnDeath)
            Destroy(gameObject,0.2f);
    }

    public void Damage(float power)
    {
        hp -= power;

        if(HP<=0)
        {
            Die();
        }
    }

    public void AddHP(int hp)
    {
        this.hp += hp;
        if(this.hp > MaxHP)
        {
            this.hp = MaxHP;
        }
    }
}
