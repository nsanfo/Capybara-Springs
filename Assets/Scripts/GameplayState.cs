using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : MonoBehaviour
{
    private int currentCapacity = 0;

    public int GetCapacity()
    {
        return currentCapacity;
    }

    public void AdjustCapacity(int capacity)
    {
        currentCapacity += capacity;
    }
}
