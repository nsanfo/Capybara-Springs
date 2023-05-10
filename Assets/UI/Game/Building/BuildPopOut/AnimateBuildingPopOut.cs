using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateBuildingPopOut : MonoBehaviour
{
    [Header("Item Select Toggles")]
    public ItemToggleHandler itemToggles;

    private RectTransform panelTransform;

    readonly private float targetTime = 0.15f;
    private float originalHeight, elapsedTime;
    private Vector2 startingHeight, targetHeight;
    private bool animate = false, active = false;

    void Start()
    {
        panelTransform = GetComponent<RectTransform>();
        originalHeight = panelTransform.rect.height;
        panelTransform.sizeDelta = new Vector2(panelTransform.rect.width, 0);
    }

    void Update()
    {
        if (animate) HandleAnimation();
    }

    private void HandleAnimation()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / targetTime;

        panelTransform.sizeDelta = Vector2.Lerp(startingHeight, targetHeight, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            animate = false;
        }
    }

    public void UpdateAnimation(bool toggleState)
    {
        animate = true;
        startingHeight = new Vector2(panelTransform.rect.width, panelTransform.rect.height);

        // Expand menu
        if (toggleState)
        {
            elapsedTime = 0;
            targetHeight = new Vector2(panelTransform.rect.width, originalHeight);
            active = true;
        }
        // Contract menu
        else
        {
            elapsedTime = 0;
            targetHeight = new Vector2(panelTransform.rect.width, 0);
            active = false;
            itemToggles.AllTogglesOff();
        }
    }

    public bool IsPopOutActive()
    {
        return active;
    }
}
