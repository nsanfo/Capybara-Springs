using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimateBuildHover : MonoBehaviour
{
    [Header("Event System")]
    public EventSystem eventSystem;

    [Header("Build Hover Panel")]
    public GameObject hoverPanel;
    private Vector3 originalHoverPosition;

    private GraphicRaycaster raycaster;
    private PointerEventData eventData;

    readonly private float targetTime = 0.3f;
    private float elapsedTime;
    private bool animate = false;
    private GameObject currentPanel, targetPanel;

    // Start is called before the first frame update
    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        originalHoverPosition = hoverPanel.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (animate) HandleAnimation();
    }

    private void HandleAnimation()
    {
        CheckForPanel();
        if (targetPanel == null) return;

        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / targetTime;

        hoverPanel.transform.position = Vector3.Lerp(hoverPanel.transform.position, targetPanel.transform.position, Mathf.SmoothStep(0, 1, percentageComplete));
        if (percentageComplete >= 1)
        {
            elapsedTime = 0;
            targetPanel = null;
        }
    }

    private void CheckForPanel()
    {
        eventData = new PointerEventData(eventSystem);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        if (results.Count > 0)
        {
            if (currentPanel == null || currentPanel != results[0].gameObject)
            {
                if (!hoverPanel.activeSelf) hoverPanel.SetActive(true);
                currentPanel = results[0].gameObject;
                targetPanel = currentPanel;
                elapsedTime = 0;
            }
        }
        else
        {
            GameObject selectedObject = transform.parent.GetComponent<BuildToggleHandler>().GetSelectionLocation();
            if (selectedObject != null)
            {
                if (targetPanel == null && hoverPanel.transform.position != selectedObject.transform.position) 
                {
                    targetPanel = selectedObject;
                    elapsedTime = 0;
                }
            }
            else
            {
                if (targetPanel == null)
                {
                    elapsedTime = 0;
                }
                
            }

            currentPanel = null;
        }
    }

    public void UpdateAnimation(bool toggleState)
    {
        animate = toggleState;
    }

    public void UpdateHoverPanelLocation()
    {
        hoverPanel.transform.position = originalHoverPosition;
    }
}
