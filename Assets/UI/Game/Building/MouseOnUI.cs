using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOnUI : MonoBehaviour
{
    [Header("Event System")]
    public EventSystem eventSystem;

    private GraphicRaycaster raycaster;
    private PointerEventData eventData;

    public bool overUI = false;

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
    }

    void Update()
    {
        eventData = new PointerEventData(eventSystem);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        overUI = results.Count > 0;
    }
}
