using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightCollisionDetection : MonoBehaviour
{
    CapyAI ai;

    private void Start()
    {
        ai = gameObject.transform.parent.GetComponent<CapyAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
        {
            ai.RightCollisionEnter();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
        {
            ai.RightCollisionExit();
        }
    }
}
