using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(StateNodeBase))]
	public class StateNodeBaseEditor : NodeEditor {

		private StateNodeBase node { get { return _node != null ? _node : _node = target as StateNodeBase; } }
		private StateNodeBase _node;

		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			StateNodeBase node = target as StateNodeBase;
			if (node.active) GUI.color = Color.green;
			base.OnHeaderGUI();
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortPair(target.GetInputPort("enter"), target.GetOutputPort("exit"));
			DrawFooterGUI();
		}

		public void DrawFooterGUI() {
			if (node.active) {
				if (GUILayout.Button("Continue")) {
					node.Exit();
				}
			} else {
				if (GUILayout.Button("Trigger")) {
					node.Enter();
				}
			}
		}
	}
}