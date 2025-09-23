using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameDataWrapper
{
    public List<BulletData> bulletDatabase;
    public PlayerData playerData;
    public BossData bossData;
}

public class DataManager : MonoSingleton<DataManager>
{
    public PlayerData Player { get; private set; }
    public BossData Boss { get; private set; }
    private Dictionary<string, BulletData> _bulletDataDict;
    public bool IsDataLoaded { get; private set; } = false;

    public BulletData GetBulletData(string bulletName)
    {
        if (_bulletDataDict != null && _bulletDataDict.TryGetValue(bulletName, out BulletData data))
        {
            return data;
        }
        Debug.LogError($"BulletData with name '{bulletName}' not found!");
        return null;
    }
    
    // --- ▼▼▼ 追加 ▼▼▼ ---
    public List<BulletData> GetAllBulletData()
    {
        return _bulletDataDict.Values.ToList();
    }
    // --- ▲▲▲ 追加 ▲▲▲ ---

    public async UniTask LoadDataAsync()
    {
        IsDataLoaded = false;
        string filePath = Path.Combine(Application.streamingAssetsPath, "game_data.json");
        string jsonString;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(filePath))
        {
            await request.SendWebRequest();
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load data file on Android: " + request.error);
                return;
            }
            jsonString = request.downloadHandler.text;
        }
#else
        if (!File.Exists(filePath))
        {
            Debug.LogError("Cannot find data file at: " + filePath);
            return;
        }
        jsonString = await File.ReadAllTextAsync(filePath);
#endif
        
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogError("Data file is empty!");
            return;
        }

        try
        {
            GameDataWrapper dataWrapper = JsonUtility.FromJson<GameDataWrapper>(jsonString);
            
            if (dataWrapper == null || dataWrapper.playerData == null || dataWrapper.bossData == null || dataWrapper.bulletDatabase == null)
            {
                Debug.LogError("Failed to parse JSON.");
                return;
            }

            _bulletDataDict = dataWrapper.bulletDatabase.ToDictionary(data => data.bulletName);
            Player = dataWrapper.playerData;
            Boss = dataWrapper.bossData;
            IsDataLoaded = true;
            Debug.Log("Game data loaded successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing JSON file: " + e.Message);
        }
    }
}