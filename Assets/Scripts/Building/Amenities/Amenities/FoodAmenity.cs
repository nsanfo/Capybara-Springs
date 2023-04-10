using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodAmenity : MonoBehaviour, AmenityInterface
{
    public AmenityEnum AmenityType { get; } = AmenityEnum.Food;
    public float insidePositioningMulti, insidePositioningRange, positioningMultiExtender;
    public float angleForSlots;
    public Color particleColor;
    public GameObject eatingObject = null;
}
