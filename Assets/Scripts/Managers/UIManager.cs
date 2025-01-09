using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public RectTransform hudHealthBarSlider;
    public RectTransform hudBatteryBarGroup;
    public RectTransform hudBatteryRechargeRateText;
    public GameObject hudBatteryBarSliderPrefab;

    [Header("HUD / Tank Info")]
    public Transform hudTankBody1;
    public Transform hudTankBody2;
    public RectTransform hudTankName;
    public RectTransform indicatorMovement;
    public RectTransform indicatorAim;
    public RectTransform indicatorShootStrength;

    [Header("HUD / Weapon")]
    public RectTransform hudWeaponIcon;
    public RectTransform hudWeaponCostText;

    [Header("Strength Indicator")]
    public RectTransform strengthIndicatorHandle;
    public RectTransform strengthIndicatorHandleGhost;
    public RectTransform strengthIndicatorRange;

    [Header("In game UI prefabs")]
    public GameObject damagePopupPrefab;
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
    [SerializeField, Range(0f, 1f)] float emptyBatteryBarAlphaValue = 0.75f;

    [Header("Advanced")]
    [SerializeField, Range(0.01f, 2f)] float strengthIndicatorSpeed = 0.4f;
    [SerializeField] public UIManagerReferences references;
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
    List<GameObject> batteryBars = new List<GameObject>();
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

    void CreateBatteryBars(int amount)
    {
        if (debug) Debug.Log("UIManager: CreateBatteryBars: " + amount);

        // Clear previous battery bars
        foreach (GameObject go in batteryBars) Destroy(go);
        batteryBars.Clear();

        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(references.hudBatteryBarSliderPrefab, references.hudBatteryBarGroup);
            RectTransform rt = go.GetComponent<RectTransform>();
            float width = rt.sizeDelta.x * rt.localScale.x * 1.1f;
            rt.anchoredPosition = new Vector2(i * width, 0);
            batteryBars.Add(go);
        }
    }

    public static void UpdateActiveBatteryBars(float currentValue, int batteryCapacity, int batteryAmount, int batteryRechargeRate)
    {
        if (Instance.debug) Debug.Log("UIManager: UpdateActiveBatteryBars: " + currentValue + " / " + batteryCapacity + " / " + batteryAmount);
        if (currentValue < 0) return;
        if (Instance.batteryBars.Count != batteryAmount) Instance.CreateBatteryBars(batteryAmount);

        Instance.references.hudBatteryRechargeRateText.GetComponent<TextMeshProUGUI>().text = "+" + batteryRechargeRate;

        for (int i = 0; i < Instance.batteryBars.Count; i++)
        {
            Slider slider = Instance.batteryBars[i].GetComponent<Slider>();
            slider.maxValue = batteryCapacity;
            slider.value = Mathf.Clamp(currentValue - i * batteryCapacity, 0, batteryCapacity);

            if (slider.value > 0 && slider.value < 0.2f * slider.maxValue)
            {
                Animation anim = Instance.batteryBars[i].GetComponent<Animation>();
                if (!anim.isPlaying)
                {
                    anim.wrapMode = WrapMode.Loop;
                    anim.Play();
                }
            }
            else
            {
                Animation anim = Instance.batteryBars[i].GetComponent<Animation>();
                if (anim.isPlaying)
                {
                    anim.Stop();
                }

                Instance.batteryBars[i].transform.Find("Fill Area").GetComponent<CanvasGroup>().alpha = 1;
            }

            if (slider.value == 0)
            {
                Instance.batteryBars[i].GetComponent<CanvasGroup>().alpha = Instance.emptyBatteryBarAlphaValue;
            }
            else
            {
                Instance.batteryBars[i].GetComponent<CanvasGroup>().alpha = 1;
            }
        }
    }

    public static void UpdateActiveHealthBar(float currentValue, float maxValue)
    {
        if (Instance.debug) Debug.Log("UIManager: UpdateActiveHealthBar: " + currentValue + " / " + maxValue);
        if (currentValue < 0) return;
        Slider slider = Instance.references.hudHealthBarSlider.GetComponent<Slider>();
        slider.maxValue = maxValue;
        slider.value = currentValue;
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

    public static void SetActiveWeapon(WeaponDetail weapon)
    {
        if (Instance.debug) Debug.Log("UIManager: SetActiveWeapon: " + weapon.slug);
        Instance.references.hudWeaponIcon.GetComponent<Image>().sprite = weapon.hudIcon;
        Instance.references.hudWeaponCostText.GetComponent<TextMeshProUGUI>().text = weapon.batteryCost.ToString();
    }
}
