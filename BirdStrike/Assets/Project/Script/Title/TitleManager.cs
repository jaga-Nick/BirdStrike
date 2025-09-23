using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Common;

public class TitleManager : MonoBehaviour
{
    
    private InputSystem_Actions _actionMap;

    private void Awake()
    {
        var manager = InputSystemActionsManager.Instance();
        _actionMap = manager.GetInputSystem_Actions();
        manager.UIEnable();
    }

    private void Update()
    {
        if (_actionMap.UI.Submit.IsPressed())
        {
            GameManager.Instance().StartGame();
        }
        
        if (_actionMap.UI.Cancel.IsPressed())
        {
            Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        }
    }
}
