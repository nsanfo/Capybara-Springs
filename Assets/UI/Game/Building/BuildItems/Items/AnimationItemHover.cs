using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimationItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Item Colors")]
    public Color unselectedColor, selectedColor;

    readonly private float targetTime = 0.03f;
    private float elapsedTime;
    private Quaternion startingRot, targetRot, originalRot;
    private bool animate = false, selected = false;
    public Image background;

    // Start is called before the first frame update
    void Start()
    {
        originalRot = transform.rotation;
    }

    void Update()
    {
        if (animate) HandleAnimation();
    }

    private void HandleAnimation()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / targetTime;

        transform.rotation = Quaternion.Lerp(startingRot, targetRot, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            animate = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selected) return;

        animate = true;
        elapsedTime = 0;
        startingRot = transform.rotation;
        targetRot = Quaternion.Euler(0, 0, 10);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selected) return;

        animate = true;
        elapsedTime = 0;
        startingRot = transform.rotation;
        targetRot = originalRot;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        /*
        if (pointerEventData.button != PointerEventData.InputButton.Left) return;

        selected = !selected;

        if (selected)
        {
            background.color = selectedColor;
        }
        else
        {
            background.color = unselectedColor;
        }
        */
    }

    public void UpdateClick()
    {
        if (GetComponent<Toggle>().isOn)
        {
            selected = true;
            background.color = selectedColor;
        }
        else
        {
            selected = false;
            background.color = unselectedColor;
            animate = true;
            elapsedTime = 0;
            startingRot = transform.rotation;
            targetRot = originalRot;
        }
    }
}
