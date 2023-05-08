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

    [Header("Animate Capybara Amount")]
    [SerializeField] private AnimateCapybaraAmount animateCapybaraAmount;
    int capAmount, amenityAmount;

    public void UpdateBalance(double balance)
    {
        balanceText.text = $"{balance:n2}";
    }

    public void UpdateNumCapybaras(int amount)
    {
        capAmount = amount;
        capybaraAmountText.text = amount.ToString();
        UpdateCapybaraAmount();
    }

    public void UpdateAmenityCapacity(int amount)
    {
        amenityAmount = amount;
        amenityCapacityText.text = amount.ToString();
        UpdateCapybaraAmount();
    }

    private void UpdateCapybaraAmount()
    {
        float percentage;
        if (amenityAmount == 0)
        {
            percentage = 1;
        }
        else
        {
            percentage = (float)capAmount / amenityAmount;
        }

        animateCapybaraAmount.UpdateBackgroundColor(percentage);
    }
}
