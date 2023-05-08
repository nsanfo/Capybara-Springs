using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateStatsMenu : MonoBehaviour
{
    [Header("Text Panel")]
    public GameObject textPanel;
    private RectTransform panelRect;

    [Header("Scrollbar")]
    public Scrollbar scrollbar;

    private Image fadePanel;

    private float elapsedTime, fadeTargetTime = 0.3f, panelTargetTime = 0.2f, startAlpha, endAlpha, targetAlpha;
    private Vector2 startRect, endRect, targetRect;
    private bool animateFade = false, animatePanel = false;

    void Start()
    {
        gameObject.SetActive(true);
        fadePanel = GetComponent<Image>();
        targetAlpha = fadePanel.color.a;

        panelRect = textPanel.GetComponent<RectTransform>();
        targetRect = new Vector2(panelRect.rect.width, panelRect.rect.height);

        panelRect.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        if (!animateFade && !animatePanel) return;

        elapsedTime += Time.deltaTime;

        HandleFadeAnimation();
        HandlePanelAnimation();
    }

    private void HandleFadeAnimation()
    {
        if (!animateFade) return;

        if (startAlpha == 0 && !fadePanel.enabled)
        {
            fadePanel.enabled = !fadePanel.enabled;
        }

        float percentageComplete = elapsedTime / fadeTargetTime;

        float newAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.SmoothStep(0, 1, percentageComplete));
        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, newAlpha);
        if (percentageComplete >= 1)
        {
            animateFade = false;

            if (endAlpha == 0 && fadePanel.enabled)
            {
                fadePanel.enabled = !fadePanel.enabled;
            }
        }
    }

    private void HandlePanelAnimation()
    {
        if (!animatePanel) return;

        if (startRect == Vector2.zero && !textPanel.activeSelf)
        {
            textPanel.SetActive(!textPanel.activeSelf);
        }

        float percentageComplete = elapsedTime / panelTargetTime;

        panelRect.sizeDelta = Vector2.Lerp(startRect, endRect, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            animatePanel = false;

            if (endRect == Vector2.zero && textPanel.activeSelf)
            {
                textPanel.SetActive(!textPanel.activeSelf);
            }
        }
    }

    public void UpdateAnimation()
    {
        animateFade = !animateFade;
        animatePanel = !animatePanel;

        scrollbar.value = 1;

        if (!fadePanel.enabled)
        {
            startAlpha = 0;
            endAlpha = targetAlpha;

            startRect = Vector2.zero;
            endRect = targetRect;
        }
        else
        {
            startAlpha = targetAlpha;
            endAlpha = 0;

            startRect = targetRect;
            endRect = Vector2.zero;
        }

        elapsedTime = 0;
    }
}
