using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontCollisionDetection : MonoBehaviour
{
    CapyAI ai;

    private void Start()
    {
        ai = gameObject.transform.parent.GetComponent<CapyAI>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Capybara" || (other.gameObject.tag == "Front" && (Mathf.Abs(other.transform.TransformDirection(Vector3.forward).y - transform.TransformDirection(Vector3.forward).y) >= 170f)))
        {
            ai.FrontCollisionEnter(other);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Capybara" || (other.gameObject.tag == "Front" && (Mathf.Abs(other.transform.TransformDirection(Vector3.forward).y - transform.TransformDirection(Vector3.forward).y) >= 170f)))
        {
            ai.FrontCollisionExit(other);
        }
    }
}
