using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	/// <summary> Restart Node </summary>
	[NodeWidth(104)]
	public class Restart : StateNodeBase {
		public override void OnEnter() {
			active = false;
			StateNodeBase firstNode = GetFirstNode();
			if (firstNode is Start) {
				(firstNode as Start).Enter();
			} else {
				Debug.LogWarning("First node not recognized as Start node. Can't reset.", firstNode);
			}
		}
	}
}