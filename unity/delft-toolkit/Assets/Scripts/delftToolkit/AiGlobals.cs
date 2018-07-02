using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGlobals : MonoBehaviour {

	public enum StringCompare
	{
		Contains,
		StartsWith,
		EqualTo,
		DoesNotContain
	}

    public enum ActionTypes {
        move, leds, delay, analogin, speak
    }

    public enum ActionMoveTypes {
		stop, forward, backward, turnRight, turnLeft
	};

    public enum ActionLedTypes {
        set, blink, allOff
    }

    public enum ActionDelayTypes {
        pause
    }

    public enum ActionAnalogInTypes {
        start, stop
    }

    public enum ActionSpeakTypes {
        voice
    }

	public enum Devices{
		ding1, ding2, ding3, ding4
	};

    public enum Easing {
        none, easeIn, easeOut, easeInOut
    }

    public enum SensorSource {
        virt, phys
    }


	public static string GetCompareString(StringCompare cm) {
		if (cm == AiGlobals.StringCompare.EqualTo) {
			return "EqualTo";
		} 
		if (cm == AiGlobals.StringCompare.Contains) {
			return "Contains";
		}
		if (cm == AiGlobals.StringCompare.StartsWith) {
			return "StartsWith";
		}
		if (cm == AiGlobals.StringCompare.DoesNotContain) {
			return "DoesNotContain";
		}
		return string.Empty;
	}

	public static bool CompareString(string a, string b, StringCompare cm) {
		if (cm == AiGlobals.StringCompare.EqualTo) {
			return a.ToLower() == b.ToLower();
		} 
		if (cm == AiGlobals.StringCompare.Contains) {
			//return a.ToLower().Contains(b.ToLower());
			bool comparison = false;
			if (b.Contains (",")) {
				// check for multiple strings, any of which must be in target
				string[] theStrings = b.Split (',');
				foreach (string token in theStrings) {
					comparison = a.ToLower ().Contains (token.ToLower ());
					if (comparison)
						break;
				}
				return comparison;
			} else {
				return a.ToLower ().Contains (b.ToLower ());
			}
		}
		if (cm == AiGlobals.StringCompare.StartsWith) {
			return a.ToLower().StartsWith(b.ToLower());
		}
		if (cm == AiGlobals.StringCompare.DoesNotContain) {
			bool comparison = true;
			if (b.Contains (",")) {
				// check for multiple strings, all of which must not be in target
				string[] theStrings = b.Split (',');
				foreach (string token in theStrings) {
					comparison = !(a.ToLower ().Contains (token.ToLower ()));
					if (!comparison)
						break;
				}
				return comparison;
			} else {
				return !a.ToLower ().Contains (b.ToLower ());
			}
		}
		return false;
	}

	void Start () {
		
	}
	
	void Update () {
		
	}
}
