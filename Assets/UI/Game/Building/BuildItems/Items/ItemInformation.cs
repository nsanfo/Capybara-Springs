using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInformation : MonoBehaviour
{
    public double cost = 0;
    public string itemName;

    public string GetFormattedCost()
    {
        return "$" + $"{cost:n0}";
    }
}
