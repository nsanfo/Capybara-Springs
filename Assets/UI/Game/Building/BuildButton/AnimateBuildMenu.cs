using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateBuildMenu : MonoBehaviour
{
    [Header("Build Type Panel")]
    public RectTransform panelTransform;

    [Header("Build Divider Panel")]
    public GameObject panelDivider;

    readonly private float targetTime = 0.3f;
    private float originalWidth, elapsedTime;
    private Vector2 startingWidth, targetWidth;
    private bool animate = false, toggleState;

    void Start()
    {
        originalWidth = panelTransform.rect.width;
        panelTransform.sizeDelta = new Vector2(0, panelTransform.rect.height);

        panelDivider.SetActive(false);
    }

    void Update()
    {
        if (animate) HandleAnimation();
    }

    private void HandleAnimation()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / targetTime;

        panelTransform.sizeDelta = Vector2.Lerp(startingWidth, targetWidth, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            animate = false;
        }

        // Edge cases for animation timing
        if (!toggleState)
        {
            if (percentageComplete >= 0.9)
            {
                GetComponent<BuildingToggleButton>().buildHover.UpdateHoverPanelLocation();
            }
            else if (percentageComplete > 0.75)
            {
                panelDivider.SetActive(false);
            }
        }
    }

    public void UpdateAnimation(bool toggleState)
    {
        animate = true;
        this.toggleState = toggleState;
        startingWidth = new Vector2(panelTransform.rect.width, panelTransform.rect.height);

        // Expand menu
        if (toggleState)
        {
            elapsedTime = 0;
            targetWidth = new Vector2(originalWidth, panelTransform.rect.height);
            panelDivider.SetActive(true);
        }
        // Contract menu
        else
        {
            elapsedTime = 0;
            targetWidth = new Vector2(0, panelTransform.rect.height);
        }
    }
}
