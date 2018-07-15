using System;
using UnityEngine;

namespace DelftToolkit {
	public struct DingSignal {
		public static Action<DingSignal> onSignalEvent;

		public AiGlobals.Devices device { get; private set; }
		public AiGlobals.SensorSource source { get; private set; }
		public string oscMessage { get; private set; }
		public object value { get; private set; }
		public bool isValid { get { return value != null && oscMessage != null; } }

		/// <summary> Constructor </summary>
		public DingSignal(AiGlobals.Devices device, AiGlobals.SensorSource source, string oscMessage, object value) {
			this.device = device;
			this.source = source;
			this.oscMessage = oscMessage;
			this.value = value;
		}

		/// <summary> Try to get value of type. Returns false if value type doesn't match. </summary>
		public bool TryGetValue<T>(out T value) {
			if (this.value is T) {
				value = (T) this.value;
				return true;
			} else {
				value = default(T);
				return false;
			}
		}

		public override string ToString() {
			return device.ToString() + ":" + oscMessage + ":" + value.ToString();
		}
	}
}