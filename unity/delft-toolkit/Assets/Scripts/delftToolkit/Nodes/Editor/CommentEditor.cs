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

		private GUIStyle headerStyle {
			get {
				if (_headerStyle == null) {
					_headerStyle = new GUIStyle(NodeEditorResources.styles.nodeHeader);
					_headerStyle.normal.textColor = Color.black;
					_headerStyle.alignment = TextAnchor.MiddleLeft;
				}
				return _headerStyle;
			}
		}
		private GUIStyle _headerStyle;

		public override void OnHeaderGUI() {
			GUILayout.Label(target.name, headerStyle, GUILayout.Height(30));
		}

		public override void OnBodyGUI() {
			serializedObject.Update();
			Rect size = GUILayoutUtility.GetRect(new GUIContent(node.text), DelftStyles.commentTextArea);
			serializedObject.FindProperty("text").stringValue = EditorGUI.TextArea(size, serializedObject.FindProperty("text").stringValue, DelftStyles.commentTextArea/*, GUILayout.Height(size.y)*/);
			serializedObject.ApplyModifiedProperties();
		}

		public override GUIStyle GetBodyStyle() {
			return DelftStyles.commentNodeBody;
		}
	}
}