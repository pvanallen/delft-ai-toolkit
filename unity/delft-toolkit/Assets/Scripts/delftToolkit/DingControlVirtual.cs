using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

public class DingControlVirtual : DingControlBase {

    public bool sendSensors = false;
    public int analoginPort = 0;

    public delegate void DingNumberEvent(AiGlobals.Devices device, string oscMessage, float val0, float val1, float val2);
    public static event DingNumberEvent DingNumVirtualEvent;

    public delegate void DingStringEvent(AiGlobals.Devices device, string oscMessage, string val);
    public static event DingStringEvent DingStrVirtualEvent;

    public override void handleAction() {
        //base.handleAction();
        switch (action.actionType) {
            case AiGlobals.ActionTypes.move:
                //Debug.LogWarning("DING-VIRTUAL: " + action.actionType + " " + action.moveParams.type.ToString());
                break;
            case AiGlobals.ActionTypes.leds:
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
            if (DingNumVirtualEvent != null) {
                float x = transform.position.x * 100.0f;
                float y = transform.position.y * 100.0f;
                float z = transform.position.z * 100.0f;
                //print("sending xyz " + z);
                DingNumVirtualEvent(thisDevice, "/num/analogin/0/", z, x, y);
            }
        }

        if (action != null) {
            switch (action.actionType) {
                case AiGlobals.ActionTypes.move:
                    float moveAmount = action.moveParams.speed * speedAdj * Time.deltaTime;
                    switch (action.moveParams.type) {
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
                    break;
                case AiGlobals.ActionTypes.leds:
                    switch (action.ledParams.type) {
                        case AiGlobals.ActionLedTypes.set:
                            //this.GetComponent<Renderer>().material.color = new Color(0.236f, 0.0f, 0.5f);
                            GetComponent<Renderer>().material.color = action.ledParams.color;
                            break;
                        case AiGlobals.ActionLedTypes.allOff:
                            this.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f);
                            break;
                        default:
                            break;
                    }
                    break;
                case AiGlobals.ActionTypes.delay:
                    break;
                default:
                    break;
            }
        }
    }
}