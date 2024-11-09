using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public bool debug = false;

    [Header("Touch Controls")]
    [SerializeField] bool uiControlsEnabled = true;
    [SerializeField] RectTransform touchControlsRt;
    [SerializeField] RectTransform touchButtonAim;
    [SerializeField] RectTransform touchButtonShoot;

    [Header("HUD")]
    [SerializeField] RectTransform hudContainer;
    [SerializeField] RectTransform hudGasBarMask; // Scaling this will mask the bar, revealing background
    [SerializeField] RectTransform hudArmorBarMask; // Scaling this will mask the bar, revealing background

    [Header("Animation")]
    [SerializeField] float uiMoveSpeed = 6;

    // Runtime variables
    // --------------------------------------------------

    bool isTouchControlsVisible = true;
    float touchControlsYDefault = 0;
    float touchControlsYOff = -(Screen.height / 2);

    float barDefaultWidth;

    // Build-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;

        Initialize();
    }

    void Update()
    {
        HandleTouchControlsPosition();
    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        touchControlsRt.gameObject.SetActive(uiControlsEnabled);

        barDefaultWidth = hudGasBarMask.sizeDelta.x;

        LevelManager.OnPlayerTurnStart.AddListener(ShowTouchControls);
        LevelManager.OnPlayerTurnEnd.AddListener(HideTouchControls);
    }

    void HandleTouchControlsPosition()
    {
        if (!uiControlsEnabled) return;
        if (touchControlsRt == null) return;
        RectTransform rt = touchControlsRt;
        HandleUIRectPosition(rt, isTouchControlsVisible ? touchControlsYDefault : touchControlsYOff);
    }

    void HandleUIRectPosition(RectTransform rt, float desiredY)
    {
        float currentY = rt.position.y;
        if (currentY == desiredY) return;
        float newY = Mathf.Lerp(currentY, desiredY, Time.deltaTime * uiMoveSpeed);
        rt.position = new Vector3(rt.position.x, newY, rt.position.z);
    }

    public static void ShowTouchControls()
    {
        if (!Instance.uiControlsEnabled) return;
        if (Instance.debug) Debug.Log("ShowTouchControls");
        Instance.isTouchControlsVisible = true;
    }

    public static void HideTouchControls()
    {
        if (!Instance.uiControlsEnabled) return;
        if (Instance.debug) Debug.Log("HideTouchControls");
        Instance.isTouchControlsVisible = false;
    }

    public static void ShowTouchAimButton()
    {
        if (Instance.debug) Debug.Log("ShowAimButton");
        Instance.touchButtonAim.gameObject.SetActive(true);
        Instance.touchButtonShoot.gameObject.SetActive(false);
    }
    public static void ShowTouchShootButton()
    {
        if (Instance.debug) Debug.Log("ShowShootButton");
        Instance.touchButtonAim.gameObject.SetActive(false);
        Instance.touchButtonShoot.gameObject.SetActive(true);
    }

    public static void UpdateActiveGasBar(float value)
    {
        if (Instance.debug) Debug.Log("UpdateActiveGasBar: " + value);
        Instance.hudGasBarMask.sizeDelta = new Vector2(value * Instance.barDefaultWidth, Instance.hudGasBarMask.sizeDelta.y);
    }

    public static void UpdateActiveArmorBar(float value)
    {
        if (Instance.debug) Debug.Log("UpdateActiveArmorBar: " + value);
        Instance.hudArmorBarMask.sizeDelta = new Vector2(value * Instance.barDefaultWidth, Instance.hudArmorBarMask.sizeDelta.y);
    }
}
