using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Common;

namespace Result
{
    /// <summary>
    /// リザルトシーンを管理するクラス
    /// </summary>
    public class ResultManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text resultText;  // "You Win!" または "You Lose" を表示するテキスト
        [SerializeField] private Text scoreText;   // スコアを表示するテキスト
        
        private InputSystem_Actions _actionMap;
        private bool isOnce = false;
    
        private void Awake()
        {
            var manager = InputSystemActionsManager.Instance();
            _actionMap = manager.GetInputSystem_Actions();
            manager.UIEnable();
        }
        
        private void Start()
        {
            isOnce = false;
            
            // GameManagerはシーンをまたいで存在しているので、インスタンスを探してくる
            GameManager gameManager = GameManager.Instance();
    
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
    
            // GameManagerが保持している勝利フラグに応じて、表示するテキストを変える
            if (gameManager.IsGameWon)
            {
                resultText.text = "You Win!";
            }
            else
            {
                resultText.text = "You Lose";
            }
    
            // GameManagerからスコアを取得して表示する（"D6"は6桁のゼロ埋め）
            scoreText.text = "Score : " + gameManager.Score.ToString("D6");
        }
    
        private void Update()
        {
            if (_actionMap.UI.Submit.WasReleasedThisFrame() && !isOnce)
            {
                isOnce = true;
                GameManager.Instance().ReturnToTitle();
            }
            
            if (_actionMap.UI.Cancel.IsPressed())
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
                #else
                    Application.Quit();//ゲームプレイ終了
                #endif
            }
        }
    }
}
