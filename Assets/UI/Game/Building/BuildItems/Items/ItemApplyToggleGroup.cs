using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemApplyToggleGroup : MonoBehaviour
{
    void Start()
    {
        GetComponent<Toggle>().group = transform.parent.GetComponent<ToggleGroup>();
    }
}
