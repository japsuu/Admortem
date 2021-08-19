using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Settings", menuName = "Custom/Settings")]
public class Settings : ScriptableObject
{
    // UI
    public static KeyCode ItemDropModifier = KeyCode.LeftControl;
    public static KeyCode ConsoleOpenKey = KeyCode.Period;

    // Movement
    public static KeyCode JumpKey = KeyCode.Space;

    // Block placing / breaking
    public static KeyCode BackgroundSelectKey = KeyCode.LeftAlt;

    // Camera control
    public static KeyCode CameraZoomInKey = KeyCode.KeypadPlus;
    public static KeyCode CameraZoomOutKey = KeyCode.KeypadMinus;
    public static KeyCode CameraZoomResetKey = KeyCode.KeypadMultiply;
}
