using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(StringCondition))]
	public class StringConditionEditor : StateNodeBaseEditor {

		private StringCondition node { get { return _node != null ? _node : _node = target as StringCondition; } }
		private StringCondition _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));

			// Get port
			NodePort port = node.GetInputPort("value");
			SerializedProperty filterProperty = serializedObject.FindProperty("filter");
			GUIContent filterContent = new GUIContent(filterProperty.displayName, filterProperty.tooltip);
			NodeEditorGUILayout.PropertyField(filterProperty);
			EditorGUILayout.Separator();

			if (Application.isPlaying) {
				EditorGUILayout.BeginVertical("Box");
				EditorGUILayout.LabelField("Last signal", EditorStyles.boldLabel);
				if (node.signal.isValid) EditorGUILayout.SelectableLabel(node.signal.device + ":" + node.signal.oscMessage + "\n" + node.signal.value.ToString());
				else EditorGUILayout.LabelField("Missing or incorrect data type");
				EditorGUILayout.EndVertical();
			}

			// Input value
			EditorGUILayout.LabelField("Input value", EditorStyles.boldLabel);
			node.overrideValue = EditorGUILayout.Toggle("Override Value", node.overrideValue);
			EditorGUI.BeginDisabledGroup(!node.overrideValue);
			if (node.overrideValue) node.value = EditorGUILayout.TextField(node.value);
			else EditorGUILayout.TextField(node.value);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
			NodeEditorGUILayout.InstancePortList("conditions", typeof(StateNodeBase.Empty), serializedObject);

			DrawFooterGUI();
		}
	}

	[CustomPropertyDrawer(typeof(StringCondition.Condition))]
	public class StringConditionDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();
			Rect ifRect = new Rect(position.x, position.y, 30, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(ifRect, "if x");
			Rect inverseRect = new Rect((position.x + position.width) - 20, position.y, 30, EditorGUIUtility.singleLineHeight);
			Rect inverseLabelRect = new Rect((position.x + position.width) - 30, position.y, 10, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(inverseLabelRect, new GUIContent("!", "Inverse"));
			EditorGUI.PropertyField(inverseRect, property.FindPropertyRelative("inverse"), GUIContent.none);
			Rect compareTypeRect = new Rect(position.x + 30, position.y, position.width - 70, EditorGUIUtility.singleLineHeight);
			SerializedProperty compareTypeProperty = property.FindPropertyRelative("compareType");
			EditorGUI.PropertyField(compareTypeRect, compareTypeProperty, GUIContent.none);

			StringCondition.Condition.CompareType compareType = (StringCondition.Condition.CompareType) compareTypeProperty.enumValueIndex;
			Rect compareValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			SerializedProperty stringValProperty = property.FindPropertyRelative("strVal");
			EditorGUI.PropertyField(compareValueRect, stringValProperty, GUIContent.none);
			if (EditorGUI.EndChangeCheck()) {
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