using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Networking;
#endif

[System.Serializable]
public class GameDataWrapper
{
    public PlayerData playerData;
    public BossData bossData;
}

public class DataManager : MonoSingleton<DataManager>
{
    public PlayerData Player { get; private set; }
    public BossData Boss { get; private set; }
    public bool IsDataLoaded { get; private set; } = false; // データが正常に読み込めたか

    public async UniTask LoadDataAsync()
    {
        IsDataLoaded = false; // 読み込み開始時にリセット
        string filePath = Path.Combine(Application.streamingAssetsPath, "game_data.json");
        string jsonString;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var request = UnityWebRequest.Get(filePath))
        {
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
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
            
            if (dataWrapper == null || dataWrapper.playerData == null || dataWrapper.bossData == null)
            {
                Debug.LogError("Failed to parse JSON. Check if the JSON structure and key names (e.g., 'playerData') match the C# classes.");
                return;
            }

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