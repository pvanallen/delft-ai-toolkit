using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	[CustomPropertyDrawer(typeof(Condition.StringCondition))]
	public class StringConditionDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.HelpBox(position, "", MessageType.None);
			Rect ifRect = new Rect(position.x, position.y, 30, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(ifRect, "if x");
			Rect inverseRect = new Rect((position.x + position.width) - 20, position.y, 30, EditorGUIUtility.singleLineHeight);
			Rect inverseLabelRect = new Rect((position.x + position.width) - 30, position.y, 10, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(inverseLabelRect, new GUIContent("!", "Inverse"));
			EditorGUI.PropertyField(inverseRect, property.FindPropertyRelative("inverse"), GUIContent.none);
			Rect compareTypeRect = new Rect(position.x + 30, position.y, position.width - 70, EditorGUIUtility.singleLineHeight);
			SerializedProperty compareTypeProperty = property.FindPropertyRelative("compareType");
			EditorGUI.PropertyField(compareTypeRect, compareTypeProperty, GUIContent.none);

			Condition.StringCondition.CompareType compareType = (Condition.StringCondition.CompareType) compareTypeProperty.enumValueIndex;
			Rect compareValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			switch (compareType) {
				case Condition.StringCondition.CompareType.Contains:
				case Condition.StringCondition.CompareType.EndsWith:
				case Condition.StringCondition.CompareType.StartsWith:
				case Condition.StringCondition.CompareType.Equals:
				EditorGUI.PropertyField(compareValueRect, property.FindPropertyRelative("stringVal"), GUIContent.none);
				break;
				case Condition.StringCondition.CompareType.Length:
				EditorGUI.PropertyField(compareValueRect, property.FindPropertyRelative("intVal"), GUIContent.none);
				break;
			}

		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}