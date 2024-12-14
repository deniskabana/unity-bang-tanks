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

    [Header("Touch Controls / Animations")]
    public AnimationClip uiAnimationIn;
    public AnimationClip uiAnimationOut;

    [Header("HUD")]
    public RectTransform hudContainer;
    public RectTransform hudGasBarMask;
    public RectTransform emptyBarReference;
    public RectTransform hudArmorBarMask;

    [Header("HUD / Tank Info")]
    public Transform hudTankBody1;
    public Transform hudTankBody2;
    public RectTransform hudTankName;
    public RectTransform indicatorMovement;
    public RectTransform indicatorAim;
    public RectTransform indicatorShootStrength;

    [Header("HUD / Weapon")]
    public RectTransform hudWeaponArrowLeft;
    public RectTransform hudWeaponArrowRight;
    public RectTransform hudWeaponIcon;
    public RectTransform hudWeaponName;

    [Header("Strength Indicator")]
    public RectTransform strengthIndicatorHandle;
    public RectTransform strengthIndicatorHandleGhost;
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

    [Header("Advanced")]
    [SerializeField, Range(0.1f, 10f)] float strengthIndicatorSpeed = 1f;
    [SerializeField] UIManagerReferences references;
    [SerializeField] float minStrengthIndicatorXPos = -100;
    [SerializeField] float maxStrengthIndicatorXPos = 100;

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
    PlayerState.TurnStage touchControlStage;

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

        references.indicatorMovement.gameObject.SetActive(true);

        references.touchStageMovement.gameObject.SetActive(true);
        references.touchStageAim.gameObject.SetActive(true);
        references.touchStageShootStrength.gameObject.SetActive(true);

        references.strengthIndicatorHandleGhost.gameObject.SetActive(false);

        PlayAnimationIn(references.touchStageMovement.gameObject, true);
        PlayAnimationOut(references.touchStageAim.gameObject, true);
        PlayAnimationOut(references.touchStageShootStrength.gameObject, true);

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
        public float ghostValue;
        public bool isIncreasing;
    }
    StrengthIndicatorStatus strengthIndicatorStatus;

    void HandleStrengthIndicator()
    {
        if (!isStrengthIndicatorVisible) return;

        RectTransform rt = references.strengthIndicatorHandle;
        if (!rt) return;

        RectTransform rtGhost = references.strengthIndicatorHandleGhost;

        float newX = Mathf.Lerp(strengthIndicatorStatus.minPos, strengthIndicatorStatus.maxPos, strengthIndicatorStatus.currentValue);
        rt.anchoredPosition = new Vector2(newX, rt.anchoredPosition.y);

        // Ghost - last shot strength indicator
        if (rtGhost)
        {
            float ghostX = Mathf.Lerp(strengthIndicatorStatus.minPos, strengthIndicatorStatus.maxPos, strengthIndicatorStatus.ghostValue);
            rtGhost.anchoredPosition = new Vector2(ghostX, rtGhost.anchoredPosition.y);
        }

        // Shot strength indicator
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

    // Animations
    // --------------------------------------------------

    void PlayAnimationIn(GameObject uiObj, bool instant = false)
    {
        if (!animationsEnabled) return;
        if (debug) Debug.Log("UIManager: PlayAnimationIn (" + uiObj.name + ")");
        Animation anim = uiObj.GetComponent<Animation>();
        if (!anim || anim.isPlaying) return;
        anim.wrapMode = WrapMode.Once;

        if (anim.GetClip(references.uiAnimationIn.name) == null)
            anim.AddClip(references.uiAnimationIn, references.uiAnimationIn.name);

        anim.Play(references.uiAnimationIn.name);
        if (instant) anim[references.uiAnimationIn.name].time = anim[references.uiAnimationIn.name].length;
    }

    void PlayAnimationOut(GameObject uiObj, bool instant = false)
    {
        if (!animationsEnabled) return;
        if (debug) Debug.Log("UIManager: PlayAnimationOut (" + uiObj.name + ")");
        Animation anim = uiObj.GetComponent<Animation>();
        if (!anim) return;
        anim.wrapMode = WrapMode.Once;

        if (anim.GetClip(references.uiAnimationOut.name) == null)
            anim.AddClip(references.uiAnimationOut, references.uiAnimationOut.name);

        anim.Play(references.uiAnimationOut.name);
        if (instant) anim[references.uiAnimationOut.name].time = anim[references.uiAnimationOut.name].length;
    }

    // Static methods
    // --------------------------------------------------

    public static void SetTouchControlsStage(System.Nullable<PlayerState.TurnStage> stage)
    {
        if (stage == Instance.touchControlStage) return;
        if (Instance.debug) Debug.Log("UIManager: SetTouchControlsStage = " + stage);
        UIManagerReferences refs = Instance.references;
        if (stage != null) Instance.touchControlStage = (PlayerState.TurnStage)stage;

        switch (stage)
        {
            case PlayerState.TurnStage.Moving:
                Instance.PlayAnimationIn(refs.touchStageMovement.gameObject, true); // Instant animation for arrows in first turn
                // Reset previous stages instantly
                Instance.PlayAnimationOut(refs.touchStageAim.gameObject, true);
                Instance.PlayAnimationOut(refs.touchStageShootStrength.gameObject, true);

                refs.indicatorMovement.gameObject.SetActive(true);
                refs.indicatorAim.gameObject.SetActive(false);
                refs.indicatorShootStrength.gameObject.SetActive(false);
                break;

            case PlayerState.TurnStage.Aiming:
                Instance.PlayAnimationIn(refs.touchStageAim.gameObject);
                Instance.PlayAnimationOut(refs.touchStageMovement.gameObject);

                refs.indicatorMovement.gameObject.SetActive(false);
                refs.indicatorAim.gameObject.SetActive(true);
                refs.indicatorShootStrength.gameObject.SetActive(false);
                break;

            case PlayerState.TurnStage.Shooting:
                Instance.PlayAnimationIn(refs.touchStageShootStrength.gameObject);
                Instance.PlayAnimationOut(refs.touchStageAim.gameObject);

                refs.indicatorMovement.gameObject.SetActive(false);
                refs.indicatorAim.gameObject.SetActive(false);
                refs.indicatorShootStrength.gameObject.SetActive(true);
                break;

            case null:
            default:
                refs.indicatorMovement.gameObject.SetActive(false);
                refs.indicatorAim.gameObject.SetActive(false);
                refs.indicatorShootStrength.gameObject.SetActive(false);
                break;
        }
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
    }

    public static float GetStrengthIndicatorValue()
    {
        return Instance.strengthIndicatorStatus.currentValue;
    }

    public static void SetStrengthIndicatorGhostValue(float value)
    {
        if (Instance.debug) Debug.Log("UIManager: SetStrengthIndicatorGhostValue: " + value);
        Instance.references.strengthIndicatorHandleGhost.gameObject.SetActive(true);
        Instance.strengthIndicatorStatus.ghostValue = value;
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
