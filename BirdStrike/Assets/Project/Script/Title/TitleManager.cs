using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Common;

public class TitleManager : MonoBehaviour
{
    
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
        AudioManager.Instance().PlayBgm("BGM");
    }

    private void Update()
    {
        if (_actionMap.UI.Submit.IsPressed() && !isOnce)
        {
            isOnce = true;
            GameManager.Instance().StartGame();
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
