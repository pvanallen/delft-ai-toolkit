using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "delft Toolkit")]
	public class StateGraph : NodeGraph {
		/// <summary> Enumerate through all StatNodeBase nodes where active == true </summary>
		public IEnumerator<StateNodeBase> ActiveNodes() {
			for (int i = 0; i < nodes.Count; i++) {
				StateNodeBase node = nodes[i] as StateNodeBase;
				if (node != null && node.active) yield return node;
			}
		}
	}
}