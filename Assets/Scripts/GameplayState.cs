using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : MonoBehaviour
{
    [Header("Information Handler")]
    public InformationHandler informationHandler;

    [Header("Stats Handler")]
    public StatsHandler statsHandler;

    private double moneyEarned = 0, moneySpent = 0;
    private float maxHappiness = 0, distanceWalked = 0, pathDistance = 0;
    private int currentCapacity = 0, capybarasServed = 0, numAmenities = 0, numOnsens = 0, numFood = 0, numFun = 0, numDecor = 0, plotsPurchased = 0;

    public int GetCapacity()
    {
        return currentCapacity;
    }

    public void AdjustCapacity(int capacity)
    {
        currentCapacity += capacity;
        informationHandler.UpdateAmenityCapacity(currentCapacity);
    }

    public void AdjustCapybarasServed()
    {
        capybarasServed++;
        statsHandler.UpdateCapybaraServed(capybarasServed);
    }

    public void AdjustMaxHappiness(float happiness)
    {
        if (happiness > maxHappiness)
        {
            maxHappiness = happiness;
            statsHandler.UpdateMaxHappiness(happiness);
        }
    }

    public void AdjustOnsenAmount()
    {
        numAmenities++;
        numOnsens++;
        statsHandler.UpdateAmenityTotal(numAmenities);
        statsHandler.UpdateOnsenTotal(numOnsens);
    }

    public void AdjustFoodAmount()
    {
        numAmenities++;
        numFood++;
        statsHandler.UpdateAmenityTotal(numAmenities);
        statsHandler.UpdateFoodPenTotal(numFood);
    }

    public void AdjustFunAmount()
    {
        numAmenities++;
        numFun++;
        statsHandler.UpdateAmenityTotal(numAmenities);
        statsHandler.UpdateFunPenTotal(numFun);
    }

    public void AdjustDecorAmount()
    {
        numDecor++;
        statsHandler.UpdateDecorTotal(numDecor);
    }

    public void AdjustPathDistance(float distance)
    {
        pathDistance += distance;
        statsHandler.UpdatePathLength(pathDistance);
    }

    public void AdjustPlotsPurchased()
    {
        plotsPurchased++;
        statsHandler.UpdatePlotsPurchased(plotsPurchased);
    }

    public void AdjustMoneyEarned(double earnedMoney)
    {
        moneyEarned += earnedMoney;
        statsHandler.UpdateMoneyEarned(moneyEarned);
    }

    public void AdjustMoneySpent(double spentMoney)
    {
        moneySpent += spentMoney;
        statsHandler.UpdateMoneySpent(moneySpent);
    }
}
