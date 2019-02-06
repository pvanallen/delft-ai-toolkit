// control the physical thing (ding) via OSC to a nodejs server which 
// communicates by Bluetooth to the robot
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityOSC;

public class DingControlPhysical : DingControlBase {

	public string TargetAddr = "127.0.0.1";
	public int OutGoingPort = 5005;
	public int InComingPort = 5006;

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	private const string OSC_SERVER_CLIENT = "DelftDingOSC";

	private long lastOscMessageIn = 0;

	// Script initialization
	void Awake() {
		// using awake so that it happens before oscCentral initializes in Start()

		//OSCHandler.Instance.Init(); //init OSC
		OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
		servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string, ClientLog>();
	}

	public override void Update() {
		OSCHandler.Instance.UpdateLogs();

		servers = OSCHandler.Instance.Servers;

		foreach (KeyValuePair<string, ServerLog> item in servers) {
			//print(item.Value.packets.Count);
			// get the most recent NEW OSC message received
			if (OSC_SERVER_CLIENT == item.Key && item.Value.packets.Count > 0 && item.Value.packets[item.Value.packets.Count - 1].TimeStamp != lastOscMessageIn) {
				
				// count back until we find the matching timestamp
				int lastMsgIndex = item.Value.packets.Count - 1;
				while (lastMsgIndex > 0 && item.Value.packets[lastMsgIndex].TimeStamp != lastOscMessageIn) {
					lastMsgIndex--;
				}

				// set how many messages are queued up
				int msgsQd = 1;
				if (item.Value.packets.Count > 1) { // not the first item
					msgsQd = item.Value.packets.Count - lastMsgIndex - 1;
				}

				// print the queued messages
				for (int msgIndex = item.Value.packets.Count - msgsQd; msgIndex < item.Value.packets.Count; msgIndex++) {
					//
					string address = item.Value.packets[msgIndex].Address;
					if (address.StartsWith("/num/")) {
						float value = item.Value.packets[msgIndex].Data.Count > 0 ? float.Parse(item.Value.packets[msgIndex].Data[0].ToString()) : 0.0f;
						//print(OSC_SERVER_CLIENT + ": " + address + " " + value);
						if (DelftToolkit.DingSignal.onSignalEvent != null)
							DelftToolkit.DingSignal.onSignalEvent(new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.phys, address, value));
					}
					if (address.StartsWith("/vec/")) {
						float value0 = item.Value.packets[msgIndex].Data.Count > 0 ? float.Parse(item.Value.packets[msgIndex].Data[0].ToString()) : 0.0f;
						float value1 = item.Value.packets[msgIndex].Data.Count > 1 ? float.Parse(item.Value.packets[msgIndex].Data[1].ToString()) : 0.0f;
						float value2 = item.Value.packets[msgIndex].Data.Count > 2 ? float.Parse(item.Value.packets[msgIndex].Data[2].ToString()) : 0.0f;
						//print(OSC_SERVER_CLIENT + ": " + address + " " + value0 + " " + value1 + " " + value2);
						if (DelftToolkit.DingSignal.onSignalEvent != null)
							DelftToolkit.DingSignal.onSignalEvent(new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.phys, address, new Vector3(value0, value1, value2)));
					} else if (address.StartsWith("/str/")) {
						string value = item.Value.packets[msgIndex].Data.Count > 0 ? item.Value.packets[msgIndex].Data[0].ToString() : "null";
						//print("Received Event" + address + value);
						if (DelftToolkit.DingSignal.onSignalEvent != null)
							DelftToolkit.DingSignal.onSignalEvent(new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.phys, address, value));
					}
					//print(OSC_SERVER_CLIENT + ": " + address + " " + float.Parse(item.Value.packets[msgIndex].Data[0].ToString()));
				}
				lastOscMessageIn = item.Value.packets[item.Value.packets.Count - 1].TimeStamp;
			}
		}
	}

	public override void handleAction() {
		//base.Update ();
		List<object> oscValues = new List<object>();

		string oscString = "/" + action.actionType + "/";

		//base.handleAction();
		switch (action.actionType) {
			case AiGlobals.ActionTypes.move:
				//Debug.LogWarning("DING-PHYSICAL: " + thisDevice.ToString() + " " + action.actionType + " " + action.moveParams.type.ToString());
				oscValues.AddRange(new object[] {
					action.moveParams.type.ToString(),
					action.moveParams.time,
					action.moveParams.speed,
					action.moveParams.easing.ToString()
				});
				break;
			case AiGlobals.ActionTypes.leds:
				oscValues.AddRange(new object[] {
					action.ledParams.type.ToString(),
					action.ledParams.time,
					action.ledParams.ledNum,
					action.ledParams.color.ToCSV()
				});
				break;
			case AiGlobals.ActionTypes.delay:
				oscValues.AddRange(new object[] {
					action.delayParams.type.ToString(),
					action.delayParams.time
				});
				break;
			case AiGlobals.ActionTypes.analogin:
				oscValues.AddRange(new object[] {
					action.analoginParams.type.ToString(),
					action.analoginParams.interval,
					action.analoginParams.port
				});
				break;
			case AiGlobals.ActionTypes.servo:
				oscValues.AddRange(new object[] { 
					action.servoParams.type.ToString(),
					action.servoParams.angle,
					action.servoParams.port,
					action.servoParams.varspeed,
					action.moveParams.easing.ToString()
				});
				break;
			case AiGlobals.ActionTypes.speak:
				oscValues.AddRange(new object[] { 
					action.speakParams.type.ToString(),
					action.speakParams.utterance
				});
				break;
			case AiGlobals.ActionTypes.listen:
				oscValues.AddRange(new object[] { 
					action.listenParams.type.ToString(),
					action.listenParams.duration
				});
				break;
			case AiGlobals.ActionTypes.recognize:
				oscValues.AddRange(new object[] { 
					action.recognizeParams.type.ToString(),
					action.recognizeParams.model.ToString()
				});
				break;
			case AiGlobals.ActionTypes.playSound:
				oscValues.AddRange(new object[] {
					action.playSoundParams.type.ToString(),
					action.playSoundParams.time,
				});
				break;
			default:
				Debug.LogWarning("DING-PHYSICAL unknown type: " + action.actionType);
				break;
		}
		if (oscValues != null) {
			OSCHandler.Instance.SendMessageToClient(OSC_SERVER_CLIENT, oscString, oscValues);
		}
	}

}