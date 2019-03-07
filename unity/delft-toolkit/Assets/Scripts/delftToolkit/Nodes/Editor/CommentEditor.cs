using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Comment))]
	public class CommentEditor : NodeEditor {
		private Comment node { get { return _node != null ? _node : _node = target as Comment; } }
		private Comment _node;

		public override void OnHeaderGUI() {
			GUILayout.Space(30);
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			serializedObject.FindProperty("text").stringValue = EditorGUILayout.TextArea(serializedObject.FindProperty("text").stringValue, DelftStyles.commentTextArea, GUILayout.MinHeight(60));
			serializedObject.ApplyModifiedProperties();
		}

		public override GUIStyle GetBodyStyle() {
			return DelftStyles.commentNodeBody;
		}
	}
}