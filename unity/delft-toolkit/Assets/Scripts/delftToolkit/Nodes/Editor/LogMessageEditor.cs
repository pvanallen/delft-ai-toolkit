using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(LogMessage))]
	public class LogMessageEditor : StateNodeBaseEditor {

		protected override string description { get { return "Logs a message to the console"; } }
		private LogMessage node { get { return _node != null ? _node : _node = target as LogMessage; } }
		private LogMessage _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("message"), GUIContent.none);
			DrawFooterGUI();
		}
	}
}