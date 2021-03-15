﻿using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

namespace DelftToolkit {
	public class DingControlVirtual : DingControlBase {

		private bool sendSensors = false;
		private int analoginPort = 0;
		private bool recognize = false;


		// servos
		public Transform body;
		public Transform yawJoint;
		public Transform pitchJoint;
		private float yawTarget;
		private float pitchTarget;
		private float servoSpeed = 4;
		private	bool moveYawNow = false;
		private bool movePitchNow = false;

		// move motors
		private AiGlobals.ActionMoveTypes moveType = AiGlobals.ActionMoveTypes.stop;
		private float moveSpeed = 1;
		private AiGlobals.Easing moveEasing = AiGlobals.Easing.easeInOut;

		// keyboard 
		private Array allKeyCodes;

		// OSC
		private string MarionetteIPAddr = "127.0.0.1";
		private int OutgoingPort = 5007;
		private int IncomingPort = 5008;

		private Dictionary<string, ServerLog> servers;
		private Dictionary<string, ClientLog> clients;

		private const string OSC_SERVER_CLIENT = "DelftMarionetteOSC";
		private string serverClientID;
		private bool OSCInit = false;
		private long lastOscMessageIn = 0;

		// LEDs
		private int ledBlinkNum = 0;
		private bool ledBlinkState = false;
		private float ledBlinkInterval = 0;
		private float ledBlinkNextTime = 0;
		private Color ledBlinkColor = new Color(0.236f, 0.0f, 0.5f);

		// Watson
		private Dictionary<AiGlobals.WatsonServices,Settings.WatsonService> watsonCredentials = new Dictionary<AiGlobals.WatsonServices,Settings.WatsonService>();
		private WatsonSTT stt;
		private WatsonTTS tts;
		//private WatsonAssistantV2 assistant;
		float sttStartTime = 0;
		float sttDuration = 5.0f;
		bool sttRecording = false;
		float recogDistance = 5;

		// ScriptableObject for settings in Assets>Resources>DelftAIToolkitSettings
		private Settings delftSettings;

		// Script initialization

		void Awake() {
			delftSettings = Resources.Load<Settings>("DelftAIToolkitSettings");
			foreach (Settings.WatsonService service in delftSettings.watsonServices) {
				//print("Virt " + thisDevice + ": Loading Watson Service Credentials: " + service.service.ToString());
				watsonCredentials.Add(service.service,service);
			}
			foreach (Settings.Ding dingNetwork in delftSettings.dings) {
				if (dingNetwork.device == thisDevice) {
					//print("Virt " + thisDevice + ": Loading Marionnette Network Settings: " + dingNetwork.device.ToString());
					MarionetteIPAddr = dingNetwork.marionetteIP;
					OutgoingPort = dingNetwork.marionetteOutPort;
					IncomingPort = dingNetwork.marionetteInPort;
				}
			}

			// OSC
			if (MarionetteIPAddr != "127.0.0.1") {
				serverClientID = OSC_SERVER_CLIENT + MarionetteIPAddr + IncomingPort;
				OSCHandler.Instance.Init(serverClientID, MarionetteIPAddr, OutgoingPort, IncomingPort);
				servers = new Dictionary<string, ServerLog>();
				clients = new Dictionary<string, ClientLog>();
				OSCInit = true;
			}

			// Watson Speech to Text
			if (watsonCredentials[AiGlobals.WatsonServices.speechToText].iamKey != "") {
				print(thisDevice + ": Initializing Virtual Watson Speech To Text...");
				stt = gameObject.AddComponent<WatsonSTT>();
				stt.StartService(sttResults,watsonCredentials[AiGlobals.WatsonServices.speechToText].iamKey);
			} // else print("Need IamKey to start Watson Speech To Text - See Assets>Resources>DelftAITookitSettings");

			// Watson Text to Speech
			if (watsonCredentials[AiGlobals.WatsonServices.textToSpeech].iamKey != "") {
				print(thisDevice + ": Initializing Virtual Watson Text to Speech...");
				tts = gameObject.AddComponent<WatsonTTS>();
				tts.StartService(ttsResults,watsonCredentials[AiGlobals.WatsonServices.textToSpeech].iamKey,
				watsonCredentials[AiGlobals.WatsonServices.textToSpeech].url);
			} // else print("Need IamKey to start Watson Text to Speech - See Assets>Resources>DelftAITookitSettings");

			// Watson Assistant
			//if (watsonCredentials[AiGlobals.WatsonServices.assistant].iamKey != "") {
			//	print(thisDevice + ": Initializing Virtual Watson Assistant...");
			//	assistant = gameObject.AddComponent<WatsonAssistantV2>();
			//	assistant.StartService(assistantResults(), watsonCredentials[AiGlobals.WatsonServices.assistant].iamKey,watsonCredentials[AiGlobals.WatsonServices.assistant].assistantid,
			//	watsonCredentials[AiGlobals.WatsonServices.textToSpeech].url);
			//	assistant.message("what is machine learning?");
			//} // else print("Need IamKey to start Watson Text to Speech - See Assets>Resources>DelftAITookitSettings");

			// yawTarget = yawJoint.localEulerAngles.z;
			// pitchTarget = pitchJoint.localEulerAngles.y;
			allKeyCodes = System.Enum.GetValues(typeof(KeyCode));
		}

		public override void handleAction() {
			//base.handleAction();
			switch (action.actionType) {
				case AiGlobals.ActionTypes.move:
					//Debug.LogWarning("DING-VIRTUAL: " + action.actionType + " " + action.moveParams.type.ToString());
					moveType = action.moveParams.type;
					moveSpeed = action.moveParams.speed;
					moveEasing = action.moveParams.easing;
					break;
				case AiGlobals.ActionTypes.leds:
					switch (action.ledParams.type) {
						case AiGlobals.ActionLedTypes.set:
							//this.GetComponent<Renderer>().material.color = new Color(0.236f, 0.0f, 0.5f);
							body.GetComponent<Renderer>().material.color = action.ledParams.color;
							break;
						case AiGlobals.ActionLedTypes.allOff:
							body.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f);
							break;
						case AiGlobals.ActionLedTypes.blink:
							ledBlinkColor = action.ledParams.color;
							ledBlinkNum = action.ledParams.ledNum * 2; // reusing ledNum for number of times to blink
							ledBlinkInterval = action.ledParams.time;
							ledBlinkNextTime = 0;
							ledBlinkState = false;
							break;
						default:
							break;
					}
					Debug.LogWarning("DING-VIRTUAL LED Action: " + action.actionType + " " + action.ledParams.type.ToString());
					break;
				case AiGlobals.ActionTypes.delay:
					//Debug.LogWarning("DING-VIRTUAL: " + action.actionType + " " + action.delayParams.type.ToString());
					break;
				case AiGlobals.ActionTypes.analogin:
					string analoginType = action.analoginParams.type.ToString();
					if (action.analoginParams.source == AiGlobals.SensorSource.virt 
					|| action.analoginParams.source == AiGlobals.SensorSource.both) {
						if (action.analoginParams.type == AiGlobals.ActionAnalogInTypes.start) {
							analoginPort = action.analoginParams.port;
							//analoginInterval = action.analoginParams.interval;
							sendSensors = true;
						} else {
							sendSensors = false;
						}
					}
					break;
				case AiGlobals.ActionTypes.servo:
					if ((action.servoParams.source == AiGlobals.SensorSource.virt) 
					|| (action.servoParams.source == AiGlobals.SensorSource.both)) {
						yawTarget = yawJoint.localEulerAngles.z;
						pitchTarget = pitchJoint.localEulerAngles.y;
						if (action.servoParams.port == 1) { // pan
							yawTarget = Mathf.Clamp(action.servoParams.angle,0,180);
							moveYawNow = true;
						} else if (action.servoParams.port == 2) { // tilt
							pitchTarget = Mathf.Clamp(action.servoParams.angle + 180,180,359);
							movePitchNow = true;
						}
					}
					break;
				case AiGlobals.ActionTypes.playSound:
					if (action.playSoundParams.source == AiGlobals.SensorSource.virt || action.playSoundParams.source == AiGlobals.SensorSource.both) {
						AudioSource audio = gameObject.AddComponent<AudioSource>();
						audio.PlayOneShot ((AudioClip)Resources.Load ("ui_sounds/" + action.playSoundParams.type));
					}
					break;
				case AiGlobals.ActionTypes.recognize:
					if (action.recognizeParams.source == AiGlobals.SensorSource.virt || action.recognizeParams.source == AiGlobals.SensorSource.both) {
						recognize = true;
						recogDistance = action.recognizeParams.minDistance;
					}
					break;
				case AiGlobals.ActionTypes.speechToText:
					if ((action.listenParams.source == AiGlobals.SensorSource.virt 
					|| action.listenParams.source == AiGlobals.SensorSource.both)
					&& watsonCredentials[AiGlobals.WatsonServices.speechToText].iamKey != "") {
						stt.StartRecording(action.listenParams.lang);
						sttStartTime = Time.time;
						sttRecording = true;
						sttDuration = action.listenParams.duration;
					}
					break;
				case AiGlobals.ActionTypes.textToSpeech:
					if ((action.speakParams.source == AiGlobals.SensorSource.virt 
					|| action.speakParams.source == AiGlobals.SensorSource.both)
					&& watsonCredentials[AiGlobals.WatsonServices.textToSpeech].iamKey != "") {
						string utterance = action.speakParams.utterance.Replace("{stringIn}", action.stringIn);
						tts.Speak(utterance, action.speakParams.type.ToString(), action.speakParams.time);
					}
					break;
				default:
					//Debug.LogWarning("DING-VIRTUAL unknown type: " + action.actionType);
					break;
			}
			// ensure this event gets processed before the next one comes in
			Update();
		}

		public override void Update() {
			// keep track of speech to text
			if (sttRecording == true && Time.time - sttStartTime >= sttDuration) {
				stt.StopRecording();
				sttRecording = false;
				//print("stopped STT");
				Debug.LogWarning("DING-VIRTUAL stopped STT after: " + sttDuration  + " seconds");
			}

			// send virtual sensor data
			if (sendSensors) {
				// very crude implementation
				if (DelftToolkit.DingSignal.onSignalEvent != null) {
					RaycastHit hit;
					Ray forwardRay = new Ray(transform.position, -Vector3.up);

					//Cast a ray straight forwards.
					float distance = 1000; // when nothing is in front of it
					Vector3 fwd = transform.TransformDirection(Vector3.forward);
					if (Physics.Raycast(transform.position, fwd, out hit))
						//print("Found an object - distance: " + hit.distance);
						// make it similar to what comes out of the phycical sensor
						//distance = 1023 - (hit.distance * 100); // IR sensor
						distance = hit.distance * 10; // sonar sensor goes lower as it object gets closer
					string url = "/num/analogin/" + analoginPort + "/";
					DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, url, distance);
					if (DelftToolkit.DingSignal.onSignalEvent != null)
						DelftToolkit.DingSignal.onSignalEvent(signal);
				}
			}

			// handle recognize request
			if (recognize) {
				if (DelftToolkit.DingSignal.onSignalEvent != null) {
					String tag = "Untagged";
					Transform tilt = FindChildByRecursion(transform,"tilt");
					RaycastHit hit;

					//Cast a ray straight forwards from servo tilt/pan head
					Vector3 fwd = tilt.TransformDirection(Vector3.forward);
					if (Physics.Raycast(tilt.position, fwd, out hit)) {
						if (hit.distance < recogDistance) {
							tag = hit.collider.tag;
						} else {
							tag = "far:" + hit.collider.tag;
						}
					}
					//Debug.LogWarning("recognize: " + tag);
					Debug.DrawRay(tilt.position, fwd * 10, Color.green,4,false );
					// if (tag != "Untagged") {
						DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/recognize/", tag);
						DelftToolkit.DingSignal.onSignalEvent(signal);
					// }
				}
			}

			// watch keyboard
			foreach (KeyCode currentKey in allKeyCodes) {
				if (Input.GetKeyDown(currentKey)) {
					
					if (DelftToolkit.DingSignal.onSignalEvent != null) {
						//print(currentKey);
						DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/keydown/", currentKey.ToString());
						DelftToolkit.DingSignal.onSignalEvent(signal);
					}
				}  
			}

			// handle led blinking
			if (ledBlinkNum > 0 && Time.time > ledBlinkNextTime ) {
				if (ledBlinkState) { // turn it off
					body.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f);
				} else { // turn it on
					body.GetComponent<Renderer>().material.color = ledBlinkColor;
				}
				ledBlinkState = !ledBlinkState;
				ledBlinkNextTime = Time.time + ledBlinkInterval;
				ledBlinkNum--;
			}

			// handle servo position
			moveYaw();
			movePitch();

			// change this to use separate vars for move and movetype if move active
			float moveAmount = moveSpeed * speedAdj * Time.deltaTime;		
			switch (moveType) {
				case AiGlobals.ActionMoveTypes.stop:
					break;
				case AiGlobals.ActionMoveTypes.forward:
					transform.position += transform.forward * moveAmount;
					break;
				case AiGlobals.ActionMoveTypes.backward:
					transform.position -= transform.forward * moveAmount;
					break;
				case AiGlobals.ActionMoveTypes.turnRight:
					transform.Rotate(Vector3.up, 1 * 100f * moveAmount);
					break;
				case AiGlobals.ActionMoveTypes.turnLeft:
					transform.Rotate(Vector3.up, -1 * 100f * moveAmount);
					break;
				default:
					break;
			}	

			// OSC
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
						lastOscMessageIn = item.Value.packets[item.Value.packets.Count - 1].TimeStamp;

						// check the queued messages
						for (int msgIndex = item.Value.packets.Count - msgsQd; msgIndex < item.Value.packets.Count; msgIndex++) {
							//
							string address = item.Value.packets[msgIndex].Address;
							if (address.StartsWith("/")) {
								float value0 = item.Value.packets[msgIndex].Data.Count > 0 ? float.Parse(item.Value.packets[msgIndex].Data[0].ToString()) : 0.0f;
								float value1 = item.Value.packets[msgIndex].Data.Count > 1 ? float.Parse(item.Value.packets[msgIndex].Data[1].ToString()) : 0.0f;
								float value2 = item.Value.packets[msgIndex].Data.Count > 2 ? float.Parse(item.Value.packets[msgIndex].Data[2].ToString()) : 0.0f;
								address = "/num" + address + "/";
								print(OSC_SERVER_CLIENT + ": " + address + " " + value0 + " " + value1 + " " + value2);
								if (DelftToolkit.DingSignal.onSignalEvent != null) {
									DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, address, value0);
									DelftToolkit.DingSignal.onSignalEvent(signal);
									//DelftToolkit.DingSignal.onSignalEvent(new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, address, new Vector3(value0, value1, value2)));
								}
							} 
							//print(OSC_SERVER_CLIENT + ": " + address + " " + float.Parse(item.Value.packets[msgIndex].Data[0].ToString()));
						}
					}
				}		
			}
		}

		public void sttResults(string transcription) {
			print("Virt - Speech To Text: " + transcription);
			//stt.StopRecording();
			sttRecording = false;
			DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/speech2text/", transcription);
					if (DelftToolkit.DingSignal.onSignalEvent != null)
						DelftToolkit.DingSignal.onSignalEvent(signal);
		}

		public void ttsResults(string utterance, float utteranceLength) {
			print("Virt - Text To Speech time for: "
				+ utterance
				+ " "
				+ utteranceLength
				+ " seconds");
		}

		public void assistantResults(string answer) {
			print("Virt - Assistant: " + answer);
		}

		private void moveYaw() {
			if (moveYawNow) {
				var yawCurrent = yawJoint.localEulerAngles;
				float yawChange = servoSpeed;
				float yawCurrentZNormalized = yawCurrent.z;
				if (yawCurrentZNormalized != yawTarget) {
					float yawDiff = yawTarget - yawCurrentZNormalized; 
					if (yawDiff < 0) {
						yawChange = -1 * servoSpeed;
					}

					float yawZ = yawCurrentZNormalized + yawChange;
					if (Mathf.Abs(yawZ - yawTarget) < servoSpeed) {
						yawZ = yawTarget;
					}

					//print(yawCurrent.z + " " + yawCurrentZNormalized + " " + yawTarget + " " + yawDiff + " " + yawZ);
					yawJoint.localRotation = Quaternion.Euler(yawCurrent.x, yawCurrent.y, yawZ);
				} else {
					moveYawNow = false;
				}
			}
		}

		private void movePitch() {
			if (movePitchNow) {
				var pitchCurrent = pitchJoint.localEulerAngles;
				float pitchChange = servoSpeed;
				if (pitchCurrent.y != pitchTarget) {
					float pitchDiff = pitchTarget - pitchCurrent.y;
					if (Mathf.Abs(pitchDiff) < servoSpeed) {
						pitchChange = pitchDiff;
					} else if (pitchDiff < 0) {
						pitchChange = -1 * servoSpeed;
					}
					float pitchY = Mathf.Clamp(pitchCurrent.y + pitchChange, 0,359);
					pitchJoint.localRotation = Quaternion.Euler(pitchCurrent.x, pitchY, pitchCurrent.z);
				} else {
					movePitchNow = false;
				}
			}
		}

		private Transform FindChildByRecursion(Transform aParent, string aName) {
			if (aParent == null) return null;
			var result = aParent.Find(aName);
			if (result != null)
				return result;
			foreach (Transform child in aParent)
			{
				result = child.FindChildByRecursion(aName);
				if (result != null)
					return result;
			}
			return null;
		}
	}
}