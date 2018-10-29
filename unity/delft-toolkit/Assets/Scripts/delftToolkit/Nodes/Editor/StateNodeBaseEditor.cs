using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(StateNodeBase))]
	public abstract class StateNodeBaseEditor : NodeEditor {

		private StateNodeBase node { get { return _node != null ? _node : _node = target as StateNodeBase; } }
		private StateNodeBase _node;
		protected abstract string description { get; }

		public override void OnHeaderGUI() {
			// Draw info icon
			GUI.color = new Color(1, 1, 1, 0.2f);
			GUI.Label(new Rect(GetWidth() - 22, 8, 14, 14), new GUIContent("i", description), DelftStyles.infoIcon);

			// Draw header name
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

		public override GUIStyle GetBodyStyle() {
			if (node.active) return DelftStyles.activeNodeBody;
			return base.GetBodyStyle();
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