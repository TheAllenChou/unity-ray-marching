/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

  Modified from project "MyBox" by Andrew Rumak.
   License : Copyright (C) 2018 Andrew Rumak.
             Distributed under the MIT License. See LICENSE file.
             https://github.com/Deadcows/MyBox
*/
/******************************************************************************/

using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
  public bool ShowRange { get { return Min != Max; } }

  public string PropertyToCheck;
  public object CompareValue;
  public object CompareValue2;
  public object CompareValue3;
  public object CompareValue4;
  public object CompareValue5;
  public object CompareValue6;
  public string Label;
  public string Tooltip;
  public float Min;
  public float Max;

  public ConditionalFieldAttribute
  (
    string propertyToCheck = null, 
    object compareValue = null,  
    object compareValue2 = null, 
    object compareValue3 = null, 
    object compareValue4 = null, 
    object compareValue5 = null, 
    object compareValue6 = null
  )
  {
    PropertyToCheck = propertyToCheck;
    CompareValue = compareValue;
    CompareValue2 = compareValue2;
    CompareValue3 = compareValue3;
    CompareValue4 = compareValue4;
    CompareValue5 = compareValue5;
    CompareValue6 = compareValue6;
    Label = "";
    Tooltip = "";
    Min = 0.0f;
    Max = 0.0f;
  }
}
