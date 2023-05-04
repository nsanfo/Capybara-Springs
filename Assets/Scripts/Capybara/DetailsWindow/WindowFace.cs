using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowFace : MonoBehaviour
{
    public CapybaraInfo info;
    private GameObject veryHappyFace, happyFace, neutralFace, sadFace, currentFace, newFace;

    private void Start()
    {
        veryHappyFace = transform.GetChild(10).gameObject;
        happyFace = transform.GetChild(11).gameObject;
        neutralFace = transform.GetChild(12).gameObject;
        sadFace = transform.GetChild(13).gameObject;
        currentFace = SetFace();
    }

    private GameObject SetFace()
    {
        if (info.happiness >= 90)
        {
            veryHappyFace.SetActive(true);
            return veryHappyFace;
        }
        else if (info.happiness >= 80)
        {
            happyFace.SetActive(true);
            return happyFace;
        }
        else if (info.happiness >= 50)
        {
            neutralFace.SetActive(true);
            return neutralFace;
        }
        else
        {
            sadFace.SetActive(true);
            return sadFace;
        }
    }

    void Update()
    {
        newFace = SetFace();
        if (currentFace != newFace)
        {
            currentFace.SetActive(false);
            newFace.SetActive(true);
        }
    }
}
