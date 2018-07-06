using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(255, 255, 0)]
	public class Condition : StateNodeBase {
		// Adding [Input] or [Output] is all you need to do to register a field as a valid port on your node 
		[Range(0, 1023)][Input] public float value;

		public AiGlobals.Devices sensorDevice = AiGlobals.Devices.ding1;
		public string matchDingMessage = "/num/analogin/0/";
		public AiGlobals.SensorSource sensorSource = AiGlobals.SensorSource.virt;
		public string incomingDingMessage = "";

		private Coroutine tick;

		protected override void Init() {
			base.Init();
			DingControlPhysical.DingNumPhysicalEvent += HandlePhysNumEvent;
			DingControlVirtual.DingNumVirtualEvent += HandleVirtNumEvent;
		}

		public override void OnEnter() {
			tick = Tick().RunCoroutine();
		}

		public override void OnExit() {
			if (tick != null) tick.StopCoroutine();
		}

		IEnumerator Tick() {
			while (true) {
				if (value > 50) {
					Exit();
					break;
				}
				yield return null;
			}
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

			incomingDingMessage = adrs;
			//UnityEngine.Debug.Log("got virt " + incomingDingMessage + " " + matchDingMessage);
			if (incomingDingMessage == matchDingMessage && sensorDevice == device) {
				value = val0;
				//GetValue(GetOutputPort("result"));
				//if (result > 50) {
				//node.MoveNext();
				//}
			} else {
				//value0 = 0;
			}
			//UnityEngine.Debug.Log("DING DATA Condition (" + adrs + "): " + val0);
		}
	}
}