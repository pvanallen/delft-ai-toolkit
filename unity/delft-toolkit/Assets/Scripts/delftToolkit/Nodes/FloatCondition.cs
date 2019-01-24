using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270), NodeTint(255, 255, 0), CreateNodeMenu("Conditions/Float")]
	public class FloatCondition : ConditionBase {
		public Condition[] conditions = new Condition[0];
		[Output] public float valueOut;
		public float value {
			get { return _value; }
			set { _value = value; CheckConditions(); }
		}

#region Events
		protected override void OnExit() {
			return;
		}

		protected override void OnEnter() {
			return;
		}
#endregion

		public override object GetValue(NodePort port) {
			if (port.fieldName == "valueOut") return value;
			else return base.GetValue(port);
		}

		[SerializeField] private float _value;

		protected override void HandleSignalEvent(DingSignal signal) {
			float value;
			if (filter.TryExtractValue<float>(signal, out value)) {
				this.signal = signal;
				this.value = value;
			}
		}

		protected override void CheckConditions() {
			// Store the active state because we want to evaluate all conditions, not just the first one
			bool activeCache = active;
			for (int i = 0; i < conditions.Length; i++) {
				// We evaluate first because we want the lastState in the condition to update regardless of 'active'
				if (conditions[i].Evaluate(value) && activeCache) {
					NodePort triggerPort = GetOutputPort("conditions " + i);
					if (triggerPort.IsConnected) {
						for (int k = 0; k < triggerPort.ConnectionCount; k++) {
							StateNodeBase nextNode = triggerPort.GetConnection(k).node as StateNodeBase;
							active = false;
							if (nextNode != null) nextNode.Enter();
						}
					}
				}
			}
		}

		[Serializable] public struct Condition {
			public enum CompareType { Gtr, Lss, Range }
			[NodeEnum] public CompareType compareType;
			public float floatValA;
			public float floatValB;
			public bool inverse;
			[NonSerialized] public bool lastState;

			public bool Evaluate(float test) {
				return lastState = EvaluateInternal(test) != inverse;
			}

			private bool EvaluateInternal(float test) {
				switch (compareType) {
					case CompareType.Gtr:
						return test > floatValA;
					case CompareType.Lss:
						return test < floatValA;
					case CompareType.Range:
						return test >= floatValA && test <= floatValB;
					default:
						return false;
				}
			}

		}
	}
}