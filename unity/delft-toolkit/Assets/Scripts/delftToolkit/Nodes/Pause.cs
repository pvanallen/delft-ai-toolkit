using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelftToolkit {
	/// <summary> Pause Node. Pauses until "continue" is clicked </summary>
	[CreateNodeMenu("Debug/Pause"), NodeWidth(104)]
	public class Pause : StateNodeBase {

		protected override void OnEnter() {
			return;
		}

		protected override void OnExit() {
			return;
		}
	}
}