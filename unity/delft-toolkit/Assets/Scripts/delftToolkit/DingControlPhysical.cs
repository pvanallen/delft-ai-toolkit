// control the physical thing (ding) via OSC to a nodejs server which 
// communicates by Bluetooth to the robot
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class DingControlPhysical : DingControlBase {

    public string TargetAddr = "127.0.0.1";
    public int OutGoingPort = 8001;
    public int InComingPort = 3333;


    private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	private const string OSC_SERVER_CLIENT = "DelftDingOSC";

    // set up events for data received from the device
    public delegate void DingNumberEvent(AiGlobals.Devices device, string oscMessage, float val0, float val1, float val2);
    public static event DingNumberEvent DingNumPhysicalEvent;

    public delegate void DingStringEvent(AiGlobals.Devices device, string oscMessage, string val);
    public static event DingStringEvent DingStrPhysicalEvent;

    private long lastOscMessageIn = 0;


    // Script initialization
    void Awake() { 
		// using awake so that it happens before oscCentral initializes in Start()

        //OSCHandler.Instance.Init(); //init OSC
		OSCHandler.Instance.Init(OSC_SERVER_CLIENT, TargetAddr, OutGoingPort, InComingPort);
        servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string, ClientLog> ();
	}

    public override void Update() {
        OSCHandler.Instance.UpdateLogs();

        servers = OSCHandler.Instance.Servers;

        foreach (KeyValuePair<string, ServerLog> item in servers) {
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
                        float value0 = item.Value.packets[msgIndex].Data.Count > 0 ? float.Parse(item.Value.packets[msgIndex].Data[0].ToString()) : 0.0f;
                        float value1 = item.Value.packets[msgIndex].Data.Count > 1 ? float.Parse(item.Value.packets[msgIndex].Data[1].ToString()) : 0.0f;
                        float value2 = item.Value.packets[msgIndex].Data.Count > 2 ? float.Parse(item.Value.packets[msgIndex].Data[2].ToString()) : 0.0f;
                        //print(OSC_SERVER_CLIENT + ": " + address + " " + value0 + " " + value1 + " " + value2);
                        if (DingNumPhysicalEvent != null)
                            DingNumPhysicalEvent(thisDevice, address, value0, value1, value2);
                    } else if (address.StartsWith("/str/")) {
                        string value = item.Value.packets[msgIndex].Data.Count > 0 ? item.Value.packets[msgIndex].Data[0].ToString() : "null";
                        //print("sending Event" + address + value);
                        if (DingStrPhysicalEvent != null)
                            DingStrPhysicalEvent(thisDevice, address, value);
                    }
                }
                lastOscMessageIn = item.Value.packets[item.Value.packets.Count - 1].TimeStamp;
            }
        }
    }

	public override void handleAction () {
		//base.Update ();
		List<object> oscValues = new List<object>();

        string oscString = "/" + action.actionType + "/";

        //base.handleAction();
        switch (action.actionType)
        {
            case AiGlobals.ActionTypes.move:
                string moveType = action.moveParams.type.ToString();
                float moveTime = action.moveParams.time;
                float moveSpeed = action.moveParams.speed;
                string moveEasing = action.moveParams.easing.ToString();
                //Debug.LogWarning("DING-PHYSICAL: " + thisDevice.ToString() + " " + action.actionType + " " + action.moveParams.type.ToString());
                oscValues.AddRange(new object[] { moveType, moveTime, moveSpeed, moveEasing });
                break;
            case AiGlobals.ActionTypes.leds:
                string ledType = action.ledParams.type.ToString();
                float ledTime = action.ledParams.time;
                int ledNum = action.ledParams.ledNum;
                string ledColor = action.ledParams.color.ToCSV();
                //Debug.LogWarning("DING-PHYSICAL: " + thisDevice.ToString() + " " + action.actionType + " " + action.ledParams.type.ToString());
                oscValues.AddRange(new object[] { ledType, ledTime, ledNum, ledColor });
                break;
            case AiGlobals.ActionTypes.delay:
                //Debug.LogWarning("DING-PHYSICAL: " + thisDevice.ToString() + " " + action.actionType + " " + action.delayParams.type.ToString());
                string delayType = action.delayParams.type.ToString();
                float delayTime = action.delayParams.time;
                oscValues.AddRange(new object[] { delayType, delayTime });
                break;
            case AiGlobals.ActionTypes.analogin:
                string analoginType = action.analoginParams.type.ToString();
                int analoginInterval = action.analoginParams.interval;
                int analoginPort = action.analoginParams.port;
                //Debug.LogWarning("DING-PHYSICAL: " + thisDevice.ToString() + " " + action.actionType + " " + action.analoginParams.type.ToString());
                oscValues.AddRange(new object[] { analoginType, analoginInterval, analoginPort });
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