using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Start Node </summary>
	[NodeWidth(104)]
	public class Start : StateNodeBase {
		public override void OnEnter() {
			Exit();
		}
	}
}