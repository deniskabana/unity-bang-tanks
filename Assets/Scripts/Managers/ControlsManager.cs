using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ControlType
{
    Left,
    Right,
    Shoot,
    Pause
}

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager Instance;
    public bool debug = false;

    // Runtime variables
    // --------------------------------------------------

    bool isLeftButtonPressed = false;
    bool isRightButtonPressed = false;
    bool isShootButtonPressed = false;

    // Events
    public static UnityEvent<ControlType> OnControlDown = new();
    public static UnityEvent<ControlType> OnControlUp = new();

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
    }

    void Update()
    {
        // Left arrow
        if (Input.GetKeyDown(KeyCode.LeftArrow)) HandleControlDown(ControlType.Left);
        if (Input.GetKeyUp(KeyCode.LeftArrow)) HandleControlUp(ControlType.Left);

        // Right arrow
        if (Input.GetKeyDown(KeyCode.RightArrow)) HandleControlDown(ControlType.Right);
        if (Input.GetKeyUp(KeyCode.RightArrow)) HandleControlUp(ControlType.Right);

        // Space
        if (Input.GetKeyDown(KeyCode.Space)) HandleControlDown(ControlType.Shoot);
        if (Input.GetKeyUp(KeyCode.Space)) HandleControlUp(ControlType.Shoot);

        // Escape or P
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            HandleControlDown(ControlType.Pause);
        }
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P))
        {
            HandleControlUp(ControlType.Pause);
        }
    }

    // Custom methods
    // --------------------------------------------------

    public static void HandleControlDown(ControlType controlType)
    {
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isLeftButtonPressed = true;
                OnControlDown?.Invoke(ControlType.Left);
                break;
            case ControlType.Right:
                Instance.isRightButtonPressed = true;
                OnControlDown?.Invoke(ControlType.Right);
                break;
            case ControlType.Shoot:
                Instance.isShootButtonPressed = true;
                OnControlDown?.Invoke(ControlType.Shoot);
                break;
            case ControlType.Pause:
                Debug.Log("Pause control pressed; not implemented");
                OnControlDown?.Invoke(ControlType.Pause);
                break;
        }
    }

    public static void HandleControlUp(ControlType controlType)
    {
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isLeftButtonPressed = false;
                OnControlUp?.Invoke(ControlType.Left);
                break;
            case ControlType.Right:
                Instance.isRightButtonPressed = false;
                OnControlUp?.Invoke(ControlType.Right);
                break;
            case ControlType.Shoot:
                Instance.isShootButtonPressed = false;
                OnControlUp?.Invoke(ControlType.Shoot);
                break;
            case ControlType.Pause:
                Debug.Log("Pause control released; not implemented");
                OnControlUp?.Invoke(ControlType.Pause);
                break;
        }
    }

    public static bool GetInput(ControlType controlType)
    {
        switch (controlType)
        {
            case ControlType.Left:
                return Instance.isLeftButtonPressed;
            case ControlType.Right:
                return Instance.isRightButtonPressed;
            case ControlType.Shoot:
                return Instance.isShootButtonPressed;
            case ControlType.Pause:
                return false;
            default:
                return false;
        }
    }
}
