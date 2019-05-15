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

	private string RobotIPAddr = "127.0.0.1";
	private int OutgoingPort = 5005;
	private int IncomingPort = 5006;

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	private const string OSC_SERVER_CLIENT = "DelftDingOSC";
	private string serverClientID;
	private bool OSCInit = false;
	private bool watsonTtsInit = false;

	private long lastOscMessageIn = 0;

	// Watson
	private Dictionary<AiGlobals.WatsonServices,Settings.WatsonService> watsonCredentials = new Dictionary<AiGlobals.WatsonServices,Settings.WatsonService>();

	// ScriptableObject for settings in Assets>Resources>DelftAIToolkitSettings
	private Settings delftSettings;

	// Script initialization
	void Awake() {

		// read toolkit settings
		delftSettings = Resources.Load<Settings>("DelftAIToolkitSettings");
		foreach (Settings.WatsonService service in delftSettings.watsonServices) {
			//print("Phys " + thisDevice + ": Loading Watson Service Credentials: " + service.service.ToString());
			watsonCredentials.Add(service.service,service);
		}
		foreach (Settings.Ding dingNetwork in delftSettings.dings) {
			if (dingNetwork.device == thisDevice) {
				//print("Phys " + thisDevice + ": Loading Robot Network Settings: " + dingNetwork.device.ToString());
				RobotIPAddr = dingNetwork.robotIP;
				OutgoingPort = dingNetwork.robotOutPort;
				IncomingPort = dingNetwork.robotInPort;
			}
		}

		if (RobotIPAddr != "127.0.0.1") {
			// set up OSC
			// using awake so that it happens before oscCentral initializes in Start()
			serverClientID = OSC_SERVER_CLIENT + RobotIPAddr + IncomingPort;
			OSCHandler.Instance.Init(serverClientID, RobotIPAddr, OutgoingPort, IncomingPort);
			servers = new Dictionary<string, ServerLog>();
			clients = new Dictionary<string, ClientLog>();
			OSCInit = true;

			// set up Watson services
			List<object> oscValues;
			string oscString;
			string url;
			// Watson Speech to Text
			if (watsonCredentials[AiGlobals.WatsonServices.speechToText].iamKey != "") {
				oscValues = new List<object>();
				oscString = "/initstt/";
				url = "default";
				if (watsonCredentials[AiGlobals.WatsonServices.speechToText].url != "") {
					url = watsonCredentials[AiGlobals.WatsonServices.speechToText].url;
				}
				print(thisDevice + ": Initializing Physical Watson Speech To Text...");
				oscValues.AddRange(new object[] {
					"watson",
					watsonCredentials[AiGlobals.WatsonServices.speechToText].iamKey,
					url
				});
				OSCHandler.Instance.SendMessageToClient(serverClientID, oscString, oscValues);
			} //else print("Need IamKey to start Watson Speech To Text - See Assets>Resources>DelftAITookitSettings");

			// Watson Text to Speech
			if (watsonCredentials[AiGlobals.WatsonServices.textToSpeech].iamKey != "") {
				oscValues = new List<object>();
				oscString = "/inittts/";
				url = "default";
				if (watsonCredentials[AiGlobals.WatsonServices.textToSpeech].url != "") {
					url = watsonCredentials[AiGlobals.WatsonServices.textToSpeech].url;
				}
				print(thisDevice + ": Initializing Physical Watson Text to Speech...");
				oscValues.AddRange(new object[] {
					"watson",
					watsonCredentials[AiGlobals.WatsonServices.textToSpeech].iamKey,
					url
				});
				OSCHandler.Instance.SendMessageToClient(serverClientID, oscString, oscValues);
			} // else print("Need IamKey to start Watson Text to Speech - See Assets>Resources>DelftAITookitSettings");
		}
	}

	public override void Update() {
		if(OSCInit) {
			OSCHandler.Instance.UpdateLogs();
			servers = OSCHandler.Instance.Servers;

			foreach (KeyValuePair<string, ServerLog> item in servers) {
				//print(item.Value.packets.Count);
				// get the most recent NEW OSC message received
				if (serverClientID == item.Key && item.Value.packets.Count > 0 && item.Value.packets[item.Value.packets.Count - 1].TimeStamp != lastOscMessageIn) {
					
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
			case AiGlobals.ActionTypes.textToSpeech:
				if (action.speakParams.source == AiGlobals.SensorSource.phys 
				|| action.speakParams.source == AiGlobals.SensorSource.both) {
					string utterance = action.speakParams.utterance.Replace("{variable}", action.variable);
					oscValues.AddRange(new object[] { 
						action.speakParams.model.ToString(), // watson, pico
						action.speakParams.type.ToString(), // voice
						utterance
					});
				}
				break;
			case AiGlobals.ActionTypes.speechToText:
				if (action.listenParams.source == AiGlobals.SensorSource.phys 
				|| action.listenParams.source == AiGlobals.SensorSource.both) {
					oscValues.AddRange(new object[] { 
						action.listenParams.model.ToString(), // watson, snips coming
						action.listenParams.lang.ToString(),
						action.listenParams.duration
					});
				}
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
		if (oscValues != null && oscValues.Count != 0 && OSCInit) {
			// print(oscString);
			// print(oscValues[1]);
			// print(oscValues[2]);
			OSCHandler.Instance.SendMessageToClient(serverClientID, oscString, oscValues);
		}
	}

}