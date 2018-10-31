using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Actions))]
	public class ActionsEditor : StateNodeBaseEditor {

		protected override string description { get { return "Plays a list of configurable actions"; } }
		private Actions node { get { return _node != null ? _node : _node = target as Actions; } }
		private Actions _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortPair(target.GetInputPort("enter"), target.GetOutputPort("exit"));

			Rect rect = GUILayoutUtility.GetLastRect();
			rect.x += (rect.width * 0.5f) - 50;
			rect.width = 100;
			node.device = (AiGlobals.Devices) EditorGUI.EnumPopup(rect, node.device);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Repeats:", GUILayout.Width(60));
			node.repeats = EditorGUILayout.IntField(node.repeats, GUILayout.Width(33));
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Random:", GUILayout.Width(60));
			node.random = EditorGUILayout.Toggle(node.random);
			GUILayout.EndHorizontal();

			// Display the valueIn port.
			NodePort valueInPort = node.GetInputPort("valueIn");
			if (valueInPort.IsConnected) {
				// Display an uneditable input value if connected
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.TextField(serializedObject.FindProperty("valueIn").displayName, valueInPort.GetInputValue<string>());
				EditorGUI.EndDisabledGroup();
				NodeEditorGUILayout.AddPortField(valueInPort);
			} else NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("valueIn"), valueInPort, true);

			string title = "Actions";
			if (Application.isPlaying) title = "Actions (" + node.repeatCount + "/" + node.repeats + " repeats)";

			// Actions list
			NodeEditorGUILayout.onCreateReorderableList += OnCreateReorderableList;
			SerializedProperty actionsProperty = serializedObject.FindProperty("actions");
			if (actionsProperty.isExpanded = EditorGUILayout.Foldout(actionsProperty.isExpanded, title, DelftStyles.foldoutNoHighlight)) {
				NodeEditorGUILayout.PropertyField(actionsProperty);
			}
			NodeEditorGUILayout.onCreateReorderableList -= OnCreateReorderableList;

			// Footer
			DrawFooterGUI();
		}

		private void OnCreateReorderableList(ReorderableList list) {
			list.drawElementCallback =
				(Rect rect, int index, bool isActive, bool isFocused) => {
					XNode.NodePort port = node.GetPort("actions " + index);
					SerializedProperty itemData = serializedObject.FindProperty("actions").GetArrayElementAtIndex(index);
					if (node.currentAction == index) EditorGUI.DrawRect(rect, Color.gray);
					EditorGUI.PropertyField(rect, itemData);
					Vector2 pos = rect.position + (port.IsOutput ? new Vector2(rect.width + 6, 0) : new Vector2(-36, 0));
					NodeEditorGUILayout.PortField(pos, port);
				};
		}
	}
}