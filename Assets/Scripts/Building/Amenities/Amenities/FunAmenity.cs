using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunAmenity : MonoBehaviour, AmenityInterface
{
    public AmenityEnum AmenityType { get; } = AmenityEnum.Fun;
    public float insideCenteringHeight, insidePositioningMulti, insidePositioningRange, splashHeight;
}
