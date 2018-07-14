using System;
using UnityEngine;

namespace DelftToolkit {
	[Serializable] public struct DingSignalFilter {
		public AiGlobals.Devices device;
		public string oscMessageFilter;

		/// <summary> Constructor </summary>
		public DingSignalFilter(AiGlobals.Devices device, string oscMessageFilter) {
			this.device = device;
			this.oscMessageFilter = oscMessageFilter;
		}

		public bool Match(DingSignal signal) {
			return signal.device == device && signal.oscMessage.Filter(oscMessageFilter);
		}

		public Type GetExpectedType() {
			if (oscMessageFilter.StartsWith("/num/")) return typeof(float);
			else if (oscMessageFilter.StartsWith("/vec/")) {
				if (oscMessageFilter.EndsWith(":x")) return typeof(float);
				else if (oscMessageFilter.EndsWith(":y")) return typeof(float);
				else if (oscMessageFilter.EndsWith(":z")) return typeof(float);
				else if (oscMessageFilter.EndsWith(":mag")) return typeof(float);
				return typeof(Vector3);
			} else if (oscMessageFilter.StartsWith("/str/")) return typeof(string);
			else return null;
		}

		public override string ToString() {
			return device.ToString() + ":" + oscMessageFilter;
		}
	}
}