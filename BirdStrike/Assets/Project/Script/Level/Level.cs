using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Level : MonoBehaviour
{
    public int LevelID;
    public string LevelName;

    public Boss Boss;

    public List<SpawnRule> Rules = new List<SpawnRule>();

    public UnityAction<LEVEL_RESULT> OnLevelEnd;

    float timeSincelLevelStart = 0;

    float levelStartTime = 0;

    public float bossTime = 60f;

    float timer = 0;

    Boss boss = null;

    public enum LEVEL_RESULT
    {
        NONE,
        SUCCESS,
        FAILD
    }

    public LEVEL_RESULT result = LEVEL_RESULT.NONE;

    void Start()
    {
        // ザコ敵のウェーブを開始する処理をコメントアウトし、ボス戦を即時開始する
        // StartCoroutine(RunLevel()); 

        UIManager.Instance.ShowLevelStart(string.Format("LEVEL {0} {1}", this.LevelID, this.LevelName));
        
        // ボスを即座に生成して戦闘開始
        if (Boss != null)
        {
            boss = (Boss)UnitManager.Instance.GenerateEnemy(Boss.gameObject);
            boss.target = GameManager.Instance.player;
            // Fly()や攻撃開始はBossスクリプトのOnStart()で行われるため、ここでは不要
            boss.OnDeath += Boss_OnDeath;
        }
        
    }

    IEnumerator RunLevel()
    {
        UIManager.Instance.ShowLevelStart(string.Format("LEVEL {0} {1}", this.LevelID, this.LevelName));
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < Rules.Count; i++)
        {
            SpawnRule rule = Instantiate<SpawnRule>(Rules[i]);
        }
    }

    void Update()
    {
        // ボス出現タイマーのロジックを不要にするため、中身を空にするかコメントアウト
        /*
        timeSincelLevelStart = Time.realtimeSinceStartup - levelStartTime;

        if(result != LEVEL_RESULT.NONE)
            return;
        timer += Time.deltaTime;
        if (timeSincelLevelStart > bossTime)
        {
            if (timer > bossTime)
            {
                if (boss == null)
                {
                    boss = (Boss)UnitManager.Instance.GenerateEnemy(Boss.gameObject);
                    boss.target = GameManager.Instance.player;
                    boss.Fly();
                    boss.OnDeath += Boss_OnDeath;
                }
            }
        }
        */
    }

    private void Boss_OnDeath(Unit sender)
    {
        this.result = LEVEL_RESULT.SUCCESS;
        if (OnLevelEnd != null)
        {
            OnLevelEnd(this.result);
        }
        timer = 0;
    }
}
