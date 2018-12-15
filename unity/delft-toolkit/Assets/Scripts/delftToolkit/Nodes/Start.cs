using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Start Node </summary>
	[NodeWidth(104), CreateNodeMenu("Graph Control/Start")]
	public class Start : StateNodeBase {
		protected override void OnEnter() {
			Exit();
		}

		protected override void OnExit() {
			return;
		}
	}
}