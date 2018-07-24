using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEngine;

public class DingControlBase : MonoBehaviour {

	public AiGlobals.Devices thisDevice = AiGlobals.Devices.ding1;
	public float speedAdj = 1.0f;

	protected AiGlobals.Devices device;
	protected Action action = null;

	void OnEnable() {
		Actions.DingEvent -= setAction;
		Actions.DingEvent += setAction;
		//Condition.DingEvent += setAction;
	}

	void OnDisable() {
		Actions.DingEvent -= setAction;
		//Condition.DingEvent -= setAction;
	}

	void Start() {

	}

	public virtual void Update() {

	}

	void setAction(AiGlobals.Devices aDevice, Action anAction) {
		//Debug.LogWarning("ding received: " + action.actionType.ToString());
		if (aDevice == thisDevice) {
			device = aDevice;
			action = anAction;
			handleAction();
		}
	}

	public virtual void handleAction() {
		// override in child
	}
}