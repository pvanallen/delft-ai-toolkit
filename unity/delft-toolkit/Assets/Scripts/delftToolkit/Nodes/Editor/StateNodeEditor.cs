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
			if (graph.current == node) GUI.color = Color.green;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(40));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
			StateNode node = target as StateNode;
			StateGraph graph = node.graph as StateGraph;
			if (GUILayout.Button ("MoveNext Node")) GoNext(node, 3.0f).RunCoroutine ();
			if (GUILayout.Button("Continue Graph")) graph.Continue();
			if (GUILayout.Button("Set as current")) graph.current = node;
		}

		public IEnumerator GoNext(StateNode node, float delay ) {
			node.Finish (delay).RunCoroutine ();
			yield return new WaitForSeconds(delay+0.01f);
			NodeEditorWindow.current.Repaint ();
		}
	}
}