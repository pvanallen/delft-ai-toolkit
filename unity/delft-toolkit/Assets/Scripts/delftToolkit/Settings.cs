using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DelftAIToolkitSettings", menuName = "DelftAIToolkit Settings", order = 1)]
public class Settings : ScriptableObject {
    

    [System.Serializable]
    public struct Ding {
        public AiGlobals.Devices device;
        public string robotIP;
	    public int robotOutPort;
	    public int robotInPort;
        public string marionetteIP;
	    public int marionetteOutPort;
	    public int marionetteInPort;
    }

    [System.Serializable]
    public struct WatsonService {
        public AiGlobals.WatsonServices service;
        public string iamKey;
        public string url;
    }

    public Ding[] dings;
    public WatsonService[] watsonServices;

}