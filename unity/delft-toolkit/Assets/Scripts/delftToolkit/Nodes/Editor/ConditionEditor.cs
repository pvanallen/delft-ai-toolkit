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

			// Draw value port as slider

			// Get port
			NodePort port = node.GetInputPort("value");
			// Draw slider
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("value"));
			EditorGUILayout.LabelField("Test condition triggers when value > 50");
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sensorDevice"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("matchDingMessage"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sensorSource"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("incomingDingMessage"));

			DrawFooterGUI();
		}
	}
}