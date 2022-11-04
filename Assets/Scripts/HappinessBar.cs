using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HappinessBar : MonoBehaviour
{
    public Slider slider;
    public CapybaraInfo info;
    float fill;

    void Update()
    {
        fill = info.happiness / 100;
        slider.value = fill;
    }
}