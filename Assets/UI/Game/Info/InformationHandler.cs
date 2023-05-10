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
}
