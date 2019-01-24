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
			// Ignore when node is already active.
			if (active) return;
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

		public IEnumerable<StateNodeBase> GetPreviousNodes() {
			NodePort enterPort = GetInputPort("enter");
			int connectionCount = enterPort.ConnectionCount;
			NodePort[] otherPorts = new NodePort[connectionCount];
			for (int i = 0; i < connectionCount; i++) {
				NodePort otherPort = enterPort.GetConnection(i);
				if (otherPort != null) yield return otherPort.node as StateNodeBase;
			}
		}

		public IEnumerable<StateNodeBase> GetRootNodes(HashSet<StateNodeBase> visited = null) {
			if (visited == null) visited = new HashSet<StateNodeBase>();
			visited.Add(this);

			int parentNodeCount = 0;
			foreach (StateNodeBase parentNode in this.GetPreviousNodes()) {
				parentNodeCount++;
				if (parentNode == null) continue;
				if (visited.Contains(parentNode)) continue;
				foreach (StateNodeBase rootNode in parentNode.GetRootNodes(visited)) {
					yield return rootNode;
				}
			}
			if (parentNodeCount == 0) yield return this;
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