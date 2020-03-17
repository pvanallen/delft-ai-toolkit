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

			Rect pos = new Rect(position.x, position.y, 0, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(pos, property, new GUIContent());
			pos.x += pos.width + 2;
			EditorGUIUtility.labelWidth = 20;

			// Draw label
			EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			SerializedProperty strPort = property.FindPropertyRelative("port");

			SerializedProperty actionAnalogIn = property.FindPropertyRelative("analoginParams");
			// Don't make child fields be indented
			// var indent = EditorGUI.indentLevel;
			// EditorGUI.indentLevel = 0;

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			DrawNextProperty(ref pos, property, "device", 50, GUIContent.none);
			DrawNextProperty(ref pos, property, "source", 50, GUIContent.none);
			DrawNextProperty(ref pos, property, "port", 50, new GUIContent(DelftStyles.portIcon));
			NextLine(ref pos);
			DrawNextProperty(ref pos, property, "messageFilter", position.width, GUIContent.none);

			// Set indent back to what it was
			// EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();

		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
		}
		private void DrawNextProperty(ref Rect pos, SerializedProperty property, string relative, float width, GUIContent content) {
			SerializedProperty relativeProp = property.FindPropertyRelative(relative);
			if (relativeProp == null) {
				Debug.LogWarning("Relative property not found (" + relative + ")");
				return;
			}
			pos.width = width;
			EditorGUI.PropertyField(pos, relativeProp, content);
			pos.x += pos.width + 2;
		}

		private void NextLine(ref Rect pos) {
			pos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			pos.x = 18;
		}
	}
	
}