using System.Collections;
using System.Collections.Generic;
using DelftToolkit;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Action))]
public class DelftActionDrawer : PropertyDrawer {
	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		SerializedProperty actionType = property.FindPropertyRelative("actionType");
		Rect pos = new Rect(position.x, position.y, 85, EditorGUIUtility.singleLineHeight);
		EditorGUI.PropertyField(pos, actionType, new GUIContent());
		pos.x += pos.width + 2;
		EditorGUIUtility.labelWidth = 20;

		switch ((AiGlobals.ActionTypes) actionType.enumValueIndex) {
			case AiGlobals.ActionTypes.move:
				//{}
				SerializedProperty actionMove = property.FindPropertyRelative("moveParams");
				DrawNextProperty(ref pos, actionMove, "type", 70, GUIContent.none);
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionMove, "time", 60, new GUIContent(DelftStyles.timeIcon));
				DrawNextProperty(ref pos, actionMove, "speed", 50, new GUIContent(DelftStyles.speedIcon));
				break;
			case AiGlobals.ActionTypes.leds:
				SerializedProperty actionLeds = property.FindPropertyRelative("ledParams");
				DrawNextProperty(ref pos, actionLeds, "type", 70, GUIContent.none);
				DrawNextProperty(ref pos, actionLeds, "ledNum", 50, new GUIContent(DelftStyles.portIcon));
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionLeds, "time", 60, new GUIContent(DelftStyles.timeIcon));
				DrawNextProperty(ref pos, actionLeds, "color", 70, GUIContent.none);
				break;
			case AiGlobals.ActionTypes.delay:
				SerializedProperty actionDelay = property.FindPropertyRelative("delayParams");
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionDelay, "time", 60, new GUIContent(DelftStyles.timeIcon));
				break;
			case AiGlobals.ActionTypes.analogin:
				SerializedProperty actionAnalogIn = property.FindPropertyRelative("analoginParams");
				DrawNextProperty(ref pos, actionAnalogIn, "type", 70, GUIContent.none);
				DrawNextProperty(ref pos, actionAnalogIn, "port", 50, new GUIContent(DelftStyles.portIcon));
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionAnalogIn, "interval", 60, new GUIContent(DelftStyles.timeIcon));
				break;
			case AiGlobals.ActionTypes.servo:
				SerializedProperty actionServo = property.FindPropertyRelative("servoParams");
				DrawNextProperty(ref pos, actionServo, "type", 70, GUIContent.none);
				DrawNextProperty(ref pos, actionServo, "port", 50, new GUIContent(DelftStyles.portIcon));
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionServo, "time", 60, new GUIContent(DelftStyles.timeIcon));
				DrawNextProperty(ref pos, actionServo, "angle", 50, new GUIContent(DelftStyles.angleIcon));
				break;
			case AiGlobals.ActionTypes.textToSpeech:
				SerializedProperty actionSpeak = property.FindPropertyRelative("speakParams");
				DrawNextProperty(ref pos, actionSpeak, "model", 49, GUIContent.none);
				DrawNextProperty(ref pos, actionSpeak, "source", 37, GUIContent.none);
				
				DrawNextProperty(ref pos, actionSpeak, "time", 25, GUIContent.none);
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionSpeak, "type", 50, GUIContent.none);
				DrawNextProperty(ref pos, actionSpeak, "utterance", 162, GUIContent.none);
				break;
			case AiGlobals.ActionTypes.speechToText:
				SerializedProperty actionListen = property.FindPropertyRelative("listenParams");
				DrawNextProperty(ref pos, actionListen, "source", 40, GUIContent.none);
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionListen, "lang", 45, GUIContent.none);
				DrawNextProperty(ref pos, actionListen, "duration", 45, new GUIContent(DelftStyles.timeIcon));
				break;
			case AiGlobals.ActionTypes.recognize:
				SerializedProperty actionRecognize = property.FindPropertyRelative("recognizeParams");
				//DrawNextProperty(ref pos, actionRecognize, "type", 70, GUIContent.none);
				DrawNextProperty(ref pos, actionRecognize, "model", 70, GUIContent.none);
				break;
			case AiGlobals.ActionTypes.playSound:
				SerializedProperty actionPlaySound = property.FindPropertyRelative("playSoundParams");
				DrawNextProperty(ref pos, actionPlaySound, "source", 40, GUIContent.none);
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionPlaySound, "time", 45, new GUIContent(DelftStyles.timeIcon));
				DrawNextProperty(ref pos, actionPlaySound, "type", 140, GUIContent.none);
				break;
			case AiGlobals.ActionTypes.chat:
				SerializedProperty actionChat = property.FindPropertyRelative("chatParams");
				DrawNextProperty(ref pos, actionChat, "type", 70, GUIContent.none);
				DrawNextProperty(ref pos, actionChat, "time", 60, new GUIContent(DelftStyles.timeIcon));
				NextLine(ref pos);
				DrawNextProperty(ref pos, actionChat, "text", position.width, GUIContent.none);
				break;
			default:
				EditorGUI.LabelField(pos, "ActionType not supported: " + (AiGlobals.ActionTypes) actionType.enumValueIndex);
				break;
		}

		EditorGUI.EndProperty();
	}

	private Color32 ParseColor(string s) {
		string[] s_rgb = s.Split(',');
		if (s_rgb.Length != 3) return Color.white;
		byte[] c_rgb = new byte[3];
		for (int i = 0; i < 3; i++) byte.TryParse(s_rgb[i], out c_rgb[i]);
		return new Color32(c_rgb[0], c_rgb[1], c_rgb[2], 255);
	}

	private void DrawNextProperty(ref Rect pos, SerializedProperty property, string relative, float width, GUIContent content) {
		SerializedProperty relativeProp = property.FindPropertyRelative(relative);
		if (relativeProp == null) {
			Debug.LogWarning("Relative property not found (" + relative + ")");
			return;
		}
		pos.width = width;
		EditorGUI.PropertyField(pos, relativeProp, content);
		pos.x += pos.width + 2;
	}

	private void NextLine(ref Rect pos) {
		pos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		pos.x = 36;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.standardVerticalSpacing;
	}
}