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
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));

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
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sensorSource"));
			SerializedProperty msgFltrProperty = serializedObject.FindProperty("messageFilter");
			GUIContent msgFltrContent = new GUIContent(msgFltrProperty.displayName, msgFltrProperty.tooltip);
			EditorGUILayout.LabelField(msgFltrContent);
			msgFltrContent.text = "";
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("messageFilter"), msgFltrContent);
			EditorGUILayout.Separator();

			if (Application.isPlaying) {
				EditorGUILayout.BeginVertical("Box");
				EditorGUILayout.LabelField("Last signal", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Incoming Ding Message:");
				EditorGUI.indentLevel++;
				if (node.filteredSignal.isValid) EditorGUILayout.SelectableLabel(node.filteredSignal.device + ":" + node.filteredSignal.oscMessage + "\n" + node.filteredSignal.value.ToString());
				else EditorGUILayout.LabelField("--");
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
			Condition.ValueType valType = (Condition.ValueType) valueTypeProperty.enumValueIndex;

			EditorGUILayout.LabelField("Input value", EditorStyles.boldLabel);
			switch (valType) {
				case Condition.ValueType.String:
				string s;
				if (node.filteredSignal.isValid && node.filteredSignal.TryGetValue(out s)) EditorGUILayout.SelectableLabel(s);
				else EditorGUILayout.LabelField("--");
				break;

				case Condition.ValueType.X:
				case Condition.ValueType.Y:
				case Condition.ValueType.Z:
				case Condition.ValueType.Vector3:
				Vector3 v;
				if (node.filteredSignal.isValid && node.filteredSignal.TryGetValue(out v)) {
				float val = 0;
				if (valType == Condition.ValueType.X) val = v.x;
				else if (valType == Condition.ValueType.Y) val = v.y;
				else if (valType == Condition.ValueType.Z) val = v.z;
				else if (valType == Condition.ValueType.Vector3) val = v.magnitude;
				EditorGUILayout.Slider(val, 0f, 1023f);
				} else EditorGUILayout.LabelField("--");
				break;
			}

			EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
			switch (valType) {
				case Condition.ValueType.String:
					NodeEditorGUILayout.InstancePortList("stringConditions", typeof(StateNodeBase.Empty), serializedObject);
					break;
				case Condition.ValueType.X:
				case Condition.ValueType.Y:
				case Condition.ValueType.Z:
				case Condition.ValueType.Vector3:
					NodeEditorGUILayout.InstancePortList("floatConditions", typeof(StateNodeBase.Empty), serializedObject);
					break;
			}

			DrawFooterGUI();
		}
	}
}