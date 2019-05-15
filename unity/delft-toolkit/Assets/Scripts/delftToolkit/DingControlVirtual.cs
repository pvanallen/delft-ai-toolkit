using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

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
	float sttStartTime = 0;
	float sttDuration = 5.0f;
	bool sttRecording = false;

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

		yawTarget = yawJoint.localEulerAngles.z;
		pitchTarget = pitchJoint.localEulerAngles.y;
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
				//Debug.LogWarning("DING-VIRTUAL: " + action.actionType + " " + action.ledParams.type.ToString());
				break;
			case AiGlobals.ActionTypes.delay:
				//Debug.LogWarning("DING-VIRTUAL: " + action.actionType + " " + action.delayParams.type.ToString());
				break;
			case AiGlobals.ActionTypes.analogin:
				string analoginType = action.analoginParams.type.ToString();
				if (action.analoginParams.type == AiGlobals.ActionAnalogInTypes.start) {
					analoginPort = action.analoginParams.port;
					//analoginInterval = action.analoginParams.interval;
					sendSensors = true;
				} else {
					sendSensors = false;
				}
				break;
			case AiGlobals.ActionTypes.servo:
				if (action.servoParams.port == 9) { // tilt
					//yawJoint.localRotation = Quaternion.Euler(0, -180, action.servoParams.angle - 90);
					yawTarget = Mathf.Clamp(action.servoParams.angle,0,180);
				} else if (action.servoParams.port == 10) { // pan
					//pitchJoint.localRotation = Quaternion.Euler(0, action.servoParams.angle + 180, 0);
					pitchTarget = Mathf.Clamp(action.servoParams.angle + 180,180,359);
					//pitchTarget = Mathf.Clamp(pitchTarget, 180, 359);
					//if (pitchTarget >= 360) pitchTarget = pitchTarget - 360;
				}
				break;
			case AiGlobals.ActionTypes.playSound:
				if (action.playSoundParams.source == AiGlobals.SensorSource.virt || action.playSoundParams.source == AiGlobals.SensorSource.both) {
					AudioSource audio = gameObject.AddComponent<AudioSource>();
					audio.PlayOneShot ((AudioClip)Resources.Load ("ui_sounds/" + action.playSoundParams.type));
				}
				break;
			case AiGlobals.ActionTypes.recognize:
				recognize = true;
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
					string utterance = action.speakParams.utterance.Replace("{variable}", action.variable);
					tts.Speak(utterance, action.speakParams.type.ToString());
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
			print("stopped");
		}

		// hand sending virtual sensor data
		if (sendSensors) {
			// very crude implementation
			if (DelftToolkit.DingSignal.onSignalEvent != null) {
				// float x = transform.position.x * 100.0f;
				// float y = transform.position.y * 100.0f;
				// float z = transform.position.z * 100.0f;
				//print("sending xyz " + z);
				//DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/vec/analogin/0/", new Vector3(z, x, y));
				//DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/num/analogin/0/", z);
				RaycastHit hit;
        		Ray forwardRay = new Ray(transform.position, -Vector3.up);

				//Cast a ray straight forwards.
				float distance = -1;
				Vector3 fwd = transform.TransformDirection(Vector3.forward);
        		if (Physics.Raycast(transform.position, fwd, out hit))
            		//print("Found an object - distance: " + hit.distance);
					// make it similar to what comes out of the arduino
					distance = 1023 - (hit.distance * 100);
				
				DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/num/analogin/0/", distance);
				if (DelftToolkit.DingSignal.onSignalEvent != null)
					DelftToolkit.DingSignal.onSignalEvent(signal);
			}
		}

		// handle recognize request
		if (DelftToolkit.DingSignal.onSignalEvent != null) {
			recognize = false;
			String tag = "no object";
			RaycastHit hit;
			Ray forwardRay = new Ray(transform.position, -Vector3.up);

			//Cast a ray straight forwards.
			Vector3 fwd = transform.TransformDirection(Vector3.forward);
			if (Physics.Raycast(transform.position, fwd, out hit)) {
				if (hit.distance < 4) {
					tag = hit.collider.tag;
				} else {
					//tag = "far:" + hit.collider.tag;
					tag = hit.collider.tag;
				}
			}
			DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/recognize/", tag);
			DelftToolkit.DingSignal.onSignalEvent(signal);
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
				transform.Rotate(Vector3.up, 100f * moveAmount);
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
		//print("Virt - Speech To Text: " + transcription);
		//stt.StopRecording();
		sttRecording = false;
		DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/speech2text/", transcription);
				if (DelftToolkit.DingSignal.onSignalEvent != null)
					DelftToolkit.DingSignal.onSignalEvent(signal);
	}

	public void ttsResults(float utteranceLength) {
		print("Virt - Text To Speech Length: " + utteranceLength + " seconds");
	}

	private void moveYaw() {
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
		}
	}

	private void movePitch() {
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
		}
	}
}