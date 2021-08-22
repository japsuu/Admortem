using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeyManager : MonoBehaviour
{
    public static HotkeyManager Instance;
    
    public event Action OnKeyDownEvent = null;

    public Dictionary<KeyCode, Action> HotKeys = new Dictionary<KeyCode, Action>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Update()
    {
        if (!Input.anyKeyDown) return;

        OnKeyDownEvent?.Invoke();
    }
}
