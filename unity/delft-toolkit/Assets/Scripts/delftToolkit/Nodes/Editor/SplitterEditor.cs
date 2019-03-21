using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Splitter))]
	public class SplitterEditor : StateNodeBaseEditor {

		protected override string description { get { return "Splits input triggers to different outputs"; } }
		private Splitter node { get { return _node != null ? _node : _node = target as Splitter; } }
		private Splitter _node;

		public override void OnBodyGUI() {
			GUI.color = Color.white;
			NodeEditorGUILayout.PortField(target.GetInputPort("enter"));
			Rect rect = GUILayoutUtility.GetLastRect();
			rect.x += 47;
			rect.width = 65;
			EditorGUI.PropertyField(rect, serializedObject.FindProperty("splitType"), GUIContent.none);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Outlets:", GUILayout.Width(43));
			node.outlets = Mathf.Clamp(EditorGUILayout.IntField(node.outlets, GUILayout.Width(22)),2,50);

			node.changeCheck();
			
			EditorGUILayout.LabelField("Next:", GUILayout.Width(35));
			node.nextOutletNum = EditorGUILayout.IntField(node.nextOutletNum, GUILayout.Width(22));
			GUILayout.EndHorizontal();
			
			// draw the outlets
			for (int i=0;i<node.outlets;i++) {	
				NodeEditorGUILayout.PortField(target.GetOutputPort("out" + i));
			}
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			DrawFooterGUI();
		}
	}
}