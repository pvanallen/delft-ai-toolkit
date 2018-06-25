using System.Collections;
using System.Collections.Generic;
using Rotorz.ReorderableList;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
    [CustomNodeEditor(typeof(Actions))]
    public class ActionsEditor : NodeEditor {

        bool showPosition = false;

        public override void OnHeaderGUI() {
            GUI.color = Color.white;
            StateGraph graph = target.graph as StateGraph;
            if (graph.current == target) GUI.color = Color.green;
            base.OnHeaderGUI();
            GUI.color = Color.white;
        }

        public override void OnBodyGUI() {

            //base.OnBodyGUI();
            GUI.color = Color.white;
            Actions node = target as Actions;
            StateGraph graph = node.graph as StateGraph;
            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(target.GetInputPort("enter"), GUILayout.Width(30));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            node.device = (AiGlobals.Devices) EditorGUILayout.EnumPopup(node.device, GUILayout.Width(50));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            NodeEditorGUILayout.PortField(target.GetOutputPort("exit"), GUILayout.Width(26));
            GUILayout.EndHorizontal();

            showPosition = EditorGUILayout.Foldout(showPosition, "More", true);
            if (showPosition) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("repeat", GUILayout.Width(40));
                node.currentRepeats = EditorGUILayout.IntField(node.currentRepeats, GUILayout.Width(22));
                EditorGUILayout.LabelField("of", GUILayout.Width(15));
                node.repeats = EditorGUILayout.IntField(node.repeats, GUILayout.Width(22));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("random", GUILayout.Width(45));
                node.random = EditorGUILayout.Toggle(node.random);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.EndVertical();

            SerializedProperty p = serializedObject.FindProperty("actions");
            Rotorz.ReorderableList.ReorderableListGUI.Title("Actions");
            Rotorz.ReorderableList.ReorderableListGUI.ListField(p);

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("+", GUILayout.Width(25))) {
                Debug.LogWarning("start add new action");
                node.actions.Add(new Action());
                Debug.LogWarning("add new action");
            }
            GUILayout.EndHorizontal();

            //node.seconds = GUILayout.HorizontalSlider(node.seconds, 0, 10);
            if (GUILayout.Button("Start Actions")) {
                graph.current = node;
                node.currentAction = 0;
                node.currentRepeats = 1;
                node.NextAction().RunCoroutine();
            }
            //if (GUILayout.Button("Continue Graph")) graph.Continue();
            if (GUILayout.Button("Set as current")) graph.current = node;
        }

        [InitializeOnLoadMethod]
        private static void ASDF() {
            Actions.DingEvent -= SetAction;
            Actions.DingEvent += SetAction;
        }

        //public IEnumerator GoNext(Actions node, float delay ) {
        //	node.Finish (delay).RunCoroutine ();
        //	yield return new WaitForSeconds(delay+0.01f);
        //	NodeEditorWindow.current.Repaint ();
        //}

        //public void Awake()
        //{
        //    Debug.LogWarning("onEnable");
        //    ActionNode.DingEvent += setAction;
        //}

        //public void OnDestroy()
        //{
        //    Debug.LogWarning("onDisable");
        //    ActionNode.DingEvent -= setAction;
        //}

        public static void SetAction(AiGlobals.Devices aDevice, Action anAction) {
            //Debug.LogWarning("Repainting");
            NodeEditorWindow.current.Repaint();
        }
    }
}