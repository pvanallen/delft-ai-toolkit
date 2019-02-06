using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

public class DingControlVirtual : DingControlBase {

	public bool sendSensors = false;
	public int analoginPort = 0;

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
	public AiGlobals.Easing moveEasing = AiGlobals.Easing.easeInOut;

	// keyboard 
	private Array allKeyCodes;

	void Awake() {
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

				analoginPort = action.analoginParams.port;
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
				AudioSource audio = gameObject.AddComponent<AudioSource>();
    			audio.PlayOneShot ((AudioClip)Resources.Load ("ui_sounds/" + action.playSoundParams.type));
				break;
			default:
				//Debug.LogWarning("DING-VIRTUAL unknown type: " + action.actionType);
				break;
		}
		// ensure this event gets processed before the next one comes in
		Update();
	}

	public override void Update() {
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

		// watch keyboard
		foreach (KeyCode currentKey in allKeyCodes) {
            if (Input.GetKeyDown(currentKey)) {
				
				if (DelftToolkit.DingSignal.onSignalEvent != null) {
					print(currentKey);
					DelftToolkit.DingSignal signal = new DelftToolkit.DingSignal(thisDevice, AiGlobals.SensorSource.virt, "/str/keydown/", currentKey.ToString());
					DelftToolkit.DingSignal.onSignalEvent(signal);
				}
			}  
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