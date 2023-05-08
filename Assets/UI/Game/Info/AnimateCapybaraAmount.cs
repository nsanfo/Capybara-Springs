using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateCapybaraAmount : MonoBehaviour
{
    public Image amountImage;
    public Image capybaraIcon;

    public Gradient gradient;

    public Sprite happy, neutral, sad;

    public void UpdateBackgroundColor(float percent)
    {
        amountImage.color = gradient.Evaluate(percent);

        if (percent > 0.8)
        {
            capybaraIcon.sprite = sad;
        }
        else if (percent > 0.6)
        {
            capybaraIcon.sprite = neutral;
        }
        else
        {
            capybaraIcon.sprite = happy;
        }
    }
}
