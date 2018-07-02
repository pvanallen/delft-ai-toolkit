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
		Rect pos = new Rect(position.x, position.y, 50, EditorGUIUtility.singleLineHeight);
		EditorGUI.PropertyField(pos, actionType, new GUIContent());

		switch ((AiGlobals.ActionTypes) actionType.enumValueIndex) {
			case AiGlobals.ActionTypes.move:
				SerializedProperty actionMove = property.FindPropertyRelative("moveParams");
				DrawNextProperty(ref pos, actionMove, "type", 70);
				DrawNextProperty(ref pos, actionMove, "time", 30);
				DrawNextProperty(ref pos, actionMove, "speed", 30);
			break;
			case AiGlobals.ActionTypes.leds:
				SerializedProperty actionLeds = property.FindPropertyRelative("ledParams");
				DrawNextProperty(ref pos, actionLeds, "type", 70);
				DrawNextProperty(ref pos, actionLeds, "time", 30);
				DrawNextProperty(ref pos, actionLeds, "ledNum", 30);
				pos.x = position.x;
				pos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				pos.width = position.width;
				EditorGUI.BeginChangeCheck();
				Color32 col = ParseColor(actionLeds.FindPropertyRelative("color").stringValue);
				col = EditorGUI.ColorField(pos, col);
				if (EditorGUI.EndChangeCheck()) {
					actionLeds.FindPropertyRelative("color").stringValue = col.r + "," + col.g + "," + col.b;
				}
			break;
			case AiGlobals.ActionTypes.delay:
				SerializedProperty actionDelay = property.FindPropertyRelative("delayParams");
				DrawNextProperty(ref pos, actionDelay, "time", 30);
			break;
			case AiGlobals.ActionTypes.analogin:
				SerializedProperty actionAnalogIn = property.FindPropertyRelative("analoginParams");
				DrawNextProperty(ref pos, actionAnalogIn, "type", 70);
				DrawNextProperty(ref pos, actionAnalogIn, "interval", 30);
				DrawNextProperty(ref pos, actionAnalogIn, "port", 30);
			break;
            case AiGlobals.ActionTypes.speak:
                SerializedProperty actionSpeak = property.FindPropertyRelative("speakParams");
                DrawNextProperty(ref pos, actionSpeak, "type", 70);
                DrawNextProperty(ref pos, actionSpeak, "time", 30);
                pos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                pos.x = 8;
                DrawNextProperty(ref pos, actionSpeak, "text", 186);
                break;
		}

		EditorGUI.EndProperty();
	}

	private Color32 ParseColor(string s) {
		string[] s_rgb = s.Split(',');
		if (s_rgb.Length != 3) return Color.white;
		byte[] c_rgb = new byte[3];
		for (int i = 0; i < 3; i++) byte.TryParse(s_rgb[i], out c_rgb[i]);
		return new Color32(c_rgb[0],c_rgb[1],c_rgb[2], 255);
	}

	private void DrawNextProperty(ref Rect pos, SerializedProperty property, string relative, float width) {
		pos.x += pos.width + 2;
		pos.width = width;
		EditorGUI.PropertyField(pos, property.FindPropertyRelative(relative), new GUIContent());
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
	}
}