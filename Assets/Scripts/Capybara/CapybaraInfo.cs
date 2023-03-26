using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapybaraInfo : MonoBehaviour
{
    public string capyName;
    public float happiness, hunger = 75, comfort = 75, fun = 75;
    public bool HungerFilling { get; set; } = false;
    public bool ComfortFilling { get; set; } = false;
    public bool FunFilling { get; set; } = false;

    // Update is called once per frame
    void Update()
    {
        if (hunger < 0)
            hunger = 0;
        else
            hunger -= .02f * Time.deltaTime;

        if (comfort < 0)
            comfort = 0;
        else
            comfort -= .02f * Time.deltaTime;

        if (fun < 0) 
            fun = 0;
        else
            fun -= .02f * Time.deltaTime;

        happiness = (hunger + comfort + fun) / 3;
    }
}
