using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Common;
using UniRx;
using UnityEngine.InputSystem;

public class Player : Unit
{
    private float invincibleTime;
    private float _slowMagnification;
    private float timer = 0;

    Vector2 worldPosLeftBottom;
    Vector2 worldPosTopRight;
    
    private InputSystem_Actions _actionMap;
    
    public IReadOnlyReactiveProperty<int> BombCount => _bombCount;
    private readonly ReactiveProperty<int> _bombCount = new ReactiveProperty<int>(3);

    private void Awake()
    {
        InitInput();
    }

    private void Start()
    {
        worldPosLeftBottom = Camera.main.ViewportToWorldPoint(Vector2.zero);
        worldPosTopRight = Camera.main.ViewportToWorldPoint(Vector2.one);
    }

    public override void OnUpdate()
    {
        if (death)
            return;

        timer += Time.deltaTime;
        
        Vector2 moveInput = _actionMap.Player.Move.ReadValue<Vector2>();
        float currentSpeed = speed * (_actionMap.Player.Slow.IsPressed() ? _slowMagnification : 1f);
        transform.position += (Vector3)moveInput * currentSpeed * Time.deltaTime;
        
        LimitPosition(this.transform);

        if (_actionMap.Player.Fire.IsPressed())
        {
            Fire();
        }
    }
    
    public void Initialize(PlayerData data)
    {
        this.bulletName = data.bulletName;
        this.speed = data.speed;
        this.fireRate = data.fireRate;
        this.life = data.life;
        this.invincibleTime = data.invincibleTime;
        this._slowMagnification = data.slowMagnification;
        this._bombCount.Value = data.bombCount;
        this.initPos = data.spawnPosition;
        
        Debug.Log("Player initialized with data. Speed: " + this.speed);
    }
    
    public void InitInput()
    {
        var manager = InputSystemActionsManager.Instance();
        _actionMap = manager.GetInputSystem_Actions();
        manager.PlayerEnable();
        
        Observable.FromEvent<InputAction.CallbackContext>(
                handler => _actionMap.Player.Bomb.started += handler,
                handler => _actionMap.Player.Bomb.started -= handler)
            .Subscribe(_ => UseBomb())
            .AddTo(this.disposables);
    }

    public void LimitPosition(Transform trNeedLimit)
    {
        trNeedLimit.position = new Vector3(Mathf.Clamp(trNeedLimit.position.x, worldPosLeftBottom.x, worldPosTopRight.x),
                                           Mathf.Clamp(trNeedLimit.position.y, worldPosLeftBottom.y, worldPosTopRight.y),
                                           trNeedLimit.position.z);
    }
    
    private void UseBomb()
    {
        if (_bombCount.Value <= 0) { return; }

        _bombCount.Value--;
        Debug.Log("BOMB! Remaining: " + _bombCount.Value);

        var activeBullets = new List<Bullet>(BulletManager.Instance().ActiveEnemyBullets);
        int scoreGained = 0;

        foreach (var bullet in activeBullets)
        {
            if (bullet != null && bullet.gameObject.activeSelf)
            {
                scoreGained += bullet.scoreValue;
                BulletManager.Instance().ReturnBullet(bullet.gameObject, bullet.bulletName, bullet.side);

            }
        }
        
        Missile[] allMissiles = FindObjectsOfType<Missile>();
        foreach (var missile in allMissiles)
        {
            scoreGained += missile.scoreValue;
            Destroy(missile.gameObject);
        }
        
        if (scoreGained > 0)
        {
            GameManager.Instance().AddScore(scoreGained);
        }
    }

    public void Rebirth()
    {
        StartCoroutine(DoRebirth());
    }

    IEnumerator DoRebirth()
    {
        yield return new WaitForSeconds(2f);
        timer = 0;
        Init();
        Fly();
    }

    public bool Isinvincible
    {
        get { return timer < invincibleTime; }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(death || Isinvincible)
            return;
        

        Bullet bullet = col.gameObject.GetComponent<Bullet>();
        BossPart part = col.gameObject.GetComponent<BossPart>();

        if ((bullet != null && bullet.side == SIDE.ENEMY) || col.GetComponent<Enemy>() != null || part != null)
        {
            Die();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (death || Isinvincible)
            return;

        
        if (col.gameObject.name.Equals("ScoreArea"))
        {
            if(OnScore != null)
            {
                OnScore(1);
            }
        }
        
    }
}