using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationHandler : MonoBehaviour
{
    [Header("Balance Text")]
    [SerializeField] private TextMeshProUGUI balanceText;

    [Header("Capybara Amount Text")]
    [SerializeField] private TextMeshProUGUI capybaraAmountText;

    [Header("Amenity Capacity Text")]
    [SerializeField] private TextMeshProUGUI amenityCapacityText;

    [Header("Capybara Happiness Image")]
    [SerializeField] private Image capybaraHappinessImage;
    [SerializeField] private Sprite capyRelaxed, capyHappy, capyNeutral, capySad;

    public void UpdateUIBalance(double balance)
    {
        balanceText.text = $"{balance:n2}";
    }

    public void UpdateUINumCapybaras(int amount)
    {
        capybaraAmountText.text = amount.ToString();
    }

    public void UpdateUIAmenityCapacity(int amount)
    {
        amenityCapacityText.text = amount.ToString();
    }

    public void UpdateUIAverageHappiness(float happiness)
    {
        if (happiness > 90)
        {
            if (capybaraHappinessImage.sprite == capyRelaxed) return;
            capybaraHappinessImage.sprite = capyRelaxed;
        }
        else if (happiness > 70)
        {
            if (capybaraHappinessImage.sprite == capyHappy) return;
            capybaraHappinessImage.sprite = capyHappy;
        }
        else if (happiness > 40)
        {
            if (capybaraHappinessImage.sprite == capyNeutral) return;
            capybaraHappinessImage.sprite = capyNeutral;
        }
        else
        {
            if (capybaraHappinessImage.sprite == capySad) return;
            capybaraHappinessImage.sprite = capySad;
        }
    }
}
