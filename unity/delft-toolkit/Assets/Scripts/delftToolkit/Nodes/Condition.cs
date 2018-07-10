using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

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
		public int conditionValue = 50;

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
					float val = GetSignalValue(filteredSignal);
					if (val > conditionValue) Exit();
				}
			}
		}

		public float GetSignalValue(DingSignal signal) {
			switch (valueType) {
				case ValueType.String:
					string str;
					if (signal.TryGetValue(out str)) {
						return str.Length; // Exit if string length exceeds 10
					} else {
						Debug.LogWarning("Value mismatch. Expected string, got " + signal.value.GetType());
					}
					break;
				case ValueType.X:
				case ValueType.Y:
				case ValueType.Z:
				case ValueType.Vector3:
					Vector3 vec;
					if (signal.TryGetValue(out vec)) {
						switch (valueType) {
							case ValueType.X:
								return vec.x;
							case ValueType.Y:
								return vec.y;
							case ValueType.Z:
								return vec.z;
							case ValueType.Vector3:
								return vec.magnitude;
						}
					} else {
						Debug.LogWarning("Value mismatch. Expected Vector3, got " + signal.value.GetType());
					}
					break;
			}
			return 0;
		}
	}
}