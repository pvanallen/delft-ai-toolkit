using System;
using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;
using XNode;

// to fix
//  change name to splitter with two Ts
//  add next outlet field
//  change random to pre-calculated full random
//  code sequential

namespace DelftToolkit {
	/// <summary> Splitter Node. Routes triggers </summary>
	[CreateNodeMenu("Graph Control/Splitter"), NodeWidth(170), NodeTint(0f, 1f, 1f)]
    //[NodeTint(0f, 1f, 1f)]
	public class Splitter : StateNodeBase {

        public int outlets = 4;
        public int lastOutlets = 0;
        public int nextOutletNum = -1;

        public enum splitterType {
            random,
            sequential
	    } 

        [NodeEnum]
        public splitterType splitType = splitterType.random;

        private bool nodeInitialized = false;

        protected override void Init() { 
            lastOutlets = outlets;
            if (!nodeInitialized) {
                for (int i=0;i<outlets;i++) {
                    AddInstanceOutput(typeof(Empty), fieldName: "out" + i);
                } 
                nextOutlet();
                nodeInitialized = true;
            }  
        }
		protected override void OnEnter() {
            NodePort triggerPort = GetOutputPort("out" + nextOutletNum);
            if (triggerPort.IsConnected) {
                for (int k = 0; k < triggerPort.ConnectionCount; k++) {
                    StateNodeBase nextNode = triggerPort.GetConnection(k).node as StateNodeBase;
                    active = false;
                    if (nextNode != null) nextNode.Enter();
                }
            }
            nextOutlet();
		}

		protected override void OnExit() {
			return;
		}

        public void nextOutlet() {
            if (splitType == splitterType.random) {
                int lastOutletNum = nextOutletNum;
                while (lastOutletNum == nextOutletNum) {
                    nextOutletNum = UnityEngine.Random.Range(0,outlets);
                } 
            } else if (splitType == splitterType.sequential) {
                nextOutletNum = (nextOutletNum + 1) % outlets;
            }
        }

	}
}