using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "delft Toolkit")]
	public class StateGraph : NodeGraph {

		[ContextMenu("Run")]
		public void Run() {
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
	}
}