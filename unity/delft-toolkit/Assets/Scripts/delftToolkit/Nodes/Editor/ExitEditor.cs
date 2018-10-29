using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Exit))]
	public class ExitEditor : StateNodeBaseEditor {

		protected override string description { get { return "Exits the graph"; } }
		private Exit node { get { return _node != null ? _node : _node = target as Exit; } }
		private Exit _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));
			DrawFooterGUI();
		}
	}
}