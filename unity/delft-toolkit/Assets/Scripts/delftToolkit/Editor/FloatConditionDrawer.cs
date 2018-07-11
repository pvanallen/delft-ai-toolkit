using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	[CustomPropertyDrawer(typeof(Condition.FloatCondition))]
	public class FloatConditionDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();
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

			Condition.FloatCondition.CompareType compareType = (Condition.FloatCondition.CompareType) compareTypeProperty.enumValueIndex;
			Rect compareValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			SerializedProperty floatValAProperty = property.FindPropertyRelative("floatValA");
			SerializedProperty floatValBProperty = property.FindPropertyRelative("floatValB");
			float valA = floatValAProperty.floatValue;
			float valB = floatValBProperty.floatValue;

			switch (compareType) {
				case Condition.FloatCondition.CompareType.Gtr:
				case Condition.FloatCondition.CompareType.Lss:
				valA = EditorGUI.Slider(compareValueRect, valA, 0, 1023);
				break;

				case Condition.FloatCondition.CompareType.Range:
				EditorGUI.MinMaxSlider(compareValueRect, GUIContent.none, ref valA, ref valB, 0f, 1023f);
				break;
			}
			if (EditorGUI.EndChangeCheck()) {
				valB = Mathf.Max(valB, valA);
				floatValAProperty.floatValue = valA;
				floatValBProperty.floatValue = valB;
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}