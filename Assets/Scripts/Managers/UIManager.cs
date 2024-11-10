using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[System.Serializable]
public struct UIManagerReferences
{
    [Header("Touch Controls")]
    public RectTransform touchControlsRt;
    public RectTransform touchButtonAim;
    public RectTransform touchButtonShoot;

    [Header("HUD")]
    public RectTransform hudContainer;
    public RectTransform hudGasBarMask;
    public RectTransform emptyBarReference;
    public RectTransform hudArmorBarMask;

    [Header("HUD / TankInfo")]
    public RectTransform hudTankImage;
    public RectTransform hudTankName;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] bool touchControlsEnabled = true;
    [SerializeField] bool animationsEnabled = true;
    [SerializeField] float uiMoveSpeed = 6;

    [Header("Advanced")]
    [SerializeField] UIManagerReferences references;

    // Runtime variables
    // --------------------------------------------------

    bool isHUDVisible = true;
    bool isTouchControlsVisible = true;
    float touchControlsYOff = -(Screen.height / 2);
    float hudYOff = Screen.height / 2;

    float touchControlsYDefault;
    float hudYDefault;
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
        HandleHUDPosition();
    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        // Activate / deactivate UI elements
        references.touchControlsRt.gameObject.SetActive(touchControlsEnabled);
        references.hudContainer.gameObject.SetActive(true);
        // Default Y positions
        touchControlsYDefault = references.touchControlsRt.localPosition.y;
        hudYDefault = references.hudContainer.localPosition.y;
        // Default bar width (all bars are same width)
        barDefaultWidth = references.emptyBarReference.sizeDelta.x;
        ResetBars();
        // Touch controls listeners
        LevelManager.OnPlayerTurnStart.AddListener(ShowTouchControls);
        LevelManager.OnPlayerTurnEnd.AddListener(HideTouchControls);
        // HUD listeners
        LevelManager.OnPlayerTurnStart.AddListener(ShowHUD);
        LevelManager.OnPlayerTurnEnd.AddListener(HideHUD);
    }

    // TODO: Problematic performance with this method in Update, should be called only when necessary and cancel early if not needed
    void HandleTouchControlsPosition()
    {
        if (!touchControlsEnabled) return;
        RectTransform rt = references.touchControlsRt;
        if (!rt) return;
        HandleUIRectPosition(rt, isTouchControlsVisible ? touchControlsYDefault : touchControlsYDefault + touchControlsYOff);
    }

    // TODO: Problematic performance with this method in Update, should be called only when necessary and cancel early if not needed
    void HandleHUDPosition()
    {
        RectTransform rt = references.hudContainer;
        if (!rt) return;
        HandleUIRectPosition(rt, isHUDVisible ? hudYDefault : hudYDefault + hudYOff);
    }

    void HandleUIRectPosition(RectTransform rt, float desiredY)
    {
        float currentY = rt.localPosition.y;
        if (currentY == desiredY) return;
        float newY = animationsEnabled ? Mathf.Lerp(currentY, desiredY, Time.deltaTime * uiMoveSpeed) : desiredY;
        rt.localPosition = new Vector3(rt.localPosition.x, newY, rt.localPosition.z);
    }

    public static void ShowTouchControls()
    {
        if (!Instance.touchControlsEnabled) return;
        if (Instance.debug) Debug.Log("ShowTouchControls");
        Instance.isTouchControlsVisible = true;
    }

    public static void HideTouchControls()
    {
        if (!Instance.touchControlsEnabled) return;
        if (Instance.debug) Debug.Log("HideTouchControls");
        Instance.isTouchControlsVisible = false;
    }

    public static void ShowHUD()
    {
        if (Instance.debug) Debug.Log("ShowHUD");
        Instance.isHUDVisible = true;
    }

    public static void HideHUD()
    {
        if (Instance.debug) Debug.Log("HideHUD");
        Instance.isHUDVisible = false;
    }

    public static void ShowTouchAimButton()
    {
        if (Instance.debug) Debug.Log("ShowAimButton");
        Instance.references.touchButtonAim.gameObject.SetActive(true);
        Instance.references.touchButtonShoot.gameObject.SetActive(false);
    }
    public static void ShowTouchShootButton()
    {
        if (Instance.debug) Debug.Log("ShowShootButton");
        Instance.references.touchButtonAim.gameObject.SetActive(false);
        Instance.references.touchButtonShoot.gameObject.SetActive(true);
    }

    public static void UpdateActiveGasBar(float value)
    {
        if (Instance.debug) Debug.Log("UpdateActiveGasBar: " + value);
        if (value < 0) return;
        Instance.references.hudGasBarMask.sizeDelta =
            new Vector2(value * Instance.barDefaultWidth, Instance.references.hudGasBarMask.sizeDelta.y);
    }

    public static void UpdateActiveArmorBar(float value)
    {
        if (Instance.debug) Debug.Log("UpdateActiveArmorBar: " + value);
        if (value < 0) return;
        Instance.references.hudArmorBarMask.sizeDelta =
            new Vector2(value * Instance.barDefaultWidth, Instance.references.hudArmorBarMask.sizeDelta.y);
    }

    public static void ResetBars()
    {
        if (Instance.debug) Debug.Log("ResetBars");
        UpdateActiveArmorBar(1);
        UpdateActiveGasBar(1);
    }

    public static void SetActiveTankHUDImage(Sprite sprite)
    {
        if (Instance.debug) Debug.Log("SetActiveTankHUDImage: " + sprite.name);
        Instance.references.hudTankImage.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
    }

    public static void SetActiveTankHUDName(string name)
    {
        if (Instance.debug) Debug.Log("SetActiveTankHUDName: " + name);
        Instance.references.hudTankName.GetComponent<TextMeshProUGUI>().text = name;
    }
}
