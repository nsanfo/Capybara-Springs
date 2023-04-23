using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCollisionDetection : MonoBehaviour
{
    CapyAI aiScript;

    private void Start()
    {
        aiScript = this.gameObject.transform.parent.gameObject.GetComponent<CapyAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        aiScript.BodyCollisionEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        aiScript.BodyCollisionExit(other);
    }
}
