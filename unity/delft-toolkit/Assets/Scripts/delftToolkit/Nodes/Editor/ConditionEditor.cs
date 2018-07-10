using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Condition))]
	public class ConditionEditor : StateNodeBaseEditor {

		private Condition node { get { return _node != null ? _node : _node = target as Condition; } }
		private Condition _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortPair(target.GetInputPort("enter"), target.GetOutputPort("exit"));

			Rect rect = GUILayoutUtility.GetLastRect();
			rect.x += (rect.width * 0.5f) - 50;
			rect.width = 100;
			node.device = (AiGlobals.Devices) EditorGUI.EnumPopup(rect, node.device);

			// Draw value port as slider

			// Get port
			NodePort port = node.GetInputPort("value");
			// Draw slider
			SerializedProperty valueTypeProperty = serializedObject.FindProperty("valueType");
			NodeEditorGUILayout.PropertyField(valueTypeProperty);
			SerializedProperty msgFltrProperty = serializedObject.FindProperty("messageFilter");
			GUIContent msgFltrContent = new GUIContent(msgFltrProperty.displayName, msgFltrProperty.tooltip);
			EditorGUILayout.LabelField(msgFltrContent);
			msgFltrContent.text = "";
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("messageFilter"), msgFltrContent);
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sensorSource"));
			EditorGUILayout.Separator();

			if (Application.isPlaying) {
				EditorGUILayout.BeginVertical("Box");
				EditorGUILayout.LabelField("Last signal (unfiltered)", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Incoming Ding Message:");
				EditorGUI.indentLevel++;
				if (node.unfilteredSignal.isValid) EditorGUILayout.SelectableLabel(node.unfilteredSignal.device + ":" + node.unfilteredSignal.oscMessage + "\n" + node.unfilteredSignal.value.ToString());
				else EditorGUILayout.LabelField("--");
				EditorGUI.indentLevel--;
				EditorGUILayout.LabelField("Last signal (filtered)", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Incoming Ding Message:");
				EditorGUI.indentLevel++;
				if (node.filteredSignal.isValid) EditorGUILayout.SelectableLabel(node.filteredSignal.device + ":" + node.filteredSignal.oscMessage + "\n" + node.filteredSignal.value.ToString());
				else EditorGUILayout.LabelField("--");
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);
			string valName = "n";
			if (Application.isPlaying && node.filteredSignal.isValid) {
				float val = node.GetSignalValue(node.filteredSignal);
				EditorGUILayout.Slider(val, 0, 1023);
				valName = val.ToString();
			} else {
				switch ((Condition.ValueType) valueTypeProperty.enumValueIndex) {
					case Condition.ValueType.X:
					case Condition.ValueType.Y:
					case Condition.ValueType.Z:
					valName = "n";
					break;
					case Condition.ValueType.Vector3:
					valName = "magnitude";
					break;
					case Condition.ValueType.String:
					valName = "char count";
					break;
				}
			}
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("conditionValue"), new GUIContent(valName + " >"));
			EditorGUILayout.EndVertical();

			DrawFooterGUI();
		}
	}
}