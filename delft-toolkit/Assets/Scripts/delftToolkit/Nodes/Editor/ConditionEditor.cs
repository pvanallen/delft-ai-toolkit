using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Condition))]
	public class ConditionEditor : NodeEditor {

		public override void OnHeaderGUI() {
            //base.OnHeaderGUI();
            GUI.color = Color.white;
            Condition node = target as Condition;
			StateGraph graph = node.graph as StateGraph;
			if (graph.current == node) GUI.color = Color.green;
            string title = target.name;
            if (renaming != 0 && Selection.Contains(target)) {
                int controlID = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1;
                if (renaming == 1) {
                    EditorGUIUtility.keyboardControl = controlID;
                    EditorGUIUtility.editingTextField = true;
                    renaming = 2;
                }
                target.name = EditorGUILayout.TextField(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
                if (!EditorGUIUtility.editingTextField) {
                    Rename(target.name);
                    renaming = 0;
                }
            }
            else {
                GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            }
			GUI.color = Color.white;
            Actions.DingEvent += setAction;
            DingControlPhysical.DingNumPhysicalEvent += handleNumEvent;
            DingControlVirtual.DingNumVirtualEvent += handleNumEvent;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
			
            Condition condition = target as Condition;
            Condition node = target as Condition;
            StateGraph graph = node.graph as StateGraph;

            EditorGUI.BeginChangeCheck();
            node.a = GUILayout.HorizontalSlider(node.a, 0, 1023);
            node.result = EditorGUILayout.FloatField(node.result, GUILayout.Width(25));

            if (EditorGUI.EndChangeCheck()) {
                node.GetValue(node.GetOutputPort("result"));
            }

            if (GUILayout.Button("Start Actions")) {
                graph.current = node;
                //node.currentAction = 0;
                node.MoveNext();
            }
			//if (GUILayout.Button("Continue Graph")) graph.Continue();
			if (GUILayout.Button("Set as current")) graph.current = node;

		}

        public void setAction(AiGlobals.Devices aDevice, Action anAction) {
            //Debug.LogWarning("Repainting");
            NodeEditorWindow.current.Repaint();
        }

        void handleNumEvent(AiGlobals.Devices devices, string adrs, float val0, float val1, float val2) {
            NodeEditorWindow.current.Repaint();
        }
	}
}