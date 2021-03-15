using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270), NodeTint(255, 255, 160), CreateNodeMenu("Conditions/Float")]
	public class FloatCondition : ConditionBase {

		public DingSignalFilter filter = new DingSignalFilter(AiGlobals.Devices.ding1, AiGlobals.SensorSource.virt, AiGlobals.FloatConditionType.analogin, 1);
		[Tooltip("Read signals matching message signature. (only exact match supported)")]
		/// <summary> The last signal we received which passed the filter </summary>
		[NonSerialized] public DingSignal signal;

		[NodeEnum] public AiGlobals.FloatConditionType inputType = AiGlobals.FloatConditionType.analogin;
		public Condition[] conditions = new Condition[0];
		[Output] public float valueOut;
		public float value {
			get { return _value; }
			set { _value = value; CheckConditions(); }
		}

		protected override void Init() {
			base.Init();
			DingSignal.onSignalEvent -= FilterSignalEvent;
			DingSignal.onSignalEvent += FilterSignalEvent;
		}

		void FilterSignalEvent(DingSignal signal) {
			if (filter.Match(signal)) {
				HandleSignalEvent(signal);
			}
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
			bool conditionMatch = false;
			for (int i = 0; i < conditions.Length; i++) {
				// We evaluate first because we want the lastState in the condition to update regardless of 'active'
				if (conditions[i].Evaluate(value, conditionMatch)) {
					conditionMatch = true; // track condition matches for "otherwise" condition
					if (activeCache) {
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
		}

		[Serializable] public struct Condition {
			public enum CompareType { GT, LT, Range, Otherwise }
			[NodeEnum] public CompareType compareType;
			public float floatValA;
			public float floatValB;
			public bool inverse;
			[NonSerialized] public bool lastState;

			public bool Evaluate(float test, bool priorConditionMatch) {
				return lastState = EvaluateInternal(test, priorConditionMatch) != inverse;
			}

			private bool EvaluateInternal(float test, bool priorConditionMatch ) {
				switch (compareType) {
					case CompareType.GT: // greater than or equal to
						return test >= floatValA;
					case CompareType.LT:
						return test < floatValA;
					case CompareType.Range:
						return test >= floatValA && test <= floatValB;
					case CompareType.Otherwise:
						// if NONE of the prior conditions are true, this is true
						return !priorConditionMatch;
					default:
						return false;
				}
			}

		}
	}
}