using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum ControlType
{
    Left,
    Right,
    Shoot,
    Pause,
    Fullscreen,
    WeaponNext,
    WeaponPrevious,
    Restart,
    EndTurn,
    PlayerModeToggle,
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

        // Tab or R
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.R)) HandleControlDown(ControlType.WeaponNext);
        if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.R)) HandleControlUp(ControlType.WeaponNext);

        // Up or down arrow changes 2 modes
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) HandleControlDown(ControlType.PlayerModeToggle);
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)) HandleControlUp(ControlType.PlayerModeToggle);

        // Escape or P
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) HandleControlDown(ControlType.Pause);
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P)) HandleControlUp(ControlType.Pause);
    }

    // Custom methods
    // --------------------------------------------------

    public static void HandleControlDown(ControlType controlType)
    {
        OnControlDown?.Invoke(controlType);
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isLeftButtonPressed = true;
                break;
            case ControlType.Right:
                Instance.isRightButtonPressed = true;
                break;
            case ControlType.Shoot:
                Instance.isShootButtonPressed = true;
                break;
            case ControlType.Pause:
                Debug.LogError("Pause control pressed; not implemented");
                break;
            case ControlType.Fullscreen:
                Screen.fullScreen = !Screen.fullScreen;
                break;
        }
    }

    public static void HandleControlUp(ControlType controlType)
    {
        OnControlUp?.Invoke(controlType);
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isLeftButtonPressed = false;
                break;
            case ControlType.Right:
                Instance.isRightButtonPressed = false;
                break;
            case ControlType.Shoot:
                Instance.isShootButtonPressed = false;
                break;
            case ControlType.Pause:
                Debug.LogError("Pause control released; not implemented");
                break;
            case ControlType.Restart:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case ControlType.EndTurn:
                LevelManager.Instance.EndPlayerTurn();
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
            default:
                return false;
        }
    }
}
