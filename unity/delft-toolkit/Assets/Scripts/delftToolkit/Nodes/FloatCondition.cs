using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(255, 255, 0)]
	public class FloatCondition : ConditionBase {
		public Condition[] conditions = new Condition[0];
		public float value {
			get { return _value; }
			set { _value = value; if (active) CheckConditions(); }
		}

		[SerializeField] private float _value;

		protected override void HandleSignalEvent(DingSignal signal) {
			float value;
			if (filter.TryExtractValue<float>(signal, out value)) {
				this.signal = signal;
				if (!overrideValue) this.value = value;
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
			public enum CompareType { Gtr, Lss, Range }
			public CompareType compareType;
			public float floatValA;
			public float floatValB;
			public bool inverse;

			public bool Evaluate(float test) {
				return EvaluateInternal(test) != inverse;
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