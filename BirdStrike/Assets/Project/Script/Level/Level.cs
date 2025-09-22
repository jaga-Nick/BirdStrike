using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UniRx; // UniRxの名前空間を追加

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
        UIManager.Instance.ShowLevelStart(string.Format("LEVEL {0} {1}", this.LevelID, this.LevelName));
        
        if (Boss != null)
        {
            boss = (Boss)UnitManager.Instance.GenerateEnemy(Boss.gameObject);
            boss.target = GameManager.Instance.player;

            // --- ▼▼▼ 修正 ▼▼▼ ---
            // OnDeathイベントをUniRxで購読する
            boss.OnDeathAsObservable
                .Subscribe(Boss_OnDeath)
                .AddTo(this); // Levelオブジェクトが破棄される時に購読も自動で解除される
            // --- ▲▲▲ 修正 ▲▲▲ ---
        }
    }

    void Update()
    {
        // Update内のボス出現ロジックは不要
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