using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailsArrows : MonoBehaviour
{
    public CapybaraInfo info;
    public GameObject happinessArrow, hungerArrow, comfortArrow, funArrow;

    void Update()
    {
        if (info.HungerFilling)
            hungerArrow.SetActive(true);
        else
            hungerArrow.SetActive(false);

        if (info.ComfortFilling)
            comfortArrow.SetActive(true);
        else
            comfortArrow.SetActive(false);

        if (info.FunFilling)
            funArrow.SetActive(true);
        else
            funArrow.SetActive(false);

        if (info.HungerFilling || info.ComfortFilling || info.FunFilling)
            happinessArrow.SetActive(true);
        else
            happinessArrow.SetActive(false);
    }
}
