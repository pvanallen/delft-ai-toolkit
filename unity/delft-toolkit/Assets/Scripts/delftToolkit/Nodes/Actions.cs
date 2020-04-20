using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelftToolkit {
	[NodeWidth(270)][NodeTint(0f, 1f, 1f)]
	public class Actions : StateNodeBase {

		[Input(instancePortList = true)]
		public List<Action> actions = new List<Action> { new Action() };

		/// <summary> How many times should we repeat the actions list </summary>
		public int repeats = 1;
		/// <summary> If true, will pick a random action from the list instead of looping through it </summary>
		public bool random = false;
		/// <summary> How many times have we repeated so far </summary>
		public int repeatCount { get; private set; }
		/// <summary> Current action index </summary>
		public int currentAction { get { return _currentAction; } }
		private int _currentAction = -1;
		[NodeEnum]
		public AiGlobals.Devices device = AiGlobals.Devices.ding1;

		/// <summary> Used only for storing the expanded state of the actions list. </summary>
		[SerializeField, HideInInspector] private bool expanded;
		[Tooltip("This input lets you define a variable to use in your actions. Simply type {stringIn} as part of a text input field to have it be replaced at runtime.")]
		[Input] public string stringIn;
		[Input] public float floatIn;
		private Action actionStopAll = new Action();

		public delegate void DingActionEvent(AiGlobals.Devices device, Action action);
		public static event DingActionEvent DingEvent;

		protected override void Init() {
			// actionStopAll.moveParams.time = 0;
			// actionStopAll.moveParams.type = AiGlobals.ActionMoveTypes.stop;
			// actionStopAll.moveParams.source = AiGlobals.SensorSource.both; //action.moveParams.source == AiGlobals.SensorSource.phys
		}

		public IEnumerator NextAction() {
			float delayTime = 0;
			if (active) {
				if (currentAction < actions.Count) {
					if (DingEvent != null) {
						if (random) {
							_currentAction = UnityEngine.Random.Range(0, actions.Count);
						}

						actions[currentAction].stringIn = GetInputValue("stringIn", stringIn);
						actions[currentAction].floatIn = GetInputValue("floatIn", floatIn);
						// DingEvent(device, actions[currentAction]);

						switch (actions[currentAction].actionType) {
							case AiGlobals.ActionTypes.move:
								delayTime = actions[currentAction].moveParams.time;
								actionStopAll.moveParams.time = 0;
								actionStopAll.moveParams.type = AiGlobals.ActionMoveTypes.stop;
								actionStopAll.moveParams.source = actions[currentAction].moveParams.source;
								break;
							case AiGlobals.ActionTypes.leds:
								delayTime = actions[currentAction].ledParams.time;
								break;
							case AiGlobals.ActionTypes.delay:
								delayTime = actions[currentAction].delayParams.time;
								break;
							case AiGlobals.ActionTypes.servo:
								if (actions[currentAction].servoParams.portLabel == AiGlobals.ActionServoPorts.pan) {
									actions[currentAction].servoParams.port = 1;
								} else if (actions[currentAction].servoParams.portLabel == AiGlobals.ActionServoPorts.tilt) {
									actions[currentAction].servoParams.port = 2;
								} else if (actions[currentAction].servoParams.portLabel == AiGlobals.ActionServoPorts.s3) {
									actions[currentAction].servoParams.port = 3;
								} else if (actions[currentAction].servoParams.portLabel == AiGlobals.ActionServoPorts.s4) {
									actions[currentAction].servoParams.port = 4;
								}
								delayTime = actions[currentAction].servoParams.time;
								break;
							case AiGlobals.ActionTypes.textToSpeech:
								delayTime = actions[currentAction].speakParams.time;
								break;
							case AiGlobals.ActionTypes.playSound:
								delayTime = actions[currentAction].playSoundParams.time;
								break;
						}
						DingEvent(device, actions[currentAction]);
						if (delayTime > 0) {
							// wait for the specified time before going to the next action
							yield return new WaitForSeconds(delayTime);
							if (actions[currentAction].actionType == AiGlobals.ActionTypes.move) {
								// stop the move motors after the time
								DingEvent(device, actionStopAll);
							}
						} else {
							// always have a very short delay for OSC to settle
							//yield return new WaitForSeconds(0.02f);
						}
					}
					if (active) {
						if (random) {
							_currentAction = actions.Count;
						} else {
							_currentAction++;
						}
						NextAction().RunCoroutine();
					}
				} else {
					repeatCount++;
					if (repeatCount >= repeats) {
						Exit();
					} else {
						_currentAction = 0;
						NextAction().RunCoroutine();
					}

				}

			}
		}

		protected override void OnExit() {
			repeatCount = 0;
			_currentAction = -1;
		}

		protected override void OnEnter() {
			_currentAction = 0;
			repeatCount = 0;
			NextAction().RunCoroutine();
		}
	}

	[Serializable]
	public class Action {
		/// <summary> Replace {stringIn} with this value when executing this action </summary>
		public string stringIn;
		public float floatIn;
		[NodeEnum] public AiGlobals.ActionTypes actionType = AiGlobals.ActionTypes.leds;
		public ActionMove moveParams = new ActionMove();
		public ActionLed ledParams = new ActionLed();
		public ActionDelay delayParams = new ActionDelay();
		public ActionAnalogIn analoginParams = new ActionAnalogIn();
		public ActionTouch touchParams = new ActionTouch();
		public ActionServo servoParams = new ActionServo();
		public ActionSpeak speakParams = new ActionSpeak();
		public ActionListen listenParams = new ActionListen();
		public ActionRecognize recognizeParams = new ActionRecognize();
		//public ActionChat chatParams = new ActionChat();
		public ActionPlaySound playSoundParams = new ActionPlaySound();

	}

	[Serializable]
	public class ActionMove {
		[NodeEnum] public AiGlobals.ActionMoveTypes type = AiGlobals.ActionMoveTypes.forward;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
		[Tooltip("Time (Seconds)")]
		public float time = 1;
		[Tooltip("Speed")]
		public float speed = 0.5f;
		[Tooltip("Easing")]
		[NodeEnum] public AiGlobals.Easing easing = AiGlobals.Easing.easeInOut;
	}

	[Serializable]
	public class ActionLed {
		[NodeEnum] public AiGlobals.ActionLedTypes type = AiGlobals.ActionLedTypes.set;
		[Tooltip("Time (Seconds)")]
		public float time = 0;
		[Tooltip("Led Num")]
		public int ledNum = -1;
		[Tooltip("Color")]
		public Color32 color = new Color32(127, 127, 0, 255);
	}

	[Serializable]
	public class ActionDelay {
		[NodeEnum] public AiGlobals.ActionDelayTypes type = AiGlobals.ActionDelayTypes.pause;
		[Tooltip("Time (Seconds)")]
		public float time = 1;
	}

	[Serializable]
	public class ActionAnalogIn {
		[Tooltip("Interval (Milliseconds)")]
		public int interval = 50; // milliseconds
		[Tooltip("Port (Typically 0-5)")]
		public int port = 1;
		[NodeEnum] public AiGlobals.ActionAnalogInTypes type = AiGlobals.ActionAnalogInTypes.start;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
	}
[Serializable]
	public class ActionTouch {
		[Tooltip("Interval (Milliseconds)")]
		public int interval = 50; // milliseconds
		[Tooltip("Port (Typically 0-3)")]
		public int port = 1;
		[NodeEnum] public AiGlobals.ActionAnalogInTypes type = AiGlobals.ActionAnalogInTypes.start;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.phys;
	}


	[Serializable]
	public class ActionServo {
		[NodeEnum] public AiGlobals.ActionServoTypes type = AiGlobals.ActionServoTypes.immediate;
		[Tooltip("Time (Seconds)")]
		public float time = 1;
		[Tooltip("Angle (Degrees 0-180)")]
		public int angle = 90; // degrees 0 - 180
		[Tooltip("Port (pan, tilt, s3, s4)")]
		[NodeEnum] public AiGlobals.ActionServoPorts portLabel = AiGlobals.ActionServoPorts.pan;
		public int port = 9; // typically 9 & 10
		[Tooltip("Speed (0-255)")]
		public int varspeed = 127; // 0-255
		[Tooltip("Easting")]
		[NodeEnum] public AiGlobals.Easing easing = AiGlobals.Easing.easeInOut;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.phys;
	}

	[Serializable]
	public class ActionSpeak {
		[NodeEnum] public AiGlobals.ActionSpeakTypes type = AiGlobals.ActionSpeakTypes.enUS1;
		[Tooltip("Wait (Seconds)")]
		public float time = 1;
		[Tooltip("Utterance")]
		public string utterance = "Hello World";
		[Tooltip("Source")]
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
		[NodeEnum] public AiGlobals.VoiceModels model = AiGlobals.VoiceModels.pico;
	}

	[Serializable]
	public class ActionListen {
		[NodeEnum] public AiGlobals.ActionListenTypes type = AiGlobals.ActionListenTypes.timed;
		[Tooltip("Time Limit (Seconds)")]
		public float duration = 10;
		[Tooltip("Source")]
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
		[NodeEnum] public AiGlobals.ActionLang lang = AiGlobals.ActionLang.enUS;
		[NodeEnum] public AiGlobals.VoiceModels model = AiGlobals.VoiceModels.watson;
	}

	[Serializable]
	public class ActionChat {
		[NodeEnum] public AiGlobals.ActionChatTypes type = AiGlobals.ActionChatTypes.voice;
		[Tooltip("Time (Seconds)")]
		public float time = 1;
		[Tooltip("Text")]
		public string text = "Hello";
	}

	[Serializable]
	public class ActionRecognize {
		[NodeEnum] public AiGlobals.ActionRecognizeTypes type = AiGlobals.ActionRecognizeTypes.one;
		[NodeEnum] public AiGlobals.ActionRecognizeModels model = AiGlobals.ActionRecognizeModels.googlenet;
		public float minDistance = 5;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
	}

	[Serializable]
	public class ActionPlaySound {
		[NodeEnum] public AiGlobals.UISoundFiles type = AiGlobals.UISoundFiles.ClickPopTwoPart;
		[NodeEnum] public AiGlobals.SensorSource source = AiGlobals.SensorSource.virt;
		[Tooltip("Time (Seconds)")]
		public float time = 0.5f;
	}
}