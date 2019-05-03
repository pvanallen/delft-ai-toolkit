using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateAssetMenu(fileName = "New DelftAIToolkit Graph", menuName = "Delft AI Toolkit Graph")]
	public class StateGraph : NodeGraph {

		/// <summary> Called when the graph exits </summary>
		public System.Action onExit;
		/// <summary> Is the graph currently running? </summary>
		public bool running { get; private set; }

		[ContextMenu("Run")]
		public void Run() {
			running = true;
			IEnumerable<Start> startNodes = nodes.FindAll(x => x is Start).Select(x => x as Start);
			foreach (Start startNode in startNodes) {
				startNode.Enter();
			}
		}

		/// <summary> Enumerate through all StatNodeBase nodes where active == true </summary>
		public IEnumerator<StateNodeBase> ActiveNodes() {
			for (int i = 0; i < nodes.Count; i++) {
				StateNodeBase node = nodes[i] as StateNodeBase;
				if (node != null && node.active) yield return node;
			}
		}

		[ContextMenu("Exit")]
		public void Exit() {
			running = false;
			if (onExit != null) onExit();
		}

		public void DrawGUI() {
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i] is ExposedValue) (nodes[i] as ExposedValue).DrawGUI();
			}
		}
	}
}