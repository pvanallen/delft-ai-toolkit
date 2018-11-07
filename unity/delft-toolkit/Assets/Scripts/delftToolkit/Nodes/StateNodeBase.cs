using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	public abstract class StateNodeBase : Node {

		[NonSerialized] public bool active;
		[Input] public Empty enter;
		[Output] public Empty exit;

#region Public methods
		public override object GetValue(NodePort port) {
			return null;
		}

		public void Enter() {
			active = true;
			OnEnter();
		}

		public void Exit() {
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

		public StateNodeBase GetPreviousNode() {
			NodePort otherPort = GetInputPort("enter").Connection;
			if (otherPort != null) return otherPort.node as StateNodeBase;
			else return null;
		}

		public StateNodeBase GetFirstNode() {
			StateNodeBase node = this;
			HashSet<StateNodeBase> visited = new HashSet<StateNodeBase>() { this };
			while (true) {
				StateNodeBase prevNode = node.GetPreviousNode();
				if (prevNode == null) return node;
				else if (visited.Contains(prevNode)) {
					Debug.LogWarning("Node tree forms a loop! Can't get first node.", node);
					return null;
				} else {
					node = prevNode;
					visited.Add(node);
				}
			}
		}
#endregion

#region Events
		protected abstract void OnExit();

		protected abstract void OnEnter();
#endregion
		[Serializable]
		public class Empty { }
	}
}