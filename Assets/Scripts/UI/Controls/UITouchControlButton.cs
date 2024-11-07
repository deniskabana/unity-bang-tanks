using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class UITouchControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] ControlType controlButtonType;
    [SerializeField] Transform buttonBackground;

    [Header("Sprites")]
    [SerializeField] Sprite spriteDefault;
    [SerializeField] Sprite spriteActive;

    // Runtime variables
    // --------------------------------------------------

    bool isPressed = false;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        buttonBackground.GetComponent<Image>().sprite = spriteDefault;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        buttonBackground.GetComponent<Image>().sprite = spriteActive;
        ControlsManager.HandleControlDown(controlButtonType);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        buttonBackground.GetComponent<Image>().sprite = spriteDefault;
        ControlsManager.HandleControlUp(controlButtonType);
    }

    // Custom methods
    // --------------------------------------------------
}
