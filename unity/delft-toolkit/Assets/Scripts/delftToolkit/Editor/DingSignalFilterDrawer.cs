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

			position = EditorGUI.IndentedRect(position);
			// Calculate rects
			Rect deviceRect = new Rect(position.x, position.y, position.width / 3, EditorGUIUtility.singleLineHeight);
			Rect sourceRect = new Rect(deviceRect.x + deviceRect.width, position.y, deviceRect.width, EditorGUIUtility.singleLineHeight);
			Rect portIconRect = new Rect(sourceRect.x + sourceRect.width + 3, position.y, 25, EditorGUIUtility.singleLineHeight);
			Rect portRect = new Rect(portIconRect.x + portIconRect.width, position.y, 52, EditorGUIUtility.singleLineHeight);
			Rect filterRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

			SerializedProperty strPort = property.FindPropertyRelative("port");

			SerializedProperty actionAnalogIn = property.FindPropertyRelative("analoginParams");
			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(deviceRect, property.FindPropertyRelative("device"), GUIContent.none);
			EditorGUI.PropertyField(sourceRect, property.FindPropertyRelative("source"), GUIContent.none);
			//DrawNextProperty(ref pos, actionAnalogIn, "port", 50, new GUIContent(DelftStyles.portIcon));
			EditorGUI.PropertyField(portIconRect, property.FindPropertyRelative("port"), new GUIContent(DelftStyles.portIcon));
			//EditorGUI.DrawNextProperty(portIconRect, property, "port", 50, new GUIContent(DelftStyles.portIcon));
			//EditorGUI.PropertyField(portRect, property.FindPropertyRelative("port"), new GUIContent(DelftStyles.portIcon));
			EditorGUI.PropertyField(portRect, property.FindPropertyRelative("port"), GUIContent.none);
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