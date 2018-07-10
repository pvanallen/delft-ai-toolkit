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
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
			EditorGUILayout.LabelField("Test condition triggers when value > 50");
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("matchDingMessage"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sensorSource"));
			EditorGUILayout.LabelField("Incoming Ding Message:");
			string msg = node.incomingDingMessage;
			EditorGUI.indentLevel++;
			EditorGUILayout.SelectableLabel(msg);
			EditorGUI.indentLevel--;

			DrawFooterGUI();
		}
	}
}