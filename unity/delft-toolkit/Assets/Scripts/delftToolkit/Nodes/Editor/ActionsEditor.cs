using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeEditor(typeof(Actions))]
	public class ActionsEditor : NodeEditor {

        bool showPosition = false;

		public override void OnHeaderGUI() {
            //base.OnHeaderGUI();
			GUI.color = Color.white;
			Actions node = target as Actions;
			StateGraph graph = node.graph as StateGraph;
			if (graph.current == node) GUI.color = Color.green;
			//string title = target.name;
			//GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            string title = target.name;
            if (renaming != 0 && Selection.Contains(target)) {
                int controlID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
                if (renaming == 1) {
                    GUIUtility.keyboardControl = controlID;
                    EditorGUIUtility.editingTextField = true;
                    renaming = 2;
                }
                target.name = EditorGUILayout.TextField(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
                if (!EditorGUIUtility.editingTextField) {
                    Rename(target.name);
                    renaming = 0;
                }
            } else {
                GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            }
			GUI.color = Color.white;
            Actions.DingEvent += setAction;
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
            node.device = (AiGlobals.Devices)EditorGUILayout.EnumPopup(node.device, GUILayout.Width(50));
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

            for (int i = 0; i < node.actions.Count; i++) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(20))) {
                    node.actions.RemoveAt(i);
                    i--;
                }
                if (node.currentAction == i) GUI.color = Color.cyan;
                node.actions[i].actionType = (AiGlobals.ActionTypes)EditorGUILayout.EnumPopup(node.actions[i].actionType, GUILayout.Width(42) );
                switch (node.actions[i].actionType) {
                    case AiGlobals.ActionTypes.move:
                        ActionMove actionMove = node.actions[i].moveParams;
                        actionMove.type = (AiGlobals.ActionMoveTypes) EditorGUILayout.EnumPopup(actionMove.type, GUILayout.Width(60));
                        actionMove.time = EditorGUILayout.FloatField(actionMove.time, GUILayout.Width(24));
                        actionMove.speed = EditorGUILayout.FloatField(actionMove.speed, GUILayout.Width(24));
                        break;
                    case AiGlobals.ActionTypes.leds:
                        ActionLed actionLeds = node.actions[i].ledParams;
                        actionLeds.type = (AiGlobals.ActionLedTypes) EditorGUILayout.EnumPopup(actionLeds.type, GUILayout.Width(60));
                        actionLeds.time = EditorGUILayout.FloatField(actionLeds.time, GUILayout.Width(24));
                        actionLeds.ledNum = EditorGUILayout.IntField(actionLeds.ledNum, GUILayout.Width(24));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        actionLeds.color = EditorGUILayout.TextField(actionLeds.color, GUILayout.Width(100));
                        break;
                    case AiGlobals.ActionTypes.delay:
                        ActionDelay actionDelay = node.actions[i].delayParams;
                        actionDelay.time = EditorGUILayout.FloatField(actionDelay.time, GUILayout.Width(24));
                        //actionStopAll.type = (AiGlobals.ActionStopAllTypes) EditorGUILayout.EnumPopup(actionStopAll.type, GUILayout.Width(50));
                        break;
                    case AiGlobals.ActionTypes.analogin:
                        ActionAnalogIn actionAnalogIn = node.actions[i].analoginParams;
                        actionAnalogIn.type = (AiGlobals.ActionAnalogInTypes)EditorGUILayout.EnumPopup(actionAnalogIn.type, GUILayout.Width(60));
                        actionAnalogIn.interval = EditorGUILayout.IntField(actionAnalogIn.interval, GUILayout.Width(24));
                        actionAnalogIn.port = EditorGUILayout.IntField(actionAnalogIn.port, GUILayout.Width(24));
                        break;
                }
                //node.actions[i].seconds = EditorGUILayout.FloatField(node.actions[i].seconds, GUILayout.Width(25) );

                //NodeEditorGUILayout.PortField(new GUIContent(), target.GetOutputPort(dialogue.answers[i].portName), GUILayout.Width(-4));
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
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

        public void setAction(AiGlobals.Devices aDevice, Action anAction) {
            //Debug.LogWarning("Repainting");
            NodeEditorWindow.current.Repaint();
        }

        public override int GetWidth() {
            return 220;
        }
	}
}