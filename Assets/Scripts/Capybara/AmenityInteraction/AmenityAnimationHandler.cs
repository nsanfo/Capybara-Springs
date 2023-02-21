using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmenityAnimationHandler
{
    private static AmenityAnimationHandler instance;
    Dictionary<AmenityNames, AmenityAnimationData> amenityMap = new Dictionary<AmenityNames, AmenityAnimationData>();

    public static AmenityAnimationHandler GetInstance()
    {
        if (instance == null)
            instance = new AmenityAnimationHandler();

        return instance;
    }

    public AmenityAnimationHandler()
    {
        List<AmenityAnimationData> animationDataList = new List<AmenityAnimationData>()
        {
            new AmenityAnimationData(
                AmenityNames.SmallOnsenLvl1,
                0.7f,
                0.04f
                ),
            new AmenityAnimationData(
                AmenityNames.MediumOnsenLvl1,
                0.9f,
                0.14f
                ),
            new AmenityAnimationData(
                AmenityNames.LargeOnsenLvl1,
                0.9f,
                0.43f
                ),
        };

        foreach (var animData in animationDataList)
            amenityMap.Add(animData.name, animData);
    }

    public AmenityAnimationData GetAnimationData(GameObject gameObject)
    {
        // Get animation name
        AmenityNames foundName = AmenityNames.None;
        foreach (AmenityNames amenityName in System.Enum.GetValues(typeof(AmenityNames)))
        {
            if (gameObject.name.Contains(amenityName.ToString()))
            {
                foundName = amenityName;
                break;
            }
        }

        if (foundName == AmenityNames.None)
            return null;

        if (amenityMap.ContainsKey(foundName))
            return amenityMap[foundName];
        else
            return null;
    }
}

public class AmenityAnimationData
{
    public readonly AmenityNames name;
    public readonly float forwardMultiplier;
    public readonly float enteredCenteringHeight;

    public AmenityAnimationData(AmenityNames name, float forwardMultiplier, float enteredCenteringHeight)
    {
        this.name = name;
        this.forwardMultiplier = forwardMultiplier;
        this.enteredCenteringHeight = enteredCenteringHeight;
    }
}

public enum AmenityNames
{
    None, SmallOnsenLvl1, MediumOnsenLvl1, LargeOnsenLvl1
}
