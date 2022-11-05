using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateBuildUI
{
    public static void AnimateSelectTypeButton(Button[] buildTypeButtons, string typeButtonName, string animationName)
    {
        for (int i = 0; i < buildTypeButtons.Length; i++)
        {
            if (buildTypeButtons[i].gameObject.name == typeButtonName)
            {
                buildTypeButtons[i].GetComponent<Animator>().Play(animationName);
                return;
            }
        }
    }

    public static IEnumerator[] AnimateShowTypeButtons(Button[] buildTypeButtons, bool canBuild)
    {
        List<IEnumerator> enumerators = new List<IEnumerator>();
        float delay = 0.0f;
        if (canBuild)
        {
            for (int i = buildTypeButtons.Length; i > 0; i--)
            {
                enumerators.Add(TypeButtonAnimations(delay, buildTypeButtons[i - 1].GetComponent<Animator>(), "ShowBuildTypeButton"));
                delay += 0.05f;
            }
        }
        else
        {
            for (int i = 0; i < buildTypeButtons.Length; i++)
            {
                enumerators.Add(TypeButtonAnimations(delay, buildTypeButtons[i].GetComponent<Animator>(), "HideBuildTypeButton"));
                delay += 0.05f;
            }
        }

        return enumerators.ToArray();
    }

    static IEnumerator TypeButtonAnimations(float timeInSec, Animator animatorComponent, string animationName)
    {
        yield return new WaitForSeconds(timeInSec);
        animatorComponent.Play(animationName);
    }

    public static void AnimateBuildButton(CanvasRenderer buildPanel, TMPro.TextMeshProUGUI buildButtonText, bool canBuild)
    {
        if (canBuild)
        {
            buildPanel.GetComponent<Animator>().Play("BuildButtonRed");
            buildButtonText.text = "Cancel Build";
        }
        else
        {
            buildPanel.GetComponent<Animator>().Play("BuildButtonNormal");
            buildButtonText.text = "Build";
        }
    }

    public static void AnimateBuildTip(CanvasRenderer helpTextPanel, bool canBuild)
    {
        if (canBuild)
        {
            helpTextPanel.GetComponent<Animator>().Play("ShowBuildTip");
        }
        else
        {
            helpTextPanel.GetComponent<Animator>().Play("HideBuildTip");
        }
    }
}
