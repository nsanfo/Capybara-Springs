using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    public GameObject indicator;
    public void activateSelection()
    {
        indicator.SetActive(true);
    }

    public void deactivateSelection()
    {
        indicator.SetActive(false);
    }
}
