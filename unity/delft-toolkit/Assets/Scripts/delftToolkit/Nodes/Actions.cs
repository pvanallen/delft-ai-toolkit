using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelftToolkit {
    [NodeWidth(270)]
    public class Actions : StateNodeBase {

        public List<Action> actions = new List<Action> { new Action() };

        /// <summary> How many times should we repeat the actions list </summary>
        public int repeats = 1;
        /// <summary> If true, will pick a random action from the list instead of looping through it </summary>
        public bool random = false;
        /// <summary> How many times have we repeated so far </summary>
        public int repeatCount { get; private set; }
        /// <summary> Current action index </summary>
        public int currentAction { get; private set; }
        public AiGlobals.Devices device = AiGlobals.Devices.ding1;

        private Action actionStopAll = new Action();

        public delegate void DingActionEvent(AiGlobals.Devices device, Action action);
        public static event DingActionEvent DingEvent;

        protected override void Init() {
            actionStopAll.moveParams.time = 0;
        }

		public IEnumerator NextAction() {
			float delayTime = 0;
			if (active) {
				if (currentAction < actions.Count) {
					if (DingEvent != null) {
						if (random) {
							currentAction = UnityEngine.Random.Range(0, actions.Count);
						}

						DingEvent(device, actions[currentAction]);

						switch (actions[currentAction].actionType) {
							case AiGlobals.ActionTypes.move:
								delayTime = actions[currentAction].moveParams.time;
								break;
							case AiGlobals.ActionTypes.leds:
								delayTime = actions[currentAction].ledParams.time;
								break;
							case AiGlobals.ActionTypes.delay:
								delayTime = actions[currentAction].delayParams.time;
								break;
						}

						if (delayTime > 0) {
							yield return new WaitForSeconds(delayTime);
							if (actions[currentAction].actionType == AiGlobals.ActionTypes.move) {
								DingEvent(device, actionStopAll);
							}
						}
					}
					if (active) {
						if (random) {
							currentAction = actions.Count;
						} else {
							currentAction++;
						}
						NextAction().RunCoroutine();
					}
				} else {
					repeatCount++;
					if (repeatCount >= repeats) {

						Exit();
					} else {
						currentAction = 0;
						NextAction().RunCoroutine();
					}

				}

			}
		}

        public override void OnExit() {
			repeatCount = 0;
			currentAction = 0;
        }

        public override void OnEnter() {
            base.OnEnter();
            currentAction = 0;
            repeatCount = 0;
            NextAction().RunCoroutine();
        }
    }

    [Serializable]
    public class Action {
        public AiGlobals.ActionTypes actionType = AiGlobals.ActionTypes.move;
        public ActionMove moveParams = new ActionMove();
        public ActionLed ledParams = new ActionLed();
        public ActionDelay delayParams = new ActionDelay();
        public ActionAnalogIn analoginParams = new ActionAnalogIn();
    }

    [Serializable]
    public class ActionMove {
        public AiGlobals.ActionMoveTypes type = AiGlobals.ActionMoveTypes.stop;
        public float time = 1;
        public float speed = 1;
        public AiGlobals.Easing easing = AiGlobals.Easing.easeInOut;
    }

    [Serializable]
    public class ActionLed {
        public AiGlobals.ActionLedTypes type = AiGlobals.ActionLedTypes.set;
        public float time = 0;
        public int ledNum = 0;
        public Color32 color = new Color32(127, 127, 0, 255);
    }

    [Serializable]
    public class ActionDelay {
        public AiGlobals.ActionDelayTypes type = AiGlobals.ActionDelayTypes.pause;
        public float time = 1;
    }

    [Serializable]
    public class ActionAnalogIn {
        public AiGlobals.ActionAnalogInTypes type = AiGlobals.ActionAnalogInTypes.start;
        public int interval = 20; // milliseconds
        public int port = 0;
    }
}