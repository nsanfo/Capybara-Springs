using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailsWindowPosition : MonoBehaviour
{
    public GameObject cam;
    public GameObject target;
    public Vector3 posOffset = new Vector3(0, 0, 0);
    Vector3 newLocation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var rect = GetComponent<RectTransform>();
        posOffset.y = 0.6f * cam.transform.position.y;
        newLocation = Camera.main.WorldToScreenPoint(target.transform.position + posOffset);
        if (newLocation.y > Screen.height - (rect.sizeDelta.y / 2))
        {
            newLocation.y = Screen.height - (rect.sizeDelta.y / 2);
        }
        else if (newLocation.y < (rect.sizeDelta.y / 2))
        {
            newLocation.y = rect.sizeDelta.y / 2;
        }
        if (newLocation.x > Screen.width - (rect.sizeDelta.x / 2))
        {
            newLocation.x = Screen.width - (rect.sizeDelta.x / 2);
        }
        else if (newLocation.x < (rect.sizeDelta.x / 2))
        {
            newLocation.x = rect.sizeDelta.x / 2;
        }
        transform.position = newLocation;
    }
}