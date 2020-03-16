using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(FloatCondition))]
	public class FloatConditionEditor : StateNodeBaseEditor {

		protected override string description { get { return "An arbitrary condition taking a float argument"; } }
		private FloatCondition node { get { return _node != null ? _node : _node = target as FloatCondition; } }
		private FloatCondition _node;
		private bool expandLastSignal = false;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));
			//Debug.Log(node.filter.messageFilter);
			// Get port
			NodePort port = node.GetInputPort("value");
			SerializedProperty filterProperty = serializedObject.FindProperty("filter");
			SerializedProperty inputTypeProperty = serializedObject.FindProperty("inputType");
			//SerializedProperty inputProperty = serializedObject.FindProperty("inputType");
			//GUIContent filterContent = new GUIContent("Condition signal filter", filterProperty.tooltip);
			//if (filterProperty.isExpanded = EditorGUILayout.Foldout(filterProperty.isExpanded, filterContent, DelftStyles.foldoutNoHighlight)) {
				//EditorGUI.indentLevel++;
				//Debug.Log(node.filter.messageFilter);
				EditorGUI.BeginChangeCheck();

				NodeEditorGUILayout.PropertyField(filterProperty, GUIContent.none);
				NodeEditorGUILayout.PropertyField(inputTypeProperty, GUIContent.none);
				//string oscMessage = filterProperty.FindPropertyRelative("messageFilter");
				//EditorGUILayout.PropertyField(inputTypeProperty, GUILayout.Width(33));
				if (EditorGUI.EndChangeCheck()) {
					//Debug.Log(node.filter.messageFilter);
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
				}
				if (node.inputType == AiGlobals.FloatConditionType.analogin) {
					//Debug.Log("analogin selected");
					node.filter.messageFilter = "/num/analogin/" + node.filter.port + "/";
					
				} else if (node.inputType == AiGlobals.FloatConditionType.touch) {
					//Debug.Log("touch selected");
					node.filter.messageFilter = "/num/touch/" + node.filter.port + "/";
				} else if (node.inputType == AiGlobals.FloatConditionType.touchOsc) {
					//Debug.Log("touchOSC selected");
					node.filter.messageFilter = "/num/1/push" + node.filter.port + "/"; // /num/1/push1/
				} else if (node.inputType == AiGlobals.FloatConditionType.cleanOsc) {
					//Debug.Log("cleanOSC selected");
					node.filter.messageFilter = "/num/clean__project_1__button_" + node.filter.port + "/"; // /num/clean__project_1__button_1/
				} else if (node.inputType == AiGlobals.FloatConditionType.any) {
					// leave the message filter alone
				}
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				
				//EditorGUI.indentLevel--;
			//}

			//if (expandLastSignal = EditorGUILayout.Foldout(expandLastSignal, "Last Signal", DelftStyles.foldoutNoHighlight)) {
				EditorGUI.indentLevel++;
				if (node.signal.isValid) EditorGUILayout.SelectableLabel(node.signal.device + ":" + node.signal.oscMessage + " " + node.signal.value.ToString());
				else EditorGUILayout.LabelField("no matching float data yet");
				EditorGUI.indentLevel--;
			//}

			// Input value
			EditorGUI.BeginChangeCheck();
			float val = node.value;
			val = EditorGUILayout.Slider(val, 0f, 1023f);
			NodeEditorGUILayout.AddPortField(node.GetOutputPort("valueOut"));
			if (EditorGUI.EndChangeCheck()) node.value = val;

			EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
			NodeEditorGUILayout.InstancePortList("conditions", typeof(StateNodeBase.Empty), serializedObject, XNode.NodePort.IO.Output, XNode.Node.ConnectionType.Multiple);

			DrawFooterGUI();
		}
	}

	[CustomPropertyDrawer(typeof(FloatCondition.Condition))]
	public class FloatConditionDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			property.serializedObject.Update();

			int elementIndex;
			string[] pathParts = property.propertyPath.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length >= 2 && int.TryParse(pathParts[1], out elementIndex)) {
				FloatCondition node = property.serializedObject.targetObject as FloatCondition;
				if (node && node.conditions.Length > elementIndex) {
					if (node.conditions[elementIndex].lastState) GUI.color = Color.green;
				}
			}
			GUI.Box(position, "");
			GUI.color = Color.white;

			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();
			Rect ifRect = new Rect(position.x, position.y, 30, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(ifRect, "if x");
			Rect inverseRect = new Rect((position.x + position.width) - 20, position.y, 30, EditorGUIUtility.singleLineHeight);
			Rect inverseLabelRect = new Rect((position.x + position.width) - 30, position.y, 10, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(inverseLabelRect, new GUIContent("!", "Inverse"));
			EditorGUI.PropertyField(inverseRect, property.FindPropertyRelative("inverse"), GUIContent.none);
			Rect compareTypeRect = new Rect(position.x + 30, position.y, position.width - 70, EditorGUIUtility.singleLineHeight);
			SerializedProperty compareTypeProperty = property.FindPropertyRelative("compareType");
			EditorGUI.PropertyField(compareTypeRect, compareTypeProperty, GUIContent.none);

			FloatCondition.Condition.CompareType compareType = (FloatCondition.Condition.CompareType) compareTypeProperty.enumValueIndex;
			Rect compareValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
			SerializedProperty floatValAProperty = property.FindPropertyRelative("floatValA");
			SerializedProperty floatValBProperty = property.FindPropertyRelative("floatValB");
			float valA = floatValAProperty.floatValue;
			float valB = floatValBProperty.floatValue;

			switch (compareType) {
				case FloatCondition.Condition.CompareType.GT:
				case FloatCondition.Condition.CompareType.LT:
				valA = EditorGUI.Slider(compareValueRect, valA, 0, 1023);
				break;

				case FloatCondition.Condition.CompareType.Range:
				Rect aRect = new Rect(compareValueRect.x, compareValueRect.y, 40, compareValueRect.height);
				Rect bRect = new Rect(compareValueRect.x + compareValueRect.width - 40, compareValueRect.y, 40, compareValueRect.height);
				compareValueRect.x += 42;
				compareValueRect.width -= 84;
				valA = EditorGUI.FloatField(aRect, valA);
				valB = EditorGUI.FloatField(bRect, valB);
				EditorGUI.MinMaxSlider(compareValueRect, GUIContent.none, ref valA, ref valB, 0f, 1023f);
				break;
			}
			if (EditorGUI.EndChangeCheck()) {
				valB = Mathf.Max(valB, valA);
				floatValAProperty.floatValue = valA;
				floatValBProperty.floatValue = valB;
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}