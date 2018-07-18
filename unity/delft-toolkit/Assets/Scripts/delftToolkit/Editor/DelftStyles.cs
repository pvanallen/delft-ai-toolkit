using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DelftToolkit {
	/// <summary> Utility class for storing various GUI styles used by Delft Toolkit </summary>
	public static class DelftStyles {
		private static GUIStyle _labelNoHighlight;
		public static GUIStyle labelNoHighlight {
			get {
				if (_labelNoHighlight == null) _labelNoHighlight = RemoveHighlight(EditorStyles.label);
				return _labelNoHighlight;
			}
		}

		private static GUIStyle _foldoutNoHighlight;
		public static GUIStyle foldoutNoHighlight {
			get {
				if (_foldoutNoHighlight == null) _foldoutNoHighlight = RemoveHighlight(EditorStyles.foldout);
				return _foldoutNoHighlight;
			}
		}

		private static GUIStyle RemoveHighlight(GUIStyle original) {
			GUIStyle style = new GUIStyle(original);
			style.active.textColor = style.normal.textColor;
			style.focused.textColor = style.normal.textColor;
			style.hover.textColor = style.normal.textColor;
			style.active.background = style.normal.background;
			style.focused.background = style.normal.background;
			style.hover.background = style.normal.background;
			style.onActive.textColor = style.onNormal.textColor;
			style.onFocused.textColor = style.onNormal.textColor;
			style.onHover.textColor = style.onNormal.textColor;
			style.onActive.background = style.onNormal.background;
			style.onFocused.background = style.onNormal.background;
			style.onHover.background = style.onNormal.background;
			return style;
		}
	}
}