using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Restart Node </summary>
	[NodeWidth(104), CreateNodeMenu("Graph Control/Restart")]
	public class Restart : StateNodeBase {
		protected override void OnEnter() {
			active = false;
			foreach (StateNodeBase rootNode in GetRootNodes()) {
				if (rootNode is Start) {
					(rootNode as Start).Enter();
				}
			}
		}

		protected override void OnExit() {
			return;
		}
	}
}