using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateNodeMenu("")]
	public class StateNodeBase : Node {

		[NonSerialized] public bool active;
		[Input] public Empty enter;
		[Output] public Empty exit;

		public override object GetValue(NodePort port) {
			return null;
		}

		public virtual void Exit() {
			if (active != this) {
				Debug.LogWarning("Exiting from a non-active node. Aborted.");
				return;
			}

			active = false;

			OnExit();
			NodePort exitPort = GetOutputPort("exit");

			if (exitPort.IsConnected) {
				for (int i = 0; i < exitPort.ConnectionCount; i++) {
					StateNodeBase nextNode = exitPort.GetConnection(i).node as StateNodeBase;
					if (nextNode != null) nextNode.Enter();
				}
			}
		}

		public virtual void OnExit() { }

		public virtual void Enter() {
			active = true;
			OnEnter();
		}

		public virtual void OnEnter() { }

		[Serializable]
		public class Empty { }
	}
}