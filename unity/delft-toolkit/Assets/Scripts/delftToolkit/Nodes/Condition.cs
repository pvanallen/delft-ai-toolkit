using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(255, 255, 0)]
	public class Condition : StateNodeBase {
		public enum ValueType {
			Vector3,
			X,
			Y,
			Z,
			String
		}

		public AiGlobals.Devices device = AiGlobals.Devices.ding1;
		public AiGlobals.SensorSource sensorSource = AiGlobals.SensorSource.virt;
		public ValueType valueType = ValueType.X;
		[Tooltip("Read signals matching message signature. (only exact match supported)")]
		public string messageFilter = "/num/analogin/0/";

		public FloatCondition[] floatConditions = new FloatCondition[0];
		public StringCondition[] stringConditions = new StringCondition[0];
		/// <summary> The last signal we received </summary>
		public DingSignal unfilteredSignal;
		/// <summary> The last signal we received which passed the filter </summary>
		public DingSignal filteredSignal;

		protected override void Init() {
			base.Init();
			DingControlPhysical.DingNumPhysicalEvent += HandlePhysNumEvent;
			DingControlVirtual.DingNumVirtualEvent += HandleVirtNumEvent;
		}

		void HandlePhysNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			if (sensorSource == AiGlobals.SensorSource.phys) {
				HandleNumEvent(device, adrs, val0, val1, val2);
			}
		}

		void HandleVirtNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			if (sensorSource == AiGlobals.SensorSource.virt) {
				HandleNumEvent(device, adrs, val0, val1, val2);
			}
		}

		void HandleNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			unfilteredSignal = new DingSignal(device, adrs, new Vector3(val0, val1, val2));
			if (messageFilter == adrs && this.device == device) {
				filteredSignal = unfilteredSignal;
				if (!active) return;
				if (filteredSignal.isValid) {
					switch (valueType) {
						case ValueType.String:
							string str;
							if (filteredSignal.TryGetValue(out str)) {
								for (int i = 0; i < stringConditions.Length; i++) {
									if (stringConditions[i].Evaluate(str)) {
										active = false;
										NodePort triggerPort = GetOutputPort("stringConditions " + i);
										if (triggerPort.IsConnected) {
											for (int k = 0; k < triggerPort.ConnectionCount; k++) {
												StateNodeBase nextNode = triggerPort.GetConnection(k).node as StateNodeBase;
												if (nextNode != null) nextNode.Enter();
											}
										}
									}
								}
							} else {
								Debug.LogWarning("Value mismatch. Expected string, got " + filteredSignal.value.GetType());
							}
							break;
						case ValueType.X:
						case ValueType.Y:
						case ValueType.Z:
						case ValueType.Vector3:
							Vector3 vec;
							if (filteredSignal.TryGetValue(out vec)) {
								float value = 0;
								switch (valueType) {
									case ValueType.X:
										value = vec.x;
										break;
									case ValueType.Y:
										value = vec.y;
										break;
									case ValueType.Z:
										value = vec.z;
										break;
									default:
										value = vec.magnitude;
										break;
								}
								for (int i = 0; i < floatConditions.Length; i++) {
									if (floatConditions[i].Evaluate(value)) {
										active = false;
										NodePort triggerPort = GetOutputPort("floatConditions " + i);
										if (triggerPort.IsConnected) {
											for (int k = 0; k < triggerPort.ConnectionCount; k++) {
												StateNodeBase nextNode = triggerPort.GetConnection(k).node as StateNodeBase;
												if (nextNode != null) nextNode.Enter();
											}
										}
									}
								}
							} else {
								Debug.LogWarning("Value mismatch. Expected Vector3, got " + filteredSignal.value.GetType());
							}
							break;
					}
				}
			}
		}

		[Serializable] public struct FloatCondition {
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

		[Serializable] public struct StringCondition {
			public enum CompareType { Contains, StartsWith, EndsWith, Equals, Length }
			public CompareType compareType;
			public string stringVal;
			public int intVal;
			public bool inverse;

			public bool Evaluate(string test) {
				return EvaluateInternal(test) != inverse;
			}

			private bool EvaluateInternal(string test) {
				switch (compareType) {
					case CompareType.Contains:
						return test.Contains(stringVal);
					case CompareType.StartsWith:
						return test.StartsWith(stringVal);
					case CompareType.EndsWith:
						return test.EndsWith(stringVal);
					case CompareType.Equals:
						return test == stringVal;
					case CompareType.Length:
						return test.Length == intVal;
					default:
						return false;
				}
			}
		}
	}
}