using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

namespace DelftToolkit {
    [System.Serializable]
    public class Condition : StateNodeBase {
        // Adding [Input] or [Output] is all you need to do to register a field as a valid port on your node 
        [Input] public float a;
        [Input] public float b;
        // The value of an output node field is not used for anything, but could be used for caching output results
        [Output] public float result;

        public AiGlobals.Devices sensorDevice = AiGlobals.Devices.ding1;
        public string matchDingMessage = "/num/analogin/0/";
        public AiGlobals.SensorSource sensorSource = AiGlobals.SensorSource.virt;
        public string incomingDingMessage = "";
        private float lastResult = 0;

        //public delegate void DingActionEvent(AiGlobals.Devices device, Action action);
        //public static event DingActionEvent DingEvent;

        // Will be displayed as an editable field - just like the normal inspector
        public MathType mathType = MathType.Add;
        public enum MathType { Add, Subtract, Multiply, Divide }

        protected override void Init() {
            base.Init();
            DingControlPhysical.DingNumPhysicalEvent += handlePhysNumEvent;
            DingControlVirtual.DingNumVirtualEvent += handleVirtNumEvent;
        }

        void handlePhysNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
            if (sensorSource == AiGlobals.SensorSource.phys) {
                handleNumEvent(device, adrs, val0, val1, val2);
            }
        }

        void handleVirtNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {

            if (sensorSource == AiGlobals.SensorSource.virt) {
                handleNumEvent(device, adrs, val0, val1, val2);
            }
        }

        void handleNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {

            incomingDingMessage = adrs;
            //UnityEngine.Debug.Log("got virt " + incomingDingMessage + " " + matchDingMessage);
            if (incomingDingMessage == matchDingMessage && sensorDevice == device) {
                a = val0;
                GetValue(GetOutputPort("result"));
                //if (result > 50) {
                //node.MoveNext();
                //}
            } else {
                //value0 = 0;
            }
            //UnityEngine.Debug.Log("DING DATA Condition (" + adrs + "): " + val0);
        }

        // GetValue should be overridden to return a value for any specified output port
        public override object GetValue(XNode.NodePort port) {

            // Get new a and b values from input connections. Fallback to field values if input is not connected
            float a = GetInputValue<float>("a", this.a);
            float b = GetInputValue<float>("b", this.b);

            // After you've gotten your input values, you can perform your calculations and return a value
            result = 0f;
            if (port.fieldName == "result")
                switch (mathType) {
                    case MathType.Add:
                    default:
                        result = a + b;
                        break;
                    case MathType.Subtract:
                        result = a - b;
                        break;
                    case MathType.Multiply:
                        result = a * b;
                        break;
                    case MathType.Divide:
                        result = a / b;
                        break;
                }
            //UnityEngine.Debug.Log("last: " + lastResult);
            if (result > 50 && lastResult <= 50) {
                MoveNext();
            }
            lastResult = result;
            return result;
        }
    }
}