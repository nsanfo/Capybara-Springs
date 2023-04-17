using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapybaraPlacer : MonoBehaviour
{
    public int Collisions { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            Collisions++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Capybara")
            Collisions--;
    }
}
