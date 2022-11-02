using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapybaraInfo : MonoBehaviour
{
    public string capyName;
    public float happiness, hunger = 75, comfort = 75, fun = 75;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hunger -= .01f * Time.deltaTime;
        comfort -= .01f * Time.deltaTime;
        fun -= .01f * Time.deltaTime;
        happiness = (hunger + comfort + fun) / 3;
    }
}
