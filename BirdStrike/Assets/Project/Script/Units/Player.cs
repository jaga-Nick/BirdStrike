using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Common;

public class Player : Unit
{
    private float invincibleTime;

    private float _slowMagnification;

    private float timer = 0;

    Vector2 worldPosLeftBottom;
    Vector2 worldPosTopRight;
    
    private InputSystem_Actions _actionMap;

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
        float currentSpeed = speed * (_actionMap.Player.Slow.IsPressed() ? _slowMagnification : 1f); // 減速
        transform.position += (Vector3)moveInput * currentSpeed * Time.deltaTime;
        

        LimitPosition(this.transform);


        // 攻撃
        if (_actionMap.Player.Fire.IsPressed())
        {
            Fire();
        }
        
        // ボム
        if (_actionMap.Player.Bomb.WasPressedThisFrame())
        {
            Debug.Log("ボム使用");
        }
        

        
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(PlayerData data)
    {
        // JSONから読み込んだデータでパラメータを初期化
        this.speed = data.speed;
        this.fireRate = data.fireRate;
        this.life = data.initialLife;
        this.invincibleTime = data.invincibleTime;
        this._slowMagnification = data.slowMagnification;
        
        Debug.Log("Player initialized with data. Speed: " + this.speed);
    }
    
    /// <summary>
    /// 入力初期化
    /// </summary>
    public void InitInput()
    {
        var manager = InputSystemActionsManager.Instance();
        _actionMap = manager.GetInputSystem_Actions();
        manager.PlayerEnable();
    }

    public void LimitPosition(Transform trNeedLimit)
    {
        trNeedLimit.position = new Vector3(Mathf.Clamp(trNeedLimit.position.x, worldPosLeftBottom.x, worldPosTopRight.x),
                                           Mathf.Clamp(trNeedLimit.position.y, worldPosLeftBottom.y, worldPosTopRight.y),
                                           trNeedLimit.position.z);
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
        if(death)
            return;
        if (Isinvincible)
            return;

        Item item = col.gameObject.GetComponent<Item>();
        if(item != null)
        {
            item.Use(this);
        }

        Element bullet = col.gameObject.GetComponent<Element>();
        Enemy enemy = col.gameObject.GetComponent<Enemy>();
        if (bullet == null && enemy == null)
        { 
            return;
        }

        if(bullet != null && bullet.side == SIDE.ENEMY)
        {
            hp = hp - bullet.power;
            if(hp <= 0)
            {
                Die();
            }
        }
        if(enemy != null)
        {
            hp = 0;
                Die();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (death)
            return;
        if(Isinvincible)
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
