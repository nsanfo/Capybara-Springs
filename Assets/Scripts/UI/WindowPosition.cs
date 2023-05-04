using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPosition : MonoBehaviour
{
    public GameObject cam;
    public GameObject target;
    Vector3 posOffset = new Vector3(0, 0, 0);
    Vector3 newLocation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var rect = GetComponent<RectTransform>();
        posOffset.y = 0.6f * cam.transform.position.y;
        newLocation = Camera.main.WorldToScreenPoint(target.transform.position + posOffset);
        if (newLocation.y > Screen.height - ((rect.sizeDelta.y * rect.localScale.x) / 2))
        {
            newLocation.y = Screen.height - (((rect.sizeDelta.y + 70) * rect.localScale.x) / 2);
        }
        else if (newLocation.y < ((rect.sizeDelta.y * rect.localScale.x) / 2))
        {
            newLocation.y = (rect.sizeDelta.y * rect.localScale.x) / 2;
        }
        if (newLocation.x > Screen.width - ((rect.sizeDelta.x * rect.localScale.x) / 2))
        {
            newLocation.x = Screen.width - ((rect.sizeDelta.x * rect.localScale.x) / 2);
        }
        else if (newLocation.x < ((rect.sizeDelta.x * rect.localScale.x) / 2))
        {
            newLocation.x = (rect.sizeDelta.x * rect.localScale.x) / 2;
        }
        transform.position = newLocation;
    }
}