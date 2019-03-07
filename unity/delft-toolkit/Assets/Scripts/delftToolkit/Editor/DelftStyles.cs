using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
    /// <summary> Utility class for storing various GUI styles used by Delft Toolkit </summary>
    public static class DelftStyles {
        private static Texture _speedIcon;
        public static Texture speedIcon { get { return _speedIcon != null ? _speedIcon : _speedIcon = Resources.Load<Texture2D>("delfticon_speed"); } }

        private static Texture _portIcon;
        public static Texture portIcon { get { return _portIcon != null ? _portIcon : _portIcon = Resources.Load<Texture2D>("delfticon_port"); } }

        private static Texture _timeIcon;
        public static Texture timeIcon { get { return _timeIcon != null ? _timeIcon : _timeIcon = Resources.Load<Texture2D>("delfticon_time"); } }

        private static Texture _angleIcon;
        public static Texture angleIcon { get { return _angleIcon != null ? _angleIcon : _angleIcon = Resources.Load<Texture2D>("delfticon_angle"); } }

        private static GUIStyle _infoIcon;
        public static GUIStyle infoIcon {
            get {
                if (_infoIcon == null) {
                    _infoIcon = new GUIStyle(NodeEditorResources.styles.nodeHeader);
                    _infoIcon.normal.background = NodeEditorResources.dotOuter;
                }
                return _infoIcon;
            }
        }

        private static GUIStyle _activeNodeBody;
        public static GUIStyle activeNodeBody {
            get {
                if (_activeNodeBody == null) {
                    _activeNodeBody = new GUIStyle(NodeEditorResources.styles.nodeBody);
                    _activeNodeBody.normal.background = Resources.Load<Texture2D>("xnode_node_active");
                }
                return _activeNodeBody;
            }
        }

        private static GUIStyle _commentNodeBody;
        public static GUIStyle commentNodeBody {
            get {
                if (_commentNodeBody == null) {
                    _commentNodeBody = new GUIStyle(NodeEditorResources.styles.nodeBody);
                    _commentNodeBody.normal.background = Resources.Load<Texture2D>("delftnode_comment");
                }
                return _commentNodeBody;
            }
        }

        private static GUIStyle _commentTextArea;
        public static GUIStyle commentTextArea {
            get {
                if (_commentTextArea == null) {
                    _commentTextArea = new GUIStyle(EditorStyles.textArea);
                    _commentTextArea.normal.background = null;
                }
                return _commentTextArea;
            }
        }

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