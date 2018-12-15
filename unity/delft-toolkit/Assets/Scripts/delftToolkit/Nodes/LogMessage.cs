using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelftToolkit {
	[CreateNodeMenu("Debug/LogMessage")]
	public class LogMessage : StateNodeBase {
		[Input, TextArea] public string message;

		protected override void OnEnter() {
			Debug.Log(GetInputValue("message", message), this);
			Exit();
		}

		protected override void OnExit() {
			return;
		}
	}
}