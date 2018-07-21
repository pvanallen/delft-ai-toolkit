using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Restart))]
	public class RestartEditor : StateNodeBaseEditor {

		private Restart node { get { return _node != null ? _node : _node = target as Restart; } }
		private Restart _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));
			DrawFooterGUI();
		}
	}
}