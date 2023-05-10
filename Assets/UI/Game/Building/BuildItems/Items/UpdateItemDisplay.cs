using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.UI;

public class UpdateItemDisplay : MonoBehaviour
{
    public TextMeshProUGUI textGUI;
    public RawImage rawImage;

    public void UpdateText(string text)
    {
        textGUI.text = text;
    }

    public void UpdateImage(Texture2D texture)
    {
        rawImage.texture = texture;
    }
}
