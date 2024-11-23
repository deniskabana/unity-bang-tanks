using TMPro;
using Unity;
using UnityEngine;

[System.Serializable]
public struct UIManagerReferences
{
    [Header("Touch Controls")]
    public RectTransform touchControlsRt;
    public RectTransform touchStageMovement;
    public RectTransform touchStageAim;
    public RectTransform touchStageShootStrength;
    public RectTransform touchButtonShoot;

    [Header("HUD")]
    public RectTransform hudContainer;
    public RectTransform hudGasBarMask;
    public RectTransform emptyBarReference;
    public RectTransform hudArmorBarMask;

    [Header("HUD / TankInfo")]
    public Transform hudTankBody1;
    public Transform hudTankBody2;
    public RectTransform hudTankName;
    public RectTransform indicatorMovement;
    public RectTransform indicatorAim;
    public RectTransform indicatorShootStrength;

    [Header("Strength Indicator")]
    public RectTransform strengthIndicatorHandle;
    public RectTransform strengthIndicatorRange;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] bool touchControlsEnabled = true;
    [SerializeField] bool animationsEnabled = true;
    [SerializeField] float animationStopThreshold = 0.1f;
    [SerializeField] float uiMoveSpeed = 6;
    [SerializeField] float minStrengthIndicatorXPos = -100;
    [SerializeField] float maxStrengthIndicatorXPos = 100;

    [Header("Advanced")]
    [SerializeField, Range(0.1f, 10f)] float strengthIndicatorSpeed = 4f;
    [SerializeField] UIManagerReferences references;

    // Runtime variables
    // --------------------------------------------------

    bool isHUDVisible = true;
    bool isStrengthIndicatorVisible = false;
    bool isTouchControlsVisible = true;
    float touchControlsYOff = -(Screen.height / 3);
    float hudYOff = Screen.height / 3;
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
        HandleStrengthIndicator();
    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        // Activate / deactivate UI elements
        references.touchControlsRt.gameObject.SetActive(touchControlsEnabled);
        references.hudContainer.gameObject.SetActive(true);
        references.touchButtonShoot.gameObject.SetActive(true);

        references.touchStageMovement.gameObject.SetActive(true); // <- Initially visible
        references.touchStageAim.gameObject.SetActive(false);
        references.touchStageShootStrength.gameObject.SetActive(false);

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

    void HandleTouchControlsPosition()
    {
        if (!touchControlsEnabled) return;
        RectTransform rt = references.touchControlsRt;
        if (!rt) return;
        HandleUIRectPosition(rt, isTouchControlsVisible ? touchControlsYDefault : touchControlsYDefault + touchControlsYOff);
    }

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

        // Stop unnecessary Lerp if the two values are close enough
        if (Mathf.Abs(currentY - desiredY) < animationStopThreshold)
        {
            rt.localPosition = new Vector3(rt.localPosition.x, desiredY, rt.localPosition.z);
            return;
        }
        else
        {
            float newY = animationsEnabled ? Mathf.Lerp(currentY, desiredY, Time.deltaTime * uiMoveSpeed) : desiredY;
            rt.localPosition = new Vector3(rt.localPosition.x, newY, rt.localPosition.z);
        }
    }

    // Strength indicator
    // --------------------------------------------------

    [System.Serializable]
    struct StrengthIndicatorStatus
    {
        public float minPos;
        public float maxPos;
        public float speed;
        public float currentValue; // 0 to 1
        public bool isIncreasing;
    }
    StrengthIndicatorStatus strengthIndicatorStatus;

    void HandleStrengthIndicator()
    {
        if (!isStrengthIndicatorVisible) return;

        RectTransform rt = references.strengthIndicatorHandle;
        if (!rt) return;

        float newX = Mathf.Lerp(strengthIndicatorStatus.minPos, strengthIndicatorStatus.maxPos, strengthIndicatorStatus.currentValue);
        rt.anchoredPosition = new Vector2(newX, rt.anchoredPosition.y);

        if (strengthIndicatorStatus.isIncreasing)
        {
            strengthIndicatorStatus.currentValue += Time.deltaTime * strengthIndicatorStatus.speed;
            if (strengthIndicatorStatus.currentValue >= 1)
            {
                strengthIndicatorStatus.currentValue = 1;
                strengthIndicatorStatus.isIncreasing = false;
            }
        }
        else
        {
            strengthIndicatorStatus.currentValue -= Time.deltaTime * strengthIndicatorStatus.speed;
            if (strengthIndicatorStatus.currentValue <= 0)
            {
                strengthIndicatorStatus.currentValue = 0;
                strengthIndicatorStatus.isIncreasing = true;
            }
        }
    }

    // Static methods
    // --------------------------------------------------

    public static void SetTouchStage(PlayerState.TurnStage stage)
    {
        if (Instance.debug) Debug.Log("UIManager: SetTouchStage: " + stage);
        UIManagerReferences refs = Instance.references;
        refs.touchStageMovement.gameObject.SetActive(PlayerState.TurnStage.Moving == stage);
        refs.touchStageAim.gameObject.SetActive(PlayerState.TurnStage.Aiming == stage);
        refs.touchStageShootStrength.gameObject.SetActive(PlayerState.TurnStage.Shooting == stage);

        refs.indicatorMovement.gameObject.SetActive(PlayerState.TurnStage.Moving == stage);
        refs.indicatorAim.gameObject.SetActive(PlayerState.TurnStage.Aiming == stage);
        refs.indicatorShootStrength.gameObject.SetActive(PlayerState.TurnStage.Shooting == stage);
    }

    public static void ShowTouchControls()
    {
        if (!Instance.touchControlsEnabled) return;
        if (Instance.debug) Debug.Log("UIManager: ShowTouchControls");
        Instance.isTouchControlsVisible = true;
    }

    public static void HideTouchControls()
    {
        if (!Instance.touchControlsEnabled) return;
        if (Instance.debug) Debug.Log("UIManager: HideTouchControls");
        Instance.isTouchControlsVisible = false;
    }

    public static void ShowHUD()
    {
        if (Instance.debug) Debug.Log("UIManager: ShowHUD");
        Instance.isHUDVisible = true;
    }

    public static void HideHUD()
    {
        if (Instance.debug) Debug.Log("UIManager: HideHUD");
        Instance.isHUDVisible = false;
    }

    public static void AnimateStrengthIndicator()
    {
        if (Instance.debug) Debug.Log("UIManager: ShowStrengthIndicator");
        Instance.isStrengthIndicatorVisible = true;
        Instance.strengthIndicatorStatus = new StrengthIndicatorStatus
        {
            minPos = Instance.minStrengthIndicatorXPos,
            maxPos = Instance.maxStrengthIndicatorXPos,
            speed = Instance.strengthIndicatorSpeed,
            currentValue = 0,
            isIncreasing = true
        };
    }

    public static void StopStrengthIndicator()
    {
        if (Instance.debug) Debug.Log("UIManager: HideStrengthIndicator");
        Instance.isStrengthIndicatorVisible = false;
        Instance.references.touchStageShootStrength.gameObject.SetActive(false);
    }

    public static float GetStrengthIndicatorValue()
    {
        return Instance.strengthIndicatorStatus.currentValue;
    }

    public static void UpdateActiveGasBar(float value)
    {
        if (Instance.debug) Debug.Log("UIManager: UpdateActiveGasBar: " + value);
        if (value < 0) return;
        Instance.references.hudGasBarMask.sizeDelta =
            new Vector2(value * Instance.barDefaultWidth, Instance.references.hudGasBarMask.sizeDelta.y);
    }

    public static void UpdateActiveArmorBar(float value)
    {
        if (Instance.debug) Debug.Log("UIManager: UpdateActiveArmorBar: " + value);
        if (value < 0) return;
        Instance.references.hudArmorBarMask.sizeDelta =
            new Vector2(value * Instance.barDefaultWidth, Instance.references.hudArmorBarMask.sizeDelta.y);
    }

    public static void ResetBars()
    {
        if (Instance.debug) Debug.Log("UIManager: ResetBars");
        UpdateActiveArmorBar(1);
        UpdateActiveGasBar(1);
    }

    public static void SetActiveTankHUDSkin(PlayerSkin playerSkin)
    {
        if (Instance.debug) Debug.Log("UIManager: SetActiveTankHUDImage: " + playerSkin.name);
        Instance.references.hudTankBody1.GetComponent<SpriteRenderer>().sprite = playerSkin.body1;
        Instance.references.hudTankBody2.GetComponent<SpriteRenderer>().sprite = playerSkin.body2;
    }

    public static void SetActiveTankHUDName(string name)
    {
        if (Instance.debug) Debug.Log("UIManager: SetActiveTankHUDName: " + name);
        Instance.references.hudTankName.GetComponent<TextMeshProUGUI>().text = name;
    }
}
