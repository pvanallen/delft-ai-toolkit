using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(StateNode))]
	public class StateNodeEditor : NodeEditor {

		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			StateNode node = target as StateNode;
			StateGraph graph = node.graph as StateGraph;
			if (node.active) GUI.color = Color.green;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(40));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
			StateNode node = target as StateNode;
			StateGraph graph = node.graph as StateGraph;
			// Why do we wait 3 seconds here?
			if (GUILayout.Button("MoveNext Node")) Extensions.WaitAndDo(node.MoveNext, 3.0f).RunCoroutine();
			if (GUILayout.Button("Set as current")) node.active = true;;
		}
	}
}