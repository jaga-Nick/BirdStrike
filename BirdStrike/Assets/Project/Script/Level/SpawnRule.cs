using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRule : MonoBehaviour
{
    public Unit Monster;
    public float InitTime;
    public float Period;
    public int MaxNum;

    public int HP;
    public int Attack;

    float timeSincelLevelStart = 0;

    float levelStartTime = 0;

    int num = 0;
    float timer = 0;

    public ItemDrepRule dropRule;

    ItemDrepRule rule;


    void Start()
    {
        levelStartTime = Time.realtimeSinceStartup;
        if (dropRule != null)
            rule = Instantiate<ItemDrepRule>(dropRule);

    }

    void Update()
    {
        timeSincelLevelStart = Time.realtimeSinceStartup - levelStartTime;

        if(num >= MaxNum)
            return;

        // Start spawning enemies
        if (timeSincelLevelStart > InitTime)
        {
            timer += Time.deltaTime;
            if(timer > Period)
            {
                timer = 0;
                Enemy enemy = UnitManager.Instance.GenerateEnemy(Monster.gameObject);
                enemy.MaxHP = this.HP;
                enemy.Attack = this.Attack;
                enemy.OnDeath += Enemy_OnDeath;
                num++;
            }
        }

    }

    private void Enemy_OnDeath(Unit sender)
    {
        if(rule != null)
        {
            rule.Execute(sender.transform.position);

        }
    }
}
