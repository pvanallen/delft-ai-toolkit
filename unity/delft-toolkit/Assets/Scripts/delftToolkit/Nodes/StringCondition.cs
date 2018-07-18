using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(255, 255, 0)]
	public class StringCondition : ConditionBase {
		public Condition[] conditions = new Condition[0];
		public string value {
			get { return _value; }
			set { _value = value; if (active) CheckConditions(); }
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
			for (int i = 0; i < conditions.Length; i++) {
				if (conditions[i].Evaluate(value)) {
					active = false;
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

		[Serializable] public struct Condition {
			public enum CompareType { StartsWith, EndsWith, Contains }
			public CompareType compareType;
			public string strVal;
			public bool inverse;

			public bool Evaluate(string test) {
				return EvaluateInternal(test) != inverse;
			}

			private bool EvaluateInternal(string test) {
				switch (compareType) {
					case CompareType.StartsWith:
						return test.StartsWith(strVal);
					case CompareType.EndsWith:
						return test.EndsWith(strVal);
					case CompareType.Contains:
						return test.Contains(strVal);
					default:
						return false;
				}
			}

		}
	}
}