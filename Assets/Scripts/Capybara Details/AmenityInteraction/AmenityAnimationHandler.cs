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
                AmenityAnimNames.SmallOnsen,
                0.7f,
                0.04f
                ),
            new AmenityAnimationData(
                AmenityNames.MediumOnsenLvl1,
                AmenityAnimNames.MediumOnsen,
                0.9f,
                0.14f
                ),
            new AmenityAnimationData(
                AmenityNames.LargeOnsenLvl1,
                AmenityAnimNames.LargeOnsen,
                0.9f,
                1f
                ),
        };

        foreach (var animData in animationDataList)
            amenityMap.Add(animData.name, animData);
    }

    public AmenityAnimationData GetAnimationData(GameObject gameObject)
    {
        // Get animation name
        AmenityNames foundName = AmenityNames.None;
        AmenityAnimNames foundAnim = AmenityAnimNames.None;
        foreach (AmenityNames amenityName in System.Enum.GetValues(typeof(AmenityNames)))
        {
            if (gameObject.name.Contains(amenityName.ToString()))
            {
                foundName = amenityName;
                foundAnim = GetAnimationFromName(amenityName);
                if (foundAnim == AmenityAnimNames.None)
                    continue;
                else
                    break;
            }
        }

        if (foundName == AmenityNames.None || foundAnim == AmenityAnimNames.None)
            return null;

        if (amenityMap.ContainsKey(foundName))
            return amenityMap[foundName];
        else
            return null;
    }

    private AmenityAnimNames GetAnimationFromName(AmenityNames amenityName)
    {
        foreach (AmenityAnimNames amenityAnim in System.Enum.GetValues(typeof(AmenityAnimNames)))
        {
            if (amenityName.ToString().Contains(amenityAnim.ToString()))
            {
                return amenityAnim;
            }
        }

        return AmenityAnimNames.None;
    }
}

public class AmenityAnimationData
{
    public readonly AmenityNames name;
    public readonly AmenityAnimNames animation;
    public readonly float forwardMultiplier;
    public readonly float enteredCenteringHeight;

    public AmenityAnimationData(AmenityNames name, AmenityAnimNames animation, float forwardMultiplier, float enteredCenteringHeight)
    {
        this.name = name;
        this.animation = animation;
        this.forwardMultiplier = forwardMultiplier;
        this.enteredCenteringHeight = enteredCenteringHeight;
    }
}

public enum AmenityNames
{
    None, SmallOnsenLvl1, MediumOnsenLvl1, LargeOnsenLvl1
}

public enum AmenityAnimNames
{
    None, SmallOnsen, MediumOnsen, LargeOnsen
}
