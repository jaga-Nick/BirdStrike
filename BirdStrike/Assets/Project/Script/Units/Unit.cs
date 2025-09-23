using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using System;

public class Unit : MonoBehaviour, IDisposable
{
    public SIDE side;
    public int life = 3;
    public Rigidbody2D rigidbodyBird;
    public Animator ani;
    protected bool death = false;
    public float speed = 5f;
    public float fireRate = 10f;

    private readonly Subject<Unit> _onDeathSubject = new Subject<Unit>();
    public IObservable<Unit> OnDeathAsObservable => _onDeathSubject;

    public UnityAction<int> OnScore;
    public GameObject bulletTemplate;
    public Transform firePoint;
    protected Vector3 initPos;
    protected bool isFlying = false;

    protected float hp;
    public float MaxHP = 10f;
    public float HP => this.hp;

    public float Attack;
    protected float fireTime = 0;
    
    public bool destroyOnDeath = false;

    protected CompositeDisposable disposables = new CompositeDisposable();
    
    void Awake()
    {
        hp = MaxHP;
        ani = GetComponent<Animator>();
        initPos = transform.position;
    }


    void Start()
    {
        Idle();
        OnStart();
    }

    public virtual void OnStart() { }

    void Update()
    {
        if (death) return;
        if (!isFlying) return;
        fireTime += Time.deltaTime;
        OnUpdate();
    }

    public virtual void OnUpdate() { }

    public void Init()
    {
        transform.position = initPos;
        Idle();
        death = false;
        hp = MaxHP;
    }

    public virtual void Fire()
    {
        if (fireTime > 1f / fireRate)
        {
            GameObject go = Instantiate(bulletTemplate);
            go.transform.position = firePoint.position;
            go.GetComponent<Element>().direction = side == SIDE.PLAYER ? Vector3.right : Vector3.left;
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
        if (death) return;

        life--;
        hp = 0;
        death = true;
        ani.SetTrigger("Die");
        _onDeathSubject.OnNext(this);
        
        if (destroyOnDeath)
            Destroy(gameObject, 0.2f);
    }

    public void Damage(float power)
    {
        hp -= power;
        if (HP <= 0)
        {
            Die();
        }
    }

    public void AddHP(int hp)
    {
        this.hp += hp;
        if (this.hp > MaxHP)
        {
            this.hp = MaxHP;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        disposables.Dispose();
        _onDeathSubject.Dispose();
    }
}