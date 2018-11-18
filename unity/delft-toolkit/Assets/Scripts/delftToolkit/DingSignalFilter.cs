using System;
using UnityEngine;

namespace DelftToolkit {
	[Serializable] public struct DingSignalFilter {
		[NodeEnum] public AiGlobals.Devices device;
		[NodeEnum] public AiGlobals.SensorSource source;
		public string messageFilter;

		/// <summary> Constructor </summary>
		public DingSignalFilter(AiGlobals.Devices device, AiGlobals.SensorSource source, string oscMessageFilter) {
			this.device = device;
			this.source = source;
			this.messageFilter = oscMessageFilter;
		}

		public bool Match(DingSignal signal) {
			return signal.device == device && signal.source == source && signal.oscMessage.Filter(messageFilter.Split(':') [0]);
		}

		public Type GetExpectedType() {
			if (messageFilter.StartsWith("/num/")) return typeof(float);
			else if (messageFilter.StartsWith("/vec/")) {
				if (messageFilter.EndsWith(":x")) return typeof(float);
				else if (messageFilter.EndsWith(":y")) return typeof(float);
				else if (messageFilter.EndsWith(":z")) return typeof(float);
				else if (messageFilter.EndsWith(":mag")) return typeof(float);
				return typeof(Vector3);
			} else if (messageFilter.StartsWith("/str/")) return typeof(string);
			else return null;
		}

		public bool TryExtractValue<T>(DingSignal signal, out T value) {
			object obj = null;
			if (messageFilter.StartsWith("/num/")) return signal.TryGetValue<T>(out value);
			else if (messageFilter.StartsWith("/vec/")) {
				Vector3 v;
				if (signal.TryGetValue<Vector3>(out v)) {
					if (messageFilter.EndsWith(":x")) obj = v.x;
					else if (messageFilter.EndsWith(":y")) obj = v.y;
					else if (messageFilter.EndsWith(":z")) obj = v.z;
					else if (messageFilter.EndsWith(":mag")) obj = v.magnitude;
					else return signal.TryGetValue<T>(out value);
					if (obj is T) {
						value = (T) obj;
						return true;
					}
				}
			} else if (messageFilter.StartsWith("/str/")) return signal.TryGetValue<T>(out value);
			value = default(T);
			return false;
		}

		public override string ToString() {
			return device.ToString() + ":" + messageFilter;
		}
	}
}