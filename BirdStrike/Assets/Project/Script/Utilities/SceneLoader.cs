using Common;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : SingletonMonoBehaviourBase<SceneLoader>
{
    private SceneInstance _currentScene;

    /// <summary>
    /// 指定されたシーンをAddressablesでロードする
    /// </summary>

    public async UniTask LoadSceneAsync(string sceneAddress, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        // 既にシーンがロードされている場合は、先にアンロードする
        if (_currentScene.Scene.IsValid() && loadMode == LoadSceneMode.Single)
        {
            await Addressables.UnloadSceneAsync(_currentScene).ToUniTask();
        }

        // Addressablesでシーンをロードし、完了を待つ
        var handle = Addressables.LoadSceneAsync(sceneAddress, loadMode);
        _currentScene = await handle.ToUniTask();

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load scene: {sceneAddress}");
        }
    }
}
