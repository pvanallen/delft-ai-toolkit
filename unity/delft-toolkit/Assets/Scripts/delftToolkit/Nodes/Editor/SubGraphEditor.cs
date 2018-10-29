using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(SubGraph))]
	public class SubGraphEditor : StateNodeBaseEditor {

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortPair(target.GetInputPort("enter"), target.GetOutputPort("exit"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("subGraph"), GUIContent.none);
			DrawFooterGUI();
		}
	}
}