using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : MonoBehaviour
{
    [Header("Information Handler")]
    public InformationHandler informationHandler;

    private int currentCapacity = 0;

    public int GetCapacity()
    {
        return currentCapacity;
    }

    public void AdjustCapacity(int capacity)
    {
        currentCapacity += capacity;
        informationHandler.UpdateUIAmenityCapacity(currentCapacity);
    }
}
