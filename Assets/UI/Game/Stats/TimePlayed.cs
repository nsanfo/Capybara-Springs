using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePlayed : MonoBehaviour
{
    private float timePlayed, elapsedTime, updateTime = 1;

    [Header("Stats Handler")]
    public StatsHandler statsHandler;

    void Update()
    {
        timePlayed += Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (elapsedTime > updateTime)
        {
            elapsedTime = 0;
            statsHandler.UpdateTimePlayed(timePlayed);
        }
    }
}
