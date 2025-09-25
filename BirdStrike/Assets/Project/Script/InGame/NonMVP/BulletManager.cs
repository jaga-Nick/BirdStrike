using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Common;
using InGame.NonMVP;



/// <summary>
/// Bulletをオブジェクトプール管理するクラス
/// </summary>
public class BulletManager : LocalMonoSingletonBase<BulletManager>
{
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    public List<Bullet> ActiveEnemyBullets { get; private set; } = new List<Bullet>(); // 現在アクティブ中の敵の弾リスト

    /// <summary>
    /// プールを非同期で初期化
    /// </summary>
    public async UniTask InitializePoolsAsync()
    {
        Debug.Log("Initializing Bullet Pools...");
        var bulletDb = DataManager.Instance.GetAllBulletData();

        foreach (var bulletData in bulletDb)
        {
            if (!_pools.ContainsKey(bulletData.bulletName))
            {
                // Addressablesからプレハブをロード
                var handle = Addressables.LoadAssetAsync<GameObject>(bulletData.addressableKey);
                GameObject prefab = await handle.ToUniTask();

                if (prefab != null)
                {
                    _pools[bulletData.bulletName] = new Queue<GameObject>();
                    for (int i = 0; i < 50; i++) // 各弾を50個ずつプール
                    {
                        GameObject bullet = Instantiate(prefab, transform);
                        bullet.SetActive(false);
                        _pools[bulletData.bulletName].Enqueue(bullet);
                    }
                    Debug.Log($"Pool for '{bulletData.bulletName}' created with 50 instances.");
                }
                else
                {
                    Debug.LogError($"Failed to load prefab for bullet: {bulletData.bulletName}");
                }
            }
        }
    }
    
    /// <summary>
    /// 同期的にプールから弾を取得する
    /// </summary>
    public GameObject GetBullet(string bulletName)
    {
        if (!_pools.ContainsKey(bulletName) || _pools[bulletName].Count == 0)
        {
            Debug.LogWarning($"Pool for {bulletName} is empty. Consider increasing pool size.");
            return null;
        }

        var bulletGO = _pools[bulletName].Dequeue();
        AudioManager.Instance().PlaySe("Fire");
        bulletGO.SetActive(true);
        
        var bulletComp = bulletGO.GetComponent<Bullet>();
        if (bulletComp.side == SIDE.ENEMY && !ActiveEnemyBullets.Contains(bulletComp))
        {
            ActiveEnemyBullets.Add(bulletComp);
        }

        return bulletGO;
    }

    /// <summary>
    /// プールに弾を戻す
    /// </summary>
    public void ReturnBullet(GameObject bullet, string bulletName, SIDE side)
    {
        if (bullet == null || !bullet.activeSelf) return;

        bullet.SetActive(false);
        if (_pools.ContainsKey(bulletName))
        {
            _pools[bulletName].Enqueue(bullet);
        }
        else
        {
            Destroy(bullet);
        }

        var bulletComp = bullet.GetComponent<Bullet>();
        if (side == SIDE.ENEMY && ActiveEnemyBullets.Contains(bulletComp))
        {
            ActiveEnemyBullets.Remove(bulletComp);
        }
    }
}
