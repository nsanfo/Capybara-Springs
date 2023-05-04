using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveEntranceControllers : MonoBehaviour
{
    void Start()
    {
        Transform entranceTransform = transform.Find("EntranceControllers");
        if (entranceTransform != null)
        {
            Destroy(entranceTransform.gameObject);
        }
    }
}
