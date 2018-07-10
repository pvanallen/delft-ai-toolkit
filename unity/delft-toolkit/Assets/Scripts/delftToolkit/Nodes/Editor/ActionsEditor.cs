using System.Collections;
using System.Collections.Generic;
using Rotorz.ReorderableList;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Actions))]
	public class ActionsEditor : StateNodeBaseEditor {

		private Actions node { get { return _node != null ? _node : _node = target as Actions; } }
		private Actions _node;
		private DelftActionListAdaptor actionListAdaptor;

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

			SerializedProperty p = serializedObject.FindProperty("actions");

			string title = "Actions";
			if (Application.isPlaying) title = "Actions (" + node.repeatCount + "/" + node.repeats + " repeats)";

			SerializedProperty actionsProperty = serializedObject.FindProperty("actions");

			if (actionsProperty.isExpanded = EditorGUILayout.Foldout(actionsProperty.isExpanded, title)) {
				if (actionListAdaptor == null) actionListAdaptor = new DelftActionListAdaptor(node.actions, node);
				Rotorz.ReorderableList.ReorderableListGUI.ListField(actionListAdaptor);
			}

			DrawFooterGUI();
		}
	}
}