using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnsenAmenity : MonoBehaviour, AmenityInterface
{
    public AmenityEnum AmenityType { get; } = AmenityEnum.Onsen;
    public float insideCenteringHeight, insidePositioningMulti, insidePositioningRange, splashHeight;
}
