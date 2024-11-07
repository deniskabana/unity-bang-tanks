using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
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

    [Header("References")]
    [SerializeField] GameObject touchControlsContainer;

    // Runtime variables
    // --------------------------------------------------

    bool isUILeftPressed = false;
    bool isUIRightPressed = false;
    bool isUIShootPressed = false;

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
        // Keyboard controls listeners
        // --------------------------------------------------

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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) HandleControlDown(ControlType.Pause);
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P)) HandleControlUp(ControlType.Pause);
    }

    // Custom methods
    // --------------------------------------------------

    public static void ShowTouchControls(bool show)
    {
        Instance.touchControlsContainer.SetActive(show);
    }

    public static void HandleControlDown(ControlType controlType)
    {
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isUILeftPressed = true;
                break;
            case ControlType.Right:
                Instance.isUIRightPressed = true;
                break;
            case ControlType.Shoot:
                Instance.isUIShootPressed = true;
                break;
            case ControlType.Pause:
                Debug.Log("Pause control pressed; not implemented");
                break;
        }
    }

    public static void HandleControlUp(ControlType controlType)
    {
        switch (controlType)
        {
            case ControlType.Left:
                Instance.isUILeftPressed = false;
                break;
            case ControlType.Right:
                Instance.isUIRightPressed = false;
                break;
            case ControlType.Shoot:
                Instance.isUIShootPressed = false;
                break;
            case ControlType.Pause:
                Debug.Log("Pause control released; not implemented");
                break;
        }
    }

}
