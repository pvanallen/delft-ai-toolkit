using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	[CustomPropertyDrawer(typeof(DingSignalFilter))]
	public class DingSignalFilterDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			Rect deviceRect = new Rect(position.x + position.width / 3, position.y, position.width / 3, EditorGUIUtility.singleLineHeight);
			Rect sourceRect = new Rect(deviceRect.x + deviceRect.width, position.y, deviceRect.width, EditorGUIUtility.singleLineHeight);
			Rect filterRect = new Rect(position.x + position.width / 3, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width / 3 * 2, EditorGUIUtility.singleLineHeight);

			// Draw fields - passs GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(deviceRect, property.FindPropertyRelative("device"), GUIContent.none);
			EditorGUI.PropertyField(sourceRect, property.FindPropertyRelative("source"), GUIContent.none);
			EditorGUI.PropertyField(filterRect, property.FindPropertyRelative("messageFilter"), GUIContent.none);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();

		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}