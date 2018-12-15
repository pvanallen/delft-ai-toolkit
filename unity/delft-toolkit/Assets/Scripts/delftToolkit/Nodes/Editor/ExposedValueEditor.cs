using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(ExposedValue))]
	public class ExposedValueEditor : NodeEditor {

		public override void OnBodyGUI() {
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("value"), GUIContent.none);

			SerializedProperty automaticGUIProperty = serializedObject.FindProperty("automaticGUI");
			NodeEditorGUILayout.PropertyField(automaticGUIProperty);

			if (automaticGUIProperty.boolValue) {
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("anchorPoint"), GUIContent.none);
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("offset"));
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
				NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("draggable"));
			}
		}
	}
}