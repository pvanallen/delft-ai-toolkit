using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Start))]
	public class StartEditor : StateNodeBaseEditor {

		protected override string description { get { return "Sends an initial signal when the graph starts playback"; } }
		private Start node { get { return _node != null ? _node : _node = target as Start; } }
		private Start _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetOutputPort("exit"));
			DrawFooterGUI();
		}
	}
}