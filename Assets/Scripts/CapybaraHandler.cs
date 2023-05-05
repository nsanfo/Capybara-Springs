using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapybaraHandler : MonoBehaviour
{
    [Header("Information Handler")]
    public InformationHandler informationHandler;

    private List<GameObject> capybaras = new List<GameObject>();

    public void AddCapybara(GameObject capybara)
    {
        capybaras.Add(capybara);
        informationHandler.UpdateUINumCapybaras(capybaras.Count);
    }

    public void RemoveCapybara(GameObject capybara)
    {
        capybaras.Remove(capybara);
        informationHandler.UpdateUINumCapybaras(capybaras.Count);
    }

    public int CapybaraCount()
    {
        return capybaras.Count;
    }

    public float AverageHappiness()
    {
        if (capybaras.Count == 0)
            return 100f;

        float happiness = 0;
        for (int i = 0; i < capybaras.Count; i++)
        {
            CapybaraInfo capybaraInfo = capybaras[i].GetComponent<CapybaraInfo>();
            if (capybaraInfo == null) continue;

            happiness += capybaraInfo.happiness;
        }

        return happiness / capybaras.Count;
    }
}
