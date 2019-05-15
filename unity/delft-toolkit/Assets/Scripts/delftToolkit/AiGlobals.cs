using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiGlobals : MonoBehaviour {

	public enum StringCompare {
		Contains,
		StartsWith,
		EqualTo,
		DoesNotContain
	}

	public enum ActionTypes {
		move,
		servo,
		leds,
		playSound,
		analogin,
		delay,
		textToSpeech,
		speechToText,
		recognize,
		chat,
	}

	public enum ActionMoveTypes {
		stop,
		forward,
		backward,
		turnRight,
		turnLeft
	};

	public enum ActionLedTypes {
		set,
		blink,
		allOff
	}

	public enum ActionDelayTypes {
		pause
	}

	public enum ActionAnalogInTypes {
		start,
		stop
	}

	public enum ActionServoTypes {
		immediate,
		varspeed
	}

	public enum ActionSpeakTypes {
		enUS1,
		enUS2,
		enGB1,
		esUS1,
		esES1,
		frFR1,
		itIT1,
		deDE1,
		deDE2
	}

	public enum ActionLang {
		enUS,
		enGB,
		deDE,
		esES,
		frFR,
		itIT
	}

	public enum ActionListenTypes {
		timed,
		auto
	}

	public enum ActionChatTypes {
		standard,
		voice
	}

	public enum ActionRecognizeTypes {
		one,
		multiple
	}
	public enum ActionRecognizeModels {
		squeezenet,
		alexnet,
		googlenet,
		inception,
		rcnn
	}

	public enum  VoiceModels {
		watson,
		pico
	}

	public enum WatsonServices {
		textToSpeech,
		speechToText,
		assistant,
		vision,
		personality,
		tone,
		discovery
	}

	public enum Devices {
		ding1,
		ding2,
		ding3,
		ding4
	};

	public enum Easing {
		none,
		easeIn,
		easeOut,
		easeInOut
	}

	public enum SensorSource {
		virt,
		phys,
		both
	}

	public enum UISoundFiles {
		UserSound1,
		UserSound2,
		UserSound3,
		UserSound4,
		UserSound5,
		BasicClickWooden,
		BeepSpaceButton,
		BlipPop,
		BlipSqueak,
		BonkClickWDenyFeel,
		BootSound,
		ButtonCasualEvent,
		ButtonConfirmSpacey,
		ButtonDenySpacey,
		ButtonSpaceyConfirm,
		CasualDeathLoose,
		Click,
		ClickBasic,
		ClickCasual,
		ClickHeavy,
		ClickHigher,
		ClickMetalTing,
		ClickPop,
		ClickPopTwoPart,
		ClickWithTwoParts,
		ClickWooden1,
		ClickWooden2,
		ComputerBeep1,
		ComputerBeep2,
		ConfirmClickSpacey,
		FlourishSpacey1,
		FlourishSpacey2,
		LooseDenyCasual1,
		LooseDenyCasual2,
		PingBing,
		PingSoundRicochet,
		PopClick,
		QuickUiOrEventDeep,
		RolloverLow,
		SpaceDrill,
		SpaceSwooshBrighter,
		SpaceSwooshDarker,
		Spacey1UpPowerUp,
		SpaceyCricketClick,
		SpaceyDrillQuick,
		SpaceyLoose,
		SpaceyQuick1,
		SpaceyQuick2,
		SpaceyRicochet,
		SpaceySmoothCricketClick,
		SpaceyTeleportRip,
		SpaceyTeleportRip2,
		TeleportCasual,
		TeleportDarker,
		TeleportHigh,
		TeleportMorphy,
		TeleportSlurp,
		TeleportSpaceMorph,
		TeleportSpacey,
		TeleportStaticky,
		WinSpacey
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
			if (b.Contains(",")) {
				// check for multiple strings, any of which must be in target
				string[] theStrings = b.Split(',');
				foreach (string token in theStrings) {
					comparison = a.ToLower().Contains(token.ToLower());
					if (comparison)
						break;
				}
				return comparison;
			} else {
				return a.ToLower().Contains(b.ToLower());
			}
		}
		if (cm == AiGlobals.StringCompare.StartsWith) {
			return a.ToLower().StartsWith(b.ToLower());
		}
		if (cm == AiGlobals.StringCompare.DoesNotContain) {
			bool comparison = true;
			if (b.Contains(",")) {
				// check for multiple strings, all of which must not be in target
				string[] theStrings = b.Split(',');
				foreach (string token in theStrings) {
					comparison = !(a.ToLower().Contains(token.ToLower()));
					if (!comparison)
						break;
				}
				return comparison;
			} else {
				return !a.ToLower().Contains(b.ToLower());
			}
		}
		return false;
	}

	void Start() {

	}

	void Update() {

	}
}