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
        hunger -= .02f * Time.deltaTime;
        comfort -= .02f * Time.deltaTime;
        fun -= .02f * Time.deltaTime;
        happiness = (hunger + comfort + fun) / 3;
    }
}
