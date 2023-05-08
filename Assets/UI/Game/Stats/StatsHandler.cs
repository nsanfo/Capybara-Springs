using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StatsHandler : MonoBehaviour
{
    [Header("Stats Text")]
    public TextMeshProUGUI time, capybaraServed, maxHappiness, amenities,
        onsens, foodPens, funPens, decor, pathLength, plotsPurchased, moneyEarned, moneySpent;

    public void UpdateTimePlayed(float timePlayed)
    {
        time.text = new DateTime(TimeSpan.FromSeconds(timePlayed).Ticks).ToString("HH:mm:ss");
    }

    public void UpdateCapybaraServed(int served)
    {
        capybaraServed.text = served.ToString();
    }

    public void UpdateMaxHappiness(float happiness)
    {
        maxHappiness.text = Mathf.RoundToInt(happiness).ToString();
    }

    public void UpdateAmenityTotal(int numAmenities)
    {
        amenities.text = numAmenities.ToString();
    }

    public void UpdateOnsenTotal(int onsens)
    {
        this.onsens.text = onsens.ToString();
    }

    public void UpdateFoodPenTotal(int food)
    {
        foodPens.text = food.ToString();
    }

    public void UpdateFunPenTotal(int fun)
    {
        funPens.text = fun.ToString();
    }

    public void UpdateDecorTotal(int decor)
    {
        this.decor.text = decor.ToString();
    }

    public void UpdatePathLength(float distance)
    {
        pathLength.text = $"{distance:n2}";
    }

    public void UpdatePlotsPurchased(int plots)
    {
        plotsPurchased.text = plots.ToString();
    }

    public void UpdateMoneyEarned(double moneyEarned)
    {
        this.moneyEarned.text = "$" + $"{moneyEarned:n2}";
    }

    public void UpdateMoneySpent(double moneySpent)
    {
        this.moneySpent.text = "$" + $"{moneySpent:n2}";
    }
}
