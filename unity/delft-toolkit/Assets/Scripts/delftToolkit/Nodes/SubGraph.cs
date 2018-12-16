using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Play another graph and continue when that graph reaches an Exit node </summary>
	[CreateNodeMenu("Graph Control/Sub Graph")]
	public class SubGraph : StateNodeBase {
		public StateGraph subGraph;

		protected override void OnEnter() {
			subGraph.onExit += Exit;
			subGraph.Run();
		}

		protected override void OnExit() {
			subGraph.onExit -= Exit;
		}
	}
}