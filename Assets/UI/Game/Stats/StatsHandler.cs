using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsHandler : MonoBehaviour
{
    [Header("Stats Text")]
    public TextMeshProUGUI time, capybaraServed, maxHappiness, distanceWalked, amenities,
        onsens, foodPens, funPens, decor, pathLength, landsPurchased, moneyEarned, moneySpent;

    public void UpdateTimePlayed(float timePlayed)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timePlayed);

        

        time.text = new DateTime(timeSpan.Ticks).ToString("HH:mm:ss");
    }

    public void UpdateCapybaraServed()
    {

    }

    public void UpdateMaxHappiness()
    {

    }

    public void UpdateDistanceWalked()
    {

    }

    public void UpdateAmenityTotal()
    {

    }

    public void UpdateOnsenTotal()
    {

    }

    public void UpdateFoodPenTotal()
    {

    }

    public void UpdateFunPenTotal()
    {

    }

    public void UpdateDecorTotal()
    {

    }

    public void UpdatePathLength()
    {

    }

    public void UpdateLandsPurchased()
    {

    }

    public void UpdateMoneyEarned()
    {

    }

    public void UpdateMoneySpent()
    {

    }
}
