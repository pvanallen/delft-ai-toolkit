using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270), NodeTint(255, 255, 0), CreateNodeMenu("Conditions/String")]
	public class StringCondition : ConditionBase {
		public Condition[] conditions = new Condition[0];
		[Output] public string valueOut;
		public string value {
			get { return _value; }
			set { _value = value; CheckConditions(); }
		}

		protected override void OnExit() {
			return;
		}

		protected override void OnEnter() {
			return;
		}

		public override object GetValue(NodePort port) {
			if (port.fieldName == "valueOut") return value;
			else return base.GetValue(port);
		}

		[SerializeField] private string _value;

		protected override void HandleSignalEvent(DingSignal signal) {
			string value;
			if (filter.TryExtractValue<string>(signal, out value)) {
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
			public enum CompareType { StartsWith, EndsWith, Contains }
			[NodeEnum] public CompareType compareType;
			public string strVal;
			public bool inverse;
			[NonSerialized] public bool lastState;

			public bool Evaluate(string test) {
				return lastState = EvaluateInternal(test) != inverse;
			}

			private bool EvaluateInternal(string test) {
				test = test.ToLower();
				switch (compareType) {
					case CompareType.StartsWith:
						return test.StartsWith(strVal.ToLower());
					case CompareType.EndsWith:
						return test.EndsWith(strVal.ToLower());
					case CompareType.Contains:
						return test.Contains(strVal.ToLower());
					default:
						return false;
				}
			}

		}
	}
}