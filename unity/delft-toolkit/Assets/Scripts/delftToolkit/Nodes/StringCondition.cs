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
			bool deactivate = false;
			for (int i = 0; i < conditions.Length; i++) {
				// We evaluate first because we want the lastState in the condition to update regardless of 'active'
				if (conditions[i].Evaluate(value, deactivate)) {
					// deactivate also used for determining otherwise condition
					deactivate = true;
					if (active) {
						NodePort triggerPort = GetOutputPort("conditions " + i);
						if (triggerPort.IsConnected) {
							for (int k = 0; k < triggerPort.ConnectionCount; k++) {
								StateNodeBase nextNode = triggerPort.GetConnection(k).node as StateNodeBase;
								if (nextNode != null) nextNode.Enter();
							}
						}
					}
				}
			}
			if (deactivate) active = false;
		}

		[Serializable] public struct Condition {
			public enum CompareType { StartsWith, EndsWith, Contains, Otherwise }
			[NodeEnum] public CompareType compareType;
			public string strVal;
			public bool inverse;
			[NonSerialized] public bool lastState;

			public bool Evaluate(string test, bool priorCompareSuccess) {
				return lastState = EvaluateInternal(test, priorCompareSuccess) != inverse;
			}

			private bool EvaluateInternal(string test, bool priorCompareSuccess ) {
				test = test.ToLower();
				switch (compareType) {
					case CompareType.StartsWith:
						return test.StartsWith(strVal.ToLower());
					case CompareType.EndsWith:
						return test.EndsWith(strVal.ToLower());
					case CompareType.Contains:
						if (strVal.Contains(",")) {
							// check for multiple strings, any of which must be in target
							bool comparison = false;
							string[] theStrings = strVal.Split(',');
							foreach (string item in theStrings) {
								comparison = test.Contains(item.ToLower());
								if (comparison)
									break;
							}
							return comparison;
						} else return test.Contains(strVal.ToLower());
					case CompareType.Otherwise:
						// if none of the prior conditions are true, this is true
						return !priorCompareSuccess;
					default:
						return false;
				}
			}

		}
	}
}