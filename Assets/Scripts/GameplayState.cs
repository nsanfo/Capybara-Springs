using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : MonoBehaviour
{
    public int currentCapacity = 0;

    public void AdjustCapacity(int capacity)
    {
        currentCapacity += capacity;
    }
}
