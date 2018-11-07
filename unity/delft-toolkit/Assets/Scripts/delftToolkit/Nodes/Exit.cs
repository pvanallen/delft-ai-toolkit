using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Exit Node </summary>
	[NodeWidth(104)]
	public class Exit : StateNodeBase {
		protected override void OnEnter() {
			StateGraph stateGraph = graph as StateGraph;
			if (stateGraph != null) stateGraph.Exit();
			else Debug.LogWarning("Graph isn't a Delft StateGraph");
		}

		protected override void OnExit() {
			return;
		}
	}
}