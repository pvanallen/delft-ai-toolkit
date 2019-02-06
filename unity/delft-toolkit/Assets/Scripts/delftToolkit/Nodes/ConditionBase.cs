using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(255, 255, 0)]
	public abstract class ConditionBase : StateNodeBase {

		public DingSignalFilter filter = new DingSignalFilter(AiGlobals.Devices.ding1, AiGlobals.SensorSource.virt, "/num/analogin/0/");
		[Tooltip("Read signals matching message signature. (only exact match supported)")]
		/// <summary> The last signal we received which passed the filter </summary>
		[NonSerialized] public DingSignal signal;

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

		protected abstract void HandleSignalEvent(DingSignal signal);

		protected abstract void CheckConditions();
	}
}