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

#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldAttributeDrawer : PropertyDrawer
{
  public static string AsStringValue(SerializedProperty property)
  {
    switch (property.propertyType)
    {
      case SerializedPropertyType.String:
        return property.stringValue;

      case SerializedPropertyType.Character:
      case SerializedPropertyType.Integer:
        if (property.type == "char") return System.Convert.ToChar(property.intValue).ToString();
        return property.intValue.ToString();

      case SerializedPropertyType.ObjectReference:
        return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";

      case SerializedPropertyType.Boolean:
        return property.boolValue.ToString();

      case SerializedPropertyType.Enum:
        return property.enumNames[property.enumValueIndex];

      default:
        return string.Empty;
    }
  }

  private ConditionalFieldAttribute Attribute
  {
    get
    {
      return _attribute ?? (_attribute = attribute as ConditionalFieldAttribute);
    }
  }

  private string PropertyToCheck { get { return Attribute != null ? _attribute.PropertyToCheck : null; } }
  private object CompareValue  { get { return Attribute != null ? _attribute.CompareValue  : null; } }
  private object CompareValue2 { get { return Attribute != null ? _attribute.CompareValue2 : null; } }
  private object CompareValue3 { get { return Attribute != null ? _attribute.CompareValue3 : null; } }
  private object CompareValue4 { get { return Attribute != null ? _attribute.CompareValue4 : null; } }
  private object CompareValue5 { get { return Attribute != null ? _attribute.CompareValue5 : null; } }
  private object CompareValue6 { get { return Attribute != null ? _attribute.CompareValue6 : null; } }

  private ConditionalFieldAttribute _attribute;

  private bool ShouldShow(SerializedProperty property)
  {
    if (PropertyToCheck != null && !PropertyToCheck.Equals(""))
    {
      var conditionProperty = FindPropertyRelative(property, PropertyToCheck);
      if (conditionProperty != null)
      {

        var aCompVal = new object[]
        {
          CompareValue,
          CompareValue2,
          CompareValue3,
          CompareValue4,
          CompareValue5,
          CompareValue6,
        };

        bool matched = false;
        foreach (object compVal in aCompVal)
        {
          if (compVal == null)
            continue;

          bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;
          string compareStringValue = compVal != null ? compVal.ToString().ToUpper() : "NULL";
          if (isBoolMatch && compareStringValue == "FALSE") isBoolMatch = false;

          string conditionPropertyStringValue = AsStringValue(conditionProperty).ToUpper();
          bool objectMatch = compareStringValue == conditionPropertyStringValue;

          if (!isBoolMatch && !objectMatch)
            continue;

          matched = true;
          break;
        }

        if (!matched)
        {
          return false;
        }
      }
    }

    return true;
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return 
      ShouldShow(property) 
        ? EditorGUI.GetPropertyHeight(property) 
        : 0.0f;
  }

  // TODO: Skip array fields
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent content)
  {
    if (!ShouldShow(property))
      return;

    if (!Attribute.Label.Equals(""))
      content.text = Attribute.Label;

    if (!Attribute.Tooltip.Equals(""))
      content.tooltip = Attribute.Tooltip;

    if (Attribute.ShowRange)
    {
      if (property.propertyType == SerializedPropertyType.Float)
        EditorGUI.Slider(position, property, Attribute.Min, Attribute.Max, content);
      else if (property.propertyType == SerializedPropertyType.Integer)
        EditorGUI.IntSlider(position, property, Convert.ToInt32(Attribute.Min), Convert.ToInt32(Attribute.Max), content);
      else
        EditorGUI.LabelField(position, content.text, "Use Range with float or int.");
    }
    else
    {
      EditorGUI.PropertyField(position, property, content);
    }
  }

  private SerializedProperty FindPropertyRelative(SerializedProperty property, string toGet)
  {
    if (property.depth == 0) return property.serializedObject.FindProperty(toGet);

    var path = property.propertyPath.Replace(".Array.data[", "[");
    var elements = path.Split('.');
    SerializedProperty parent = null;

    for (int i = 0; i < elements.Length - 1; i++)
    {
      var element = elements[i];
      int index = -1;
      if (element.Contains("["))
      {
        index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
        element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
      }

      parent = i == 0 ?
        property.serializedObject.FindProperty(element) :
        parent.FindPropertyRelative(element);

      if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
    }

    return parent.FindPropertyRelative(toGet);
  }
}

#endif
